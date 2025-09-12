<template>
  <div class="conversation-item" :class="{ selected: props.selected }">
    <div class="avatar">
      <img
        :src="
          props.conversation.contactUser?.avatar ||
          'https://placehold.co/100x100/facc15/78350f?text=U'
        "
        :alt="props.conversation.contactUser?.nickname || `用户${props.conversation.otherUserId}`"
      />
      <!-- 未读消息红点 - 放在头像右上角 -->
      <div v-if="props.conversation.unreadCount > 0" class="unread-count">
        {{ displayUnreadCount(props.conversation.unreadCount) }}
      </div>
    </div>
    <div class="content">
      <div class="header">
        <span class="nickname" v-html="highlightedNickname"></span>
        <span class="time">{{ diffime }}</span>
      </div>
      <div class="newest-message" v-html="highlightedMessage"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { formatTime } from '../../core/utils/time'
import type { conversation } from '../types'

const props = defineProps<{
  conversation: conversation
  selected?: boolean
  searchTerm?: string
}>()

const diffime = formatTime(
  props.conversation.time || props.conversation.lastMessage?.sentAt || new Date().toISOString()
)

// 显示未读消息数量，与NoticeView.vue保持一致
function displayUnreadCount(count: number): string {
  if (count <= 0) return ''
  if (count <= 99) return count.toString()
  return '99+'
}

// 高亮搜索关键词
function highlightSearchTerm(text: string, searchTerm: string): string {
  if (!searchTerm || !text) return text

  const regex = new RegExp(`(${searchTerm})`, 'gi')
  return text.replace(regex, '<span class="highlight">$1</span>')
}

// 计算属性：高亮后的昵称
const highlightedNickname = computed(() => {
  const nickname =
    props.conversation.contactUser?.nickname || `用户${props.conversation.otherUserId}`
  return highlightSearchTerm(nickname, props.searchTerm || '')
})

// 计算属性：高亮后的最新消息
const highlightedMessage = computed(() => {
  const message = props.conversation.newestMessage || props.conversation.lastMessage?.content || ' '
  return highlightSearchTerm(message, props.searchTerm || '')
})
</script>

<style scoped>
.conversation-item {
  display: flex;
  align-items: center;
  min-width: 0;
  padding: 8px 0;
  cursor: pointer;
  transition: background 0.2s;
  position: relative;
}

.conversation-item:hover {
  background: #4b5563;
}

.conversation-item.selected {
  background: #374151;
  border-left: 3px solid #3b82f6;
}

.avatar {
  display: flex;
  align-items: center;
  padding-left: 8px;
  padding-right: 8px;
  position: relative;
}

.avatar img {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  object-fit: cover;
  display: block;
}

.content {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  justify-content: flex-start;
}

.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.nickname {
  font-weight: bold;
  font-size: 18px;
  color: #ffffff;
}

.time {
  font-size: 12px;
  color: #d1d5db;
  margin-left: 14px;
  margin-right: 8px;
}

.newest-message {
  margin-top: 4px;
  font-size: 14px;
  color: #d1d5db;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  text-align: left;
  min-height: 18px; /* 确保即使内容为空也占据一定高度 */
}

.unread-count {
  position: absolute;
  top: 0;
  right: 4px;
  min-width: 18px;
  height: 18px;
  border-radius: 99px;
  background: #ef4444;
  color: #fff;
  font-size: 11px;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  transform: translate(15%, -25%);
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