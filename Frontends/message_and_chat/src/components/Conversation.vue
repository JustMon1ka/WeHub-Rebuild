<template>
  <div class = "conversation-item"
       :class="{selected: props.selected}">
    <div class="avatar">
      <img :src="props.conversation.contactUser.avatar"
           :alt="props.conversation.contactUser.nickname"/>
    </div>
    <div class="content">
      <div class="header">
        <span class="nickname">{{ props.conversation.contactUser.nickname }}</span>
        <span class="time">{{ diffime }}</span>
      </div>
      <div class="newest-message">{{ props.conversation.newestMessage }}</div>
    </div>
    
  </div>
</template>

<script setup lang="ts">
import { formatTime } from '../types/message'
import type { conversation } from '../types/message'

const props = defineProps<{
  conversation: conversation,
  selected?: boolean
}>()


const diffime = formatTime(props.conversation.time);

</script>

<style scoped>
.conversation-item {
  display: flex;
  align-items: center;
  min-width: 0;
  padding:8px 0;
  cursor: pointer;
  transition: background 0.2s;
}

.conversation-item:hover {
  background: #f3f4f6;
}

.conversation-item.selected {
  background: #e0e7ff;
}

.avatar {
  display: flex;
  align-items: center;
  padding-left: 8px;
  padding-right: 8px;
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
  justify-content: center;
}

.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.nickname {
  font-weight: bold;
  font-size: 18px;
}
 
.time {
  font-size: 12px;
  color: #999;
  margin-left: 14px;
  margin-right: 8px;
}

.newest-message {
  margin-top: 4px;
  font-size: 14px;
  color: #999;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  text-align: left;
}
</style>