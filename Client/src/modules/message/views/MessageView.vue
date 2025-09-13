<template>
  <div class="flex md:flex-row flex-col h-full">
    <!-- 中间内容 -->
    <div class="divider-vertical"></div>
    <div class="center" :style="{ width: centerWidth + '%' }">
      <div class="message-search">
        <SearchInput v-model="searchText" placeholder="🔍搜索" />
      </div>
      <div class="divider-horizontal"></div>

      <!-- 搜索结果 -->
      <div class="message-list">
        <div v-if="loading" class="loading">加载中...</div>
        <div v-else-if="error" class="error">{{ error }}</div>
        <div v-else-if="!searchText.trim()">
          <!-- 无搜索时显示所有会话 -->
          <div v-if="conversationList.length === 0" class="empty">暂无会话</div>
          <div v-else>
            <Conversation
              v-for="item in conversationList"
              :key="item.otherUserId"
              :conversation="item"
              :selected="selectedConversation?.otherUserId === item.otherUserId"
              @click="handleConversationSelect(item)"
            />
          </div>
        </div>
        <div v-else>
          <!-- 有搜索时显示分类结果 -->
          <div
            v-if="searchResults.conversations.length === 0 && searchResults.messages.length === 0"
            class="empty"
          >
            未找到匹配的内容
          </div>
          <div v-else>
            <!-- 联系人搜索结果 -->
            <div v-if="searchResults.conversations.length > 0" class="search-section">
              <div class="search-section-title">联系人</div>
              <Conversation
                v-for="item in searchResults.conversations"
                :key="'conv-' + item.otherUserId"
                :conversation="item"
                :selected="selectedConversation?.otherUserId === item.otherUserId"
                :search-term="searchText.trim()"
                @click="handleConversationSelect(item)"
              />
            </div>

            <!-- 聊天记录搜索结果 -->
            <div v-if="searchResults.messages.length > 0" class="search-section">
              <div class="search-section-title">聊天记录</div>
              <div
                v-for="(result, index) in searchResults.messages"
                :key="'msg-' + index"
                class="message-search-result"
                @click="handleConversationSelect(result.conversation)"
              >
                <div class="message-search-header">
                  <img
                    v-if="!!result.conversation.contactUser?.avatar"
                    :src="result.conversation.contactUser?.avatar"
                    alt="user"
                  />
                  <PlaceHolder
                    v-else
                    width="100"
                    height="100"
                    :text="
                      result.conversation.contactUser?.nickname ||
                      `${result.conversation.OtherUserId}`
                    "
                    class="w-12 h-12 rounded-full"
                  />
                  <div class="message-search-info">
                    <span class="message-search-name">{{
                      result.conversation.contactUser?.nickname
                    }}</span>
                    <span class="message-search-time">{{ result.message?.time }}</span>
                  </div>
                </div>
                <div
                  class="message-search-content"
                  v-html="highlightSearchTerm(result.message?.content || '', searchText.trim())"
                ></div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="divider-horizontal"></div>
    </div>
    <div class="resizer" @mousedown="startResize" :class="{ resizing: isResizing }"></div>

    <div class="divider-vertical"></div>

    <div class="right" :style="{ width: rightWidth + '%' }">
      <div class="divider-horizontal"></div>
      <div class="chat-header">
        <ConservationHeader v-if="selectedConversation" :conversation="selectedConversation" />
      </div>
      <div class="divider-horizontal"></div>
      <!-- 聊天窗口 -->
      <div class="chat-window bg-slate-800" :style="{ height: chatWindowHeight + '%' }">
        <div class="chat-content">
          <!-- 调试信息 -->
          <div v-if="false" class="debug-info" style="color: red; font-size: 12px; padding: 5px">
            消息数量: {{ currentChatHistory.length }} | 原始数量: {{ currentChatMessages.length }}
          </div>
          <ChatMessage
            v-for="message in currentChatHistory"
            :key="message.messageId"
            :message="message"
            :isSelf="message.sender.id === myUserId"
            :myUserId="myUserId"
            @messageAction="handleMessageAction"
          />
        </div>
      </div>

      <div
        class="horizontal-resizer"
        @mousedown="startHorizontalResize"
        :class="{ resizing: isHorizontalResizing }"
      ></div>
      <div class="divider-horizontal"></div>

      <!-- 聊天输入框 -->
      <div class="chat-input bg-slate-900" :style="{ height: chatInputHeight + '%' }">
        <ChatInput @sendMessage="handleSendMessage" />
      </div>
      <div class="divider-horizontal"></div>
    </div>
    <div class="divider-vertical"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, nextTick, onMounted, watch, triggerRef } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import SearchInput from '../components/SearchInput.vue'
