<template>
  <div class="page-content-wrapper">
    <!-- 中间内容 -->
    <div class="divider-vertical"></div>
    <div class="center">
      <div class="divider-horizontal"></div>
      <div class="message-heading">
        <h2>私信</h2>
      </div>
      <div class="divider-horizontal"></div>
      <div class="message-search">
        <SearchInput v-model="searchText" placeholder="🔍搜索" />
      </div>
      <div class="divider-horizontal"></div>
      <!-- 会话列表 -->
      <div class="message-list">
        <div v-if="loading" class="loading">加载中...</div>
        <div v-else-if="error" class="error">{{ error }}</div>
        <div v-else-if="conversationList.length === 0" class="empty">暂无会话</div>
        <div v-else>
          <Conversation
            v-for="item in conversationList"
            :key="item.OtherUserId"
            :conversation="item"
            :selected="selectedConversation?.OtherUserId === item.OtherUserId"
            @click="handleConversationSelect(item)"
          />
        </div>
      </div>
      <div class="divider-horizontal"></div>
    </div>
    <div class="divider-vertical"></div>
    <div class="right">
      <div class="divider-horizontal"></div>
      <div class="chat-header">
        <ConservationHeader v-if="selectedConversation" :conversation="selectedConversation" />
      </div>
      <div class="divider-horizontal"></div>
      <!-- 聊天窗口 -->
      <div class="chat-window">
        <div class="chat-content">
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
      <div class="divider-horizontal"></div>
      <!-- 聊天输入框 -->
      <div class="chat-input">
        <ChatInput @sendMessage="handleSendMessage" />
      </div>
      <div class="divider-horizontal"></div>
    </div>
    <div class="divider-vertical"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, nextTick, onMounted, watch } from 'vue'
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

const router = useRouter()
const route = useRoute()
const searchText = ref('')
const conversationListData = ref<conversation[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

// 用户信息缓存，避免重复请求
const userCache = new Map<number, user>()

function getDefaultAvatar(): string {
  return 'https://placehold.co/100x100/facc15/78350f?text=U'
}

async function ensureUser(userId: number): Promise<user> {
  const cached = userCache.get(userId)
  if (cached) return cached
  const detail = await getUserDetail(userId)
  const u: user = {
    id: detail.userId,
    nickname: detail.nickname || detail.username,
    avatar: detail.avatar || getDefaultAvatar(),
    url: `/user/${detail.userId}`,
  }
  userCache.set(userId, u)
  return u
}

// 按时间排序的会话列表，最新的在前面
const conversationList = computed(() => {
  return [...conversationListData.value].sort((a, b) => {
    const timeA = new Date(a.lastMessage?.SendAt || a.time || 0).getTime()
    const timeB = new Date(b.lastMessage?.SendAt || b.time || 0).getTime()
    return timeB - timeA // 降序排列，新消息在前
  })
})

// 聊天记录
const chatHistoryList = ref<chatHistory[]>([])
const currentChatMessages = ref<messageDisplay[]>([])

// 用户信息
const myUser = ref<user>({
  id: 0,
  nickname: 'Loading...',
  avatar: 'https://placehold.co/100x100/facc15/78350f?text=L',
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
    try {
      const userDetail = await getUserDetail(parseInt(user.userAuth.userId))
      myUser.value = {
        id: userDetail.userId,
        nickname: userDetail.nickname || userDetail.username,
        avatar: userDetail.avatar || 'https://placehold.co/100x100/facc15/78350f?text=U',
        url: `/user/${userDetail.userId}`,
      }
    } catch (error) {
      console.error('获取用户信息失败:', error)
    }
  }
})

// 选中的会话
const selectedConversation = ref<conversation | null>(null)

