<template>
  <div class="page-content-wrapper">

    <!-- 中间内容 -->
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
        <div>
          <Conversation
            v-for="item in conversationList"
            :key="item.contactUser.id"
            :conversation="item"
            :selected="selectedConversation?.contactUser.id === item.contactUser.id"
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
import { ref, computed, nextTick, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import SearchInput from '../components/SearchInput.vue'
import Conversation from '../components/Conversation.vue'
import ConservationHeader from '../components/ConservationHeader.vue'
import ChatInput from '../components/ChatInput.vue'
import ChatMessage from '../components/ChatMessage.vue'
import ReportDialog from '../../report/views/ReportDialog.vue'
import type { conversation, chatHistory, user, message } from '../types'

const router = useRouter()
const searchText = ref('')
const conversationListData = ref<conversation[]>([
  {
    contactUser: {
      id: 2,
      nickname: '小明',
      avatar: 'https://placehold.co/100x100/ec4899/831843?text=B',
      url: '/user/1',
    },
    newestMessage: 'test message',
    time: '2024-06-01',
    unreadMessageCount: 2,
  },
  {
    contactUser: {
      id: 3,
      nickname: '小红',
      avatar: 'https://placehold.co/100x100/34d399/064e3b?text=D',
      url: '/user/2',
    },
    newestMessage: '11111',
    time: '2024-05-20',
    unreadMessageCount: 0,
  },
])

// 按时间排序的会话列表，最新的在前面
const conversationList = computed(() => {
  return [...conversationListData.value].sort((a, b) => {
    const timeA = new Date(a.time).getTime()
    const timeB = new Date(b.time).getTime()
    return timeB - timeA // 降序排列，新消息在前
  })
})

// 聊天记录(模拟)
const chatHistoryList = ref<chatHistory[]>([
  {
    contactUser: {
      id: 2,
      nickname: '小明',
      avatar: 'https://placehold.co/100x100/ec4899/831843?text=B',
      url: '/user/2',
    },
    messageList: [
      {
        messageId: 1,
        content: '你好！😄',
        sendTime: '2024-06-01 10:00',
        sender: {
          id: 2,
          nickname: '小明',
          avatar: 'https://placehold.co/100x100/ec4899/831843?text=B',
          url: '/user/1',
        },
        //  isRead: true,
        type: 'text',
      },
      {
        messageId: 2,
        content: '你好！',
        sendTime: '2024-06-01 10:01',
        sender: {
          id: 1,
          nickname: '小白',
          avatar: 'https://placehold.co/100x100/facc15/78350f?text=F',
          url: '/user/2',
        },
        //    isRead: true,
        type: 'text',
      },
      {
        messageId: 3,
        content: 'https://placehold.co/100x100/34d399/064e3b?text=D',
        sendTime: '2024-06-01 10:02',
        sender: {
          id: 2,
          nickname: '小明',
          avatar: 'https://placehold.co/100x100/ec4899/831843?text=B',
          url: '/user/1',
        },
        //  isRead: true,
        type: 'image',
      },
    ],
  },
  // ...其他联系人
])

// 用户信息
const myUser = ref<user>({
  id: 1,
  nickname: '小白',
  avatar: 'https://placehold.co/100x100/facc15/78350f?text=F',
  url: '/user/1',
})

// 当前用户ID
const myUserId = computed(() => myUser.value.id)

// 选中的会话
const selectedConversation = ref<conversation | null>(null)

// 初始化选中第一个会话
const initializeSelectedConversation = () => {
  if (conversationList.value.length > 0) {
    selectedConversation.value = conversationList.value[0]
  }
}

// 在组件挂载时初始化
onMounted(() => {
  initializeSelectedConversation()
})

// 切换选中会话
function handleConversationSelect(item: conversation) {
  selectedConversation.value = item
  selectedConversation.value.unreadMessageCount = 0
}

// 当前会话的聊天记录
const currentChatHistory = computed(() => {
  if (!selectedConversation.value) return []
  const history = chatHistoryList.value.find(
    (h) => h.contactUser.id === selectedConversation.value?.contactUser.id
  )
  return history ? history.messageList : []
})

function handleSendMessage(content: string, type: 'text' | 'image') {
  // 创建新消息
  const newMessage = {
    messageId: Date.now(), // 临时ID
    content: content,
    sendTime: new Date().toLocaleString(),
    sender: myUser.value,
    receiver: selectedConversation.value?.contactUser!,
    isRead: false,
    type: type,
  }

  // 添加到当前聊天记录
  const currentHistory = chatHistoryList.value.find(
    (h) => h.contactUser.id === selectedConversation.value?.contactUser.id
  )

  if (currentHistory) {
    currentHistory.messageList.push(newMessage)
  }

  if (selectedConversation.value) {
    // 更新原始数据中的会话信息
    const originalConversation = conversationListData.value.find(
      (c) => c.contactUser.id === selectedConversation.value?.contactUser.id
    )
    if (originalConversation) {
      originalConversation.newestMessage = content
      originalConversation.time = new Date().toLocaleString()
    }
  }

  // 滚动到最新消息
  nextTick(() => {
    const chatWindow = document.querySelector('.chat-window') as HTMLElement
    if (chatWindow) {
      chatWindow.scrollTop = chatWindow.scrollHeight
    }
  })
}

// 处理消息操作
const handleMessageAction = async (action: string, message: message) => {
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
const handleMessageCopy = async (message: message) => {
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

const isReportDialogVisible = ref(false)
const currentReportTargetId = ref<number | undefined>(undefined)
const currentReportTargetType = ref<'message' | undefined>(undefined)

const showReportDialog = (messageId: number, messageType: 'message' | undefined) => {
  currentReportTargetId.value = messageId
  currentReportTargetType.value = messageType
  isReportDialogVisible.value = true
}
</script>


<style scoped>
.page-content-wrapper {
  display: flex;
  flex-direction: row;
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
</style>