import Conversation from '../components/Conversation.vue'
import ConservationHeader from '../components/ConservationHeader.vue'
import ChatInput from '../components/ChatInput.vue'
import ChatMessage from '../components/ChatMessage.vue'
import type { conversation, chatHistory, user, message, messageDisplay } from '../types'
import {
  getConversationList,
  getChatHistory,
  sendMessage,
  markMessagesRead,
  getUserDetail,
} from '../api'
import { User } from '@/modules/auth/public.ts'
import { highlightSearchTerm, createDebounceSearch } from '../utils/search'
import { copyMessageContent } from '../utils/message'
import { ensureUser, userCache } from '../utils/user'
import { convertMessagesToDisplay, sortConversationsByTime } from '../utils/data'
import { GATEWAY } from '@/modules/core/public.ts'
import PlaceHolder from '@/modules/user/components/PlaceHolder.vue'

const router = useRouter()
const route = useRoute()
const searchText = ref('')
const conversationListData = ref<conversation[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

// 拖动分割线相关状态
const centerWidth = ref(22) // 中间面板宽度百分比
const rightWidth = ref(78) // 右侧面板宽度百分比
const isResizing = ref(false)

// 水平拖动相关状态
const chatWindowHeight = ref(64) // 聊天窗口高度百分比
const chatInputHeight = ref(28) // 聊天输入框高度百分比
const isHorizontalResizing = ref(false)

// 用户信息缓存已迁移到 utils/user.ts

// 搜索结果类型
interface SearchResult {
  type: 'conversation' | 'message'
  conversation: conversation
  message?: {
    content: string
    time: string
    sender: string
  }
  relevance: number // 相关性评分
}

// 搜索防抖
const debouncedSearchText = ref('')

// 防抖搜索函数已迁移到 utils/search.ts
const debounceSearch = createDebounceSearch((text: string) => {
  debouncedSearchText.value = text
}, 300)

// 监听搜索文本变化
watch(searchText, (newText) => {
  debounceSearch(newText)
})

// 统一搜索结果
const searchResults = computed(() => {
  if (!debouncedSearchText.value.trim()) {
    return {
      conversations: conversationList.value,
      messages: [],
    }
  }

  const searchTerm = debouncedSearchText.value.toLowerCase().trim()
  const results: SearchResult[] = []

  // 搜索会话（联系人）
  conversationList.value.forEach((conv) => {
    const user = conv.contactUser
    if (!user) return

    let relevance = 0

    // 用户名完全匹配得分最高
    if (user.nickname.toLowerCase() === searchTerm) {
      relevance = 100
    } else if (user.nickname.toLowerCase().startsWith(searchTerm)) {
      relevance = 80
    } else if (user.nickname.toLowerCase().includes(searchTerm)) {
      relevance = 60
    }

    // 最新消息匹配
    if (conv.newestMessage && conv.newestMessage.toLowerCase().includes(searchTerm)) {
      relevance = Math.max(relevance, 40)
    }

    if (relevance > 0) {
      results.push({
        type: 'conversation',
        conversation: conv,
        relevance,
      })
    }
  })

  // 搜索聊天记录 - 暂时禁用，等待API支持
  // TODO: 实现基于API的聊天记录搜索功能

  // 按相关性排序
  results.sort((a, b) => b.relevance - a.relevance)

  return {
    conversations: results.filter((r) => r.type === 'conversation').map((r) => r.conversation),
    messages: results.filter((r) => r.type === 'message'),
  }
})

// 过滤后的会话列表（保持向后兼容）
const filteredConversationList = computed(() => {
  return searchResults.value.conversations
})

// 按时间排序的会话列表，最新的在前面
const conversationList = computed(() => {
  return sortConversationsByTime(conversationListData.value)
})

// 聊天记录
const chatHistoryList = ref<chatHistory[]>([])
const currentChatMessages = ref<messageDisplay[]>([])

// 用户信息
const myUser = ref<user>({
  id: 0,
  nickname: 'Loading...',
  avatar: '',
  url: '/user/0',
})

// 当前用户ID
const myUserId = computed(() => {
  const user = User.getInstance()
  return user?.userAuth?.userId ? parseInt(user.userAuth.userId) : 0
})

// 初始化用户信息
onMounted(async () => {
  const user = User.getInstance()

  if (user?.userAuth?.userId) {
    const userId = parseInt(user.userAuth.userId)

    if (userId > 0) {
      try {
        const userDetail = await getUserDetail(userId)
        myUser.value = {
          id: userDetail.userId,
          nickname: userDetail.nickname || userDetail.username,
          avatar: `${GATEWAY}/api/media/${userDetail.avatar}` || '',
          url: `/user/${userDetail.userId}`,
        }
      } catch (error) {
        // 使用默认用户信息
        myUser.value = {
          id: userId,
          nickname: userId.toString(),
          avatar: '',
          url: `/user/${userId}`,
        }
      }
    }
  }
})

// 高亮搜索关键词已迁移到 utils/search.ts

// 选中的会话
const selectedConversation = ref<conversation | null>(null)

// 获取会话列表
const fetchConversationList = async () => {
  try {
    loading.value = true
    error.value = null

    const apiConversations = await getConversationList()

    if (apiConversations.length > 0) {
      // 并行获取所有会话的对端用户信息，并填充到 contactUser
      const filled = await Promise.all(
        apiConversations.map(async (conv) => {
          const contact = await ensureUser(conv.otherUserId)
          return {
            ...conv,
            contactUser: contact,
            newestMessage: conv.lastMessage?.content || '',
            time: conv.lastMessage?.sentAt || new Date().toISOString(),
          }
        })
      )
      conversationListData.value = filled
    } else {
      conversationListData.value = []
    }
  } catch (err) {
    error.value = '获取会话列表失败'
  } finally {
    loading.value = false
  }
}

// 初始化选中第一个会话
const initializeSelectedConversation = () => {
  if (conversationList.value.length > 0) {
    selectedConversation.value = conversationList.value[0]
  }
}

// 在组件挂载时初始化
onMounted(async () => {
  // 加载本人资料，确保聊天窗口我的头像与其他位置一致
  const currentUserId = myUserId.value
  if (currentUserId > 0) {
    try {
      const me = await ensureUser(currentUserId)
      myUser.value = me
    } catch (e) {
      // 忽略头像失败，使用占位
    }
  }
  await fetchConversationList()
  // 如果路由带有 userId，则优先选中该会话
  const routeUserId = Number(route.params.userId)
  if (routeUserId && !Number.isNaN(routeUserId)) {
    await selectConversationByUserId(routeUserId)
  } else {
    initializeSelectedConversation()
  }
})

// 根据路由选择会话
async function selectConversationByUserId(otherUserId: number) {
  let conv = conversationListData.value.find((c) => c.otherUserId === otherUserId)
  if (!conv) {
    // 若列表中暂无该会话，构造一个占位会话并填充用户信息
    const contact = await ensureUser(otherUserId)
    conv = {
      otherUserId: otherUserId,
      lastMessage: {
        messageId: 0,
        senderId: otherUserId,
        receiverId: myUser.value.id,
        content: '',
        sentAt: new Date().toISOString(),
        isRead: true,
      },
      unreadCount: 0,
      contactUser: contact,
      newestMessage: '',
      time: new Date().toISOString(),
    }
    conversationListData.value.push(conv)
  }
  selectedConversation.value = conv
  await fetchChatHistory(otherUserId)
}

// 监听路由变化
watch(
  () => route.params.userId,
  async (val) => {
    const uid = Number(val)
    if (uid && !Number.isNaN(uid)) {
      await selectConversationByUserId(uid)
    }
  }
)

// 获取聊天记录
const fetchChatHistory = async (userId: number) => {
  try {
    // 移除加载状态，避免点击会话时的加载动画
    // loading.value = true
    error.value = null
    // 首先尝试从API获取聊天记录
    let messages = await getChatHistory(userId)

    // 预取涉及到的用户信息（发送者/接收者）
    const ids = new Set<number>()
    messages.forEach((m) => {
      ids.add(m.senderId)
      ids.add(m.receiverId)
    })
    await Promise.all(Array.from(ids).map((id) => ensureUser(id)))

    // 转换API数据格式为前端显示格式
    const displayMessages = convertMessagesToDisplay(messages, userCache)
    currentChatMessages.value = displayMessages

    // 同步更新会话列表的最新消息（以最新一条消息为准）
    if (displayMessages.length > 0) {
      const latest = displayMessages[displayMessages.length - 1]
      const conv = conversationListData.value.find((c) => c.otherUserId === userId)
      if (conv) {
        conv.newestMessage = latest.content
        conv.time = latest.sendTime
        conv.lastMessage = {
          messageId: latest.messageId,
          senderId: latest.sender.id,
          receiverId: latest.receiver.id,
          content: latest.content,
          sentAt: latest.sendTime,
          isRead: latest.isRead,
        }
      }
    }
  } catch (err) {
    error.value = '获取聊天记录失败'
  }
}

// 切换选中会话
async function handleConversationSelect(item: conversation) {
  selectedConversation.value = item
  // 标记消息已读
  try {
    await markMessagesRead(item.otherUserId)
    item.unreadCount = 0
  } catch (err) {
    return
  }
  // 获取聊天记录
  await fetchChatHistory(item.otherUserId)
  // 路由跳转到子路径 /message/:userId（避免依赖命名路由）
  router.push({ path: `/message/${item.otherUserId}` }).catch(() => {})
}

// 当前会话的聊天记录（使用缓存进行用户标准化）
const currentChatHistory = computed(() => {
  const result = currentChatMessages.value.map((m) => {
    // 优先使用消息对象中已有的用户信息，避免依赖非响应式的userCache
    const sender = m.sender ||
      userCache.get(m.sender.id) || { id: m.senderId, nickname: '未知用户', avatar: '', url: '' }
    const receiver = m.receiver ||
      userCache.get(m.receiver.id) || {
        id: m.receiverId,
        nickname: '未知用户',
        avatar: '',
        url: '',
      }
    return { ...m, sender, receiver }
  })
  return result
})

async function handleSendMessage(content: string, type: 'text' | 'image') {
  if (!selectedConversation.value) return

  try {
    // 发送消息到服务器
    const result = await sendMessage({
      receiverId: selectedConversation.value.otherUserId,
      content: content,
      type: type,
    })

    if (result.success) {
      console.log('[发送消息] 消息发送成功，重新获取聊天记录')

      // 重新获取聊天记录以确保数据同步
      await fetchChatHistory(selectedConversation.value.otherUserId)

      // 重新获取会话列表以确保数据同步
      await fetchConversationList()

      // 滚动到最新消息
      await nextTick(() => {
        const chatWindow = document.querySelector('.chat-window') as HTMLElement
        if (chatWindow) {
          chatWindow.scrollTop = chatWindow.scrollHeight
        }
      })

      console.log('[发送消息] ✅ 消息发送成功，聊天记录和会话列表已更新')
    }
  } catch (err) {
    error.value = '发送消息失败'
  }
}

// 处理消息操作
const handleMessageAction = async (action: string, message: messageDisplay) => {
  switch (action) {
    case 'copy':
      await handleMessageCopy(message)
      break
    case 'report':
      const to = router.resolve({
        name: 'report',
        params: { type: 'message', id: message.messageId },
        query: {
          reporterId: myUserId.value,
          reportedId: message.sender.id,
          reportTime: new Date().toISOString(),
        },
      })
      const url = new URL(to.href, window.location.origin) // 保证绝对地址
      window.open(url.toString(), '_blank', 'noopener,noreferrer')
      break
  }
}

// 复制消息内容已迁移到 utils/message.ts
const handleMessageCopy = async (message: messageDisplay) => {
  await copyMessageContent(message)
}

// 渲染消息内容已迁移到 utils/message.ts

// 拖动分割线处理函数
const startResize = (e: MouseEvent) => {
  isResizing.value = true
  document.body.style.cursor = 'col-resize'
  document.body.style.userSelect = 'none'

  const startX = e.clientX
  const startCenterWidth = centerWidth.value
  const startRightWidth = rightWidth.value

  const handleMouseMove = (e: MouseEvent) => {
    const deltaX = e.clientX - startX
    const containerWidth = document.querySelector('.page-content-wrapper')?.clientWidth || 1000
    const deltaPercent = (deltaX / containerWidth) * 100

    // 计算新的宽度
    let newCenterWidth = startCenterWidth + deltaPercent
    let newRightWidth = startRightWidth - deltaPercent

    // 限制最小宽度
    const minWidth = 15 // 最小15%
    const maxWidth = 60 // 最大60%

    if (newCenterWidth < minWidth) {
      newCenterWidth = minWidth
      newRightWidth = 100 - minWidth - 20 // 20%是其他元素占用的空间
    } else if (newCenterWidth > maxWidth) {
      newCenterWidth = maxWidth
      newRightWidth = 100 - maxWidth - 20
    } else if (newRightWidth < minWidth) {
      newRightWidth = minWidth
      newCenterWidth = 100 - minWidth - 20
    }

    centerWidth.value = newCenterWidth
    rightWidth.value = newRightWidth
  }

  const handleMouseUp = () => {
    isResizing.value = false
    document.body.style.cursor = ''
    document.body.style.userSelect = ''
    document.removeEventListener('mousemove', handleMouseMove)
    document.removeEventListener('mouseup', handleMouseUp)
  }

  document.addEventListener('mousemove', handleMouseMove)
  document.addEventListener('mouseup', handleMouseUp)
}

// 水平拖动分割线处理函数
const startHorizontalResize = (e: MouseEvent) => {
  isHorizontalResizing.value = true
  document.body.style.cursor = 'row-resize'
  document.body.style.userSelect = 'none'

  const startY = e.clientY
  const startWindowHeight = chatWindowHeight.value
  const startInputHeight = chatInputHeight.value

  const handleMouseMove = (e: MouseEvent) => {
    const deltaY = e.clientY - startY
    const containerHeight = document.querySelector('.right')?.clientHeight || 600
    const deltaPercent = (deltaY / containerHeight) * 100

    // 计算新的高度
    let newWindowHeight = startWindowHeight + deltaPercent
    let newInputHeight = startInputHeight - deltaPercent

    // 限制最小高度
    const minWindowHeight = 50 // 最小50%（聊天窗口最小高度）
    const maxWindowHeight = 85 // 最大85%（聊天窗口最大高度）
    const minInputHeight = 20 // 最小35%（确保发送按钮可见）
    const maxInputHeight = 30 // 最大40%（输入框最大高度）

    if (newWindowHeight < minWindowHeight) {
      newWindowHeight = minWindowHeight
      newInputHeight = 100 - minWindowHeight - 8 // 8%是头部占用的空间
    } else if (newWindowHeight > maxWindowHeight) {
      newWindowHeight = maxWindowHeight
      newInputHeight = 100 - maxWindowHeight - 8
    } else if (newInputHeight < minInputHeight) {
      newInputHeight = minInputHeight
      newWindowHeight = 100 - minInputHeight - 8
    } else if (newInputHeight > maxInputHeight) {
      newInputHeight = maxInputHeight
      newWindowHeight = 100 - maxInputHeight - 8
    }

    chatWindowHeight.value = newWindowHeight
    chatInputHeight.value = newInputHeight
  }

  const handleMouseUp = () => {
    isHorizontalResizing.value = false
    document.body.style.cursor = ''
    document.body.style.userSelect = ''
    document.removeEventListener('mousemove', handleMouseMove)
    document.removeEventListener('mouseup', handleMouseUp)
  }

  document.addEventListener('mousemove', handleMouseMove)
  document.addEventListener('mouseup', handleMouseUp)
}
</script>


<style scoped>
.center {
  min-width: 200px;
  display: flex;
  flex-direction: column;
  overflow-wrap: break-word;
  word-break: break-word;
}

.message-search {
  flex: 8;
  display: flex;
  align-items: center;
  justify-content: center;
}

.message-list {
  flex: 84;
}

.right {
  display: flex;
  flex-direction: column;
}

.chat-header {
  height: 10%;
  display: flex;
  align-items: center;
}

.chat-window {
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  background: linear-gradient(135deg, #1f2937 0%, #485a60 100%);
}

.chat-content {
  padding: 12px 8px;
  flex: 1;
}

.divider-horizontal {
  width: 100%;
  border-bottom: 1px solid #323345;
}

.divider-vertical {
  width: 1px;
  background-color: #444c5c;
}

/* 拖动分割线样式 */
.resizer {
  width: 1px;
  background-color: #444c5c;
  cursor: col-resize;
  position: relative;
  transition: background-color 0.2s ease;
}

.resizer:hover {
  background-color: #5a6478;
}

.resizer.resizing {
  background-color: #4a9eff;
}

.resizer::before {
  content: '';
  position: absolute;
  top: 0;
  left: -2px;
  right: -2px;
  bottom: 0;
  background-color: transparent;
}

/* 水平拖动分割线样式 */
.horizontal-resizer {
  height: 1px;
  background-color: #444c5c;
  cursor: row-resize;
  position: relative;
  transition: background-color 0.2s ease;
}

.horizontal-resizer:hover {
  background-color: #5a6478;
}

.horizontal-resizer.resizing {
  background-color: #4a9eff;
}

.horizontal-resizer::before {
  content: '';
  position: absolute;
  top: -2px;
  left: 0;
  right: 0;
  bottom: -2px;
  background-color: transparent;
}

.loading,
.error,
.empty {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  color: #666;
  font-size: 14px;
}

.error {
  color: #e74c3c;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .center {
    width: 25%;
    min-width: 180px;
  }

  .right {
    width: 55%;
  }
}

@media (max-width: 768px) {
  .center {
    width: 100% !important;
    min-width: unset;
    height: 40vh;
  }

  .right {
    width: 100% !important;
    height: 60vh;
  }

  .resizer {
    display: none;
  }

  .horizontal-resizer {
    display: none;
  }

  .chat-header {
    height: 10% !important;
  }

  .chat-window {
    height: 68% !important;
  }

  .chat-input {
    height: 22% !important;
  }

  .divider-vertical {
    width: 100%;
    height: 1px;
    background-color: #444c5c;
  }
}

@media (max-width: 480px) {
  .center {
    height: 35vh;
  }

  .right {
    height: 65vh;
  }
}

/* 搜索结果分类样式 */
.search-section {
  margin-bottom: 16px;
}

.search-section-title {
  font-size: 14px;
  font-weight: bold;
  color: #6b7280;
  padding: 8px 16px;
  background-color: #f9fafb;
  border-bottom: 1px solid #e5e7eb;
}

/* 聊天记录搜索结果样式 */
.message-search-result {
  padding: 12px 16px;
  cursor: pointer;
  transition: background-color 0.2s;
  border-bottom: 1px solid #f3f4f6;
}

.message-search-result:hover {
  background-color: #f9fafb;
}

.message-search-header {
  display: flex;
  align-items: center;
  margin-bottom: 8px;
}

.message-search-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  object-fit: cover;
  margin-right: 12px;
}

.message-search-info {
  flex: 1;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.message-search-name {
  font-weight: bold;
  font-size: 14px;
  color: #374151;
}

.message-search-time {
  font-size: 12px;
  color: #9ca3af;
}

.message-search-content {
  font-size: 14px;
  color: #6b7280;
  line-height: 1.4;
  margin-left: 44px;
}

/* 搜索高亮样式 */
:deep(.highlight) {
  background-color: #fef3c7;
  color: #92400e;
  padding: 1px 2px;
  border-radius: 2px;
  font-weight: bold;
}
</style>