// 获取会话列表
const fetchConversationList = async () => {
  try {
    loading.value = true
    error.value = null
    const data = await getConversationList()
    // 并行获取所有会话的对端用户信息，并填充到 contactUser
    const filled = await Promise.all(
      data.map(async (conv) => {
        const contact = await ensureUser(conv.OtherUserId)
        return {
          ...conv,
          contactUser: contact,
          newestMessage: conv.lastMessage?.Content || '',
          time: conv.lastMessage?.SendAt || new Date().toISOString(),
        }
      })
    )
    conversationListData.value = filled
    // 调试：输出会话列表中的用户信息
    console.log(
      '[MessageView] conversationList contactUser:',
      conversationListData.value.map((c) => ({
        id: c.contactUser?.id,
        nickname: c.contactUser?.nickname,
        avatar: c.contactUser?.avatar,
        url: c.contactUser?.url,
      }))
    )
  } catch (err) {
    error.value = '获取会话列表失败'
    console.error('获取会话列表失败:', err)
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
  try {
    const me = await ensureUser(myUser.value.id)
    myUser.value = me
  } catch (e) {
    // 忽略头像失败，使用占位
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
  let conv = conversationListData.value.find((c) => c.OtherUserId === otherUserId)
  if (!conv) {
    // 若列表中暂无该会话，构造一个占位会话并填充用户信息
    const contact = await ensureUser(otherUserId)
    conv = {
      OtherUserId: otherUserId,
      lastMessage: {
        MessageId: 0,
        SenderId: otherUserId,
        ReceiverId: myUser.value.id,
        Content: '',
        SendAt: new Date().toISOString(),
        IsRead: true,
      },
      UnreadCount: 0,
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
    loading.value = true
    error.value = null
    const messages = await getChatHistory(userId)
    // 预取涉及到的用户信息（发送者/接收者）
    const ids = new Set<number>()
    messages.forEach((m) => {
      ids.add(m.SenderId)
      ids.add(m.ReceiverId)
    })
    await Promise.all(Array.from(ids).map((id) => ensureUser(id)))

    // 转换API数据格式为前端显示格式
    const displayMessages: messageDisplay[] = messages.map((msg) => ({
      MessageId: msg.MessageId,
      SenderId: msg.SenderId,
      ReceiverId: msg.ReceiverId,
      Content: msg.Content,
      SendAt: msg.SendAt,
      IsRead: msg.IsRead,
      messageId: msg.MessageId,
      content: msg.Content,
      sendTime: msg.SendAt,
      sender: userCache.get(msg.SenderId)!,
      receiver: userCache.get(msg.ReceiverId)!,
      isRead: msg.IsRead,
      type: 'text' as const,
    }))
    currentChatMessages.value = displayMessages

    // 同步更新会话列表的最新消息（以最新一条消息为准）
    if (displayMessages.length > 0) {
      const latest = displayMessages[displayMessages.length - 1]
      const conv = conversationListData.value.find((c) => c.OtherUserId === userId)
      if (conv) {
        conv.newestMessage = latest.content
        conv.time = latest.sendTime
        conv.lastMessage = {
          MessageId: latest.messageId,
          SenderId: latest.sender.id,
          ReceiverId: latest.receiver.id,
          Content: latest.content,
          SendAt: latest.sendTime,
          IsRead: latest.isRead,
        }
      }
    }
    // 调试：输出聊天记录中的 sender/receiver 信息
    console.log(
      '[MessageView] chatHistory users:',
      currentChatMessages.value.map((m) => ({
        sender: { id: m.sender.id, nickname: m.sender.nickname, avatar: m.sender.avatar },
        receiver: { id: m.receiver.id, nickname: m.receiver.nickname, avatar: m.receiver.avatar },
      }))
    )
  } catch (err) {
    error.value = '获取聊天记录失败'
    console.error('获取聊天记录失败:', err)
  } finally {
    loading.value = false
  }
}

// 切换选中会话
async function handleConversationSelect(item: conversation) {
  selectedConversation.value = item
  // 标记消息已读
  try {
    await markMessagesRead(item.OtherUserId)
    item.UnreadCount = 0
  } catch (err) {
    console.error('标记消息已读失败:', err)
  }
  // 获取聊天记录
  await fetchChatHistory(item.OtherUserId)
  // 路由跳转到子路径 /message/:userId（避免依赖命名路由）
  router.push({ path: `/message/${item.OtherUserId}` }).catch(() => {})
}

// 当前会话的聊天记录（使用缓存进行用户标准化）
const currentChatHistory = computed(() => {
  return currentChatMessages.value.map((m) => {
    const sender = userCache.get(m.sender.id) || m.sender
    const receiver = userCache.get(m.receiver.id) || m.receiver
    return { ...m, sender, receiver }
  })
})

async function handleSendMessage(content: string, type: 'text' | 'image') {
  if (!selectedConversation.value) return

  try {
    // 发送消息到服务器
    const result = await sendMessage({
      receiverId: selectedConversation.value.OtherUserId,
      content: content,
      type: type,
    })

    if (result.success) {
      // 确保接收者用户信息
      const receiverUser = await ensureUser(selectedConversation.value.OtherUserId)
      // 创建新消息对象用于前端显示
      const newMessage: messageDisplay = {
        MessageId: result.messageId,
        SenderId: myUser.value.id,
        ReceiverId: selectedConversation.value.OtherUserId,
        Content: content,
        SendAt: new Date().toISOString(),
        IsRead: false,
        messageId: result.messageId,
        content: content,
        sendTime: new Date().toLocaleString(),
        sender: myUser.value,
        receiver: receiverUser,
        isRead: false,
        type: type,
      }

      // 添加到当前聊天记录
      currentChatMessages.value.push(newMessage)

      // 更新会话列表中的最新消息
      const originalConversation = conversationListData.value.find(
        (c) => c.OtherUserId === selectedConversation.value?.OtherUserId
      )
      if (originalConversation) {
        originalConversation.lastMessage = {
          MessageId: result.messageId,
          SenderId: myUser.value.id,
          ReceiverId: selectedConversation.value.OtherUserId,
          Content: content,
          SendAt: new Date().toISOString(),
          IsRead: false,
        }
        // 同时更新前端显示字段
        originalConversation.newestMessage = content
        originalConversation.time = new Date().toISOString()
      }

      // 滚动到最新消息
      nextTick(() => {
        const chatWindow = document.querySelector('.chat-window') as HTMLElement
        if (chatWindow) {
          chatWindow.scrollTop = chatWindow.scrollHeight
        }
      })
    }
  } catch (err) {
    error.value = '发送消息失败'
    console.error('发送消息失败:', err)
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

// 复制消息内容
const handleMessageCopy = async (message: messageDisplay) => {
  try {
    let textToCopy = ''

    if (message.type === 'text') {
      // 移除HTML标签，获取纯文本
      const tempDiv = document.createElement('div')
      tempDiv.innerHTML = renderContent(message.content)
      textToCopy = tempDiv.textContent || tempDiv.innerText || ''
    } else if (message.type === 'image') {
      textToCopy = message.content // 图片URL
    }

    await navigator.clipboard.writeText(textToCopy)
  } catch (error) {
    console.error('复制失败:', error)
  }
}

// 渲染消息内容（从ChatMessage组件复制）
function renderContent(content: string) {
  let html = content.replace(/\[emoji:(\w+)\]/g, (_match, p1) => {
    return `<img src="/emoji/${p1}.png" alt="${p1}" class="emoji-img" />`
  })
  return html
}
</script>


<style scoped>
.page-content-wrapper {
  display: flex;
  flex-direction: row;
  padding: 20px 0;
  min-height: calc(100vh - 40px);
}

.center {
  width: 22%;
  display: flex;
  flex-direction: column;
  overflow-wrap: break-word;
  word-break: break-word;
}

.message-heading {
  flex: 1;
  display: flex;
  align-items: center;
  font-size: 20px;
  font-weight: bold;
  padding-left: 32px;
}

.message-search {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}

.message-list {
  flex: 11;
}

.right {
  width: 58%;
  display: flex;
  flex-direction: column;
}

.chat-header {
  height: 8%;
  display: flex;
  align-items: center;
}

.chat-window {
  height: 70%;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
}

.chat-content {
  padding: 12px 8px, 12px 8px;
  flex: 1;
}

.chat-input {
  height: 22%;
}

.divider-horizontal {
  width: 100%;
  border-bottom: 1px solid #444c5c;
}

.divider-vertical {
  width: 1px;
  background-color: #444c5c;
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
</style>