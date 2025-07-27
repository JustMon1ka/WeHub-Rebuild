<template>
  <div class = "container">
    <div class = "left">
      <SideNavigationBar />

    </div>
    <div class="divider-vertical"></div>
    <div class = "center">
      <div class="divider-horizontal"></div> 
      <div class = 'message-heading'>
        <h2>私信</h2>
      </div>
      <div class="divider-horizontal"></div> 
      <div class = 'message-search'>
        <input
          type = "text"
          placeholder="🔍搜索"
        />
      </div>
      <div class="divider-horizontal"></div> 
      <div class = 'message-list'>
        <div>
          <Conversation
            v-for="item in conversationList"
            :key="item.contactUser.id"
            :conversation="item"
            :selected="selectedConversation?.contactUser.id === item.contactUser.id"
            @click="selectConversation(item)"
          />
        </div>
      </div>
      <div class="divider-horizontal"></div> 
    </div>
    <div class="divider-vertical"></div>
    <div class = "right">
      <div class="divider-horizontal"></div> 
      <div class = 'chat-header'>
        <ConservationHeader v-if="selectedConversation" 
                          :conversation="selectedConversation" />
      </div>
      <div class="divider-horizontal"></div> 
        <div class = 'chat-window'>
          <div class="chat-content">
            <ChatMessage
            v-for="message in currentChatHistory"
            :key="message.messageId"
            :message="message"
            :isSelf="message.sender.id === myUserId"
            :myUserId="myUserId"
          />
          </div>
        </div>
      <div class="divider-horizontal"></div> 
        <div class = "chat-input">
          <ChatInput  @sendMessage="handleSendMessage" />
        </div>
      <div class="divider-horizontal"></div> 
    </div>
    <div class="divider-vertical"></div>
  </div>
</template>

<script setup lang="ts">
import { ref,computed,nextTick } from 'vue';
import SideNavigationBar from '../components/SideNavigationBar.vue'
import Conversation from '../components/Conversation.vue';
import ConservationHeader from '../components/ConservationHeader.vue';
import ChatInput from '../components/ChatInput.vue';
import ChatMessage from '../components/ChatMessage.vue';

import type { conversation, chatHistory, user } from '../types/message'

const conversationList = ref<conversation[]>([
  {
    contactUser: {
      id: 2,
      nickname: '小明',
      avatar: 'https://placehold.co/100x100/ec4899/831843?text=B',
      url: '/user/1'
    },
    newestMessage: 'test message',
    time: '2024-06-01',
    unreadMessageCount: 2
  },
  {
    contactUser: {
      id: 3,
      nickname: '小红',
      avatar: 'https://placehold.co/100x100/34d399/064e3b?text=D',
      url: '/user/2'
    },
    newestMessage: '11111',
    time: '2025-07-20',
    unreadMessageCount: 0
  }
]);

// 聊天记录(模拟)
const chatHistoryList = ref<chatHistory[]>([
  {
    contactUser: {
      id: 2,
      nickname: '小明',
      avatar: 'https://placehold.co/100x100/ec4899/831843?text=B',
      url: '/user/2'
    },
    messageList: [
      {
        messageId: 1,
        content: '你好！😄',
        sendTime: '2024-06-01 10:00',
        sender: { id: 2, nickname: '小明', avatar: 'https://placehold.co/100x100/ec4899/831843?text=B', url: '/user/1' },
      //  isRead: true,
        type: 'text'
      },
      {
        messageId: 2,
        content: '你好！',
        sendTime: '2024-06-01 10:01',
        sender: { id: 1, nickname: '小白', avatar: 'https://placehold.co/100x100/facc15/78350f?text=F', url: '/user/2' },
    //    isRead: true,
        type: 'text'
      },
      {
        messageId: 3,
        content: 'https://placehold.co/100x100/34d399/064e3b?text=D',
        sendTime: '2024-06-01 10:02',
        sender: { id: 2, nickname: '小明', avatar: 'https://placehold.co/100x100/ec4899/831843?text=B', url: '/user/1' },
     //  isRead: true,
        type: 'image'
      }
    ]
  },
  // ...其他联系人
]);

// 用户信息
const myUser = ref<user>({
  id: 1,
  nickname: '小白',
  avatar: 'https://placehold.co/100x100/facc15/78350f?text=F',
  url: '/user/1'
});

// 当前用户ID
const myUserId = computed(() => myUser.value.id);

// 选中的会话
const selectedConversation = ref<conversation | null>(conversationList.value[0] || null);

// 切换选中会话
function selectConversation(item: conversation) { 
  selectedConversation.value = item;
  selectedConversation.value.unreadMessageCount = 0;
}

// 当前会话的聊天记录
const currentChatHistory = computed(() => {
  if (!selectedConversation.value) return [];
  const history = chatHistoryList.value.find(
    h => h.contactUser.id === selectedConversation.value?.contactUser.id
  );
  return history ? history.messageList : [];
});

function handleSendMessage(content: string, type: 'text' | 'image') { 
  // 创建新消息
  const newMessage = {
    messageId: Date.now(), // 临时ID
    content: content,
    sendTime: new Date().toLocaleString(),
    sender: myUser.value,
    receiver: selectedConversation.value?.contactUser!,
    isRead: false,
    type: type
  }

    // 添加到当前聊天记录
  const currentHistory = chatHistoryList.value.find(
    h => h.contactUser.id === selectedConversation.value?.contactUser.id
  )

  if (currentHistory) {
    currentHistory.messageList.push(newMessage)
  }

  if (selectedConversation.value) {
    selectedConversation.value.newestMessage = content
    selectedConversation.value.time = new Date().toLocaleString()
  
  }

  // 滚动到最新消息
  nextTick(() => {
    const chatWindow = document.querySelector('.chat-window') as HTMLElement
    if (chatWindow) {
      chatWindow.scrollTop = chatWindow.scrollHeight
    }
  })
}
</script>


<style scoped>
.container {
  display: flex;
  height: 100vh;
  width: 100vw; 
  max-width: 100%;
  margin:0 auto;
  box-sizing: border-box;
  overflow:hidden;
}

.left{
  width:20%;
  display:flex;
  align-items: center;
  justify-content: center;
}

.center{
  width:20%;
  display: flex;
  flex-direction: column;
  overflow-wrap: break-word;
  word-break: break-all;
}

.message-heading{
  flex:1;
  display: flex;
  padding-left: 32px;
}

.message-search{
  flex:1;
  display: flex;
  align-items: center;     
  justify-content: center;  
}

.message-list{
  flex:11;
} 

.right{
  width:60%;
  display:flex;
  flex-direction: column;
}

.chat-header{
  height: 8%;
  display: flex;
  align-items: center;
}

.chat-window{
  height: 70%;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
}

.chat-content {
  padding: 12px 8px, 12px 8px; 
  flex: 1;
}

.chat-input{
  height: 22%
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