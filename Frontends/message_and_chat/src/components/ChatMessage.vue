<template>
  <div class="chat-message-wrapper">


    <!-- 使用绝对定位强制布局 -->
    <div :class="isSelf ? 'my-message' : 'other-message'">
      <!-- 对方消息：头像在左 -->
      <img v-if="!isSelf" class="message-avatar" :src="props.message.sender.avatar" alt="对方头像" />
      
      <div class="message-bubble">
        <template v-if="props.message.type === 'image'">
          <img class="msg-image" :src="props.message.content" alt="图片显示失败"/>
        </template>
        <template v-else>
          <span v-html="renderContent(props.message.content)"></span>
        </template>
      </div>
      
      <!-- 自己消息：头像在右 -->
      <img v-if="isSelf" class="message-avatar" :src="props.message.sender.avatar" alt="我的头像" />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { message } from '../types/message';

const props = defineProps<{
  message: message;
  isSelf: boolean;
  myUserId: number;
}>();

// 替换表情为图片
function renderContent(content: string) {
  let html = content.replace(/\[emoji:(\w+)\]/g, (match, p1) => {
    return `<img src="/emoji/${p1}.png" alt="${p1}" class="emoji-img" />`;
  });
  return html;
}
</script>

<style scoped>
.chat-message-wrapper {
  position: relative;
  margin: 8px 0;
  min-height: 50px;
}

.message-avatar {
  width: 36px;
  height: 36px;
  border-radius: 99%;

  object-fit: cover;
  flex-shrink: 0;
}

.message-bubble {
  max-width: 60%;
  padding: 8px 12px;
  border-radius: 8px;
  font-size: 16px;
  word-break: break-word;
}


/* 对方消息：左对齐 */
.other-message {
  position: relative;
  display: flex;
  align-items: flex-start;
  justify-content: flex-start;
  padding-left: 8px;
}

/* 自己消息：右对齐 */
.my-message {
  position: relative;
  display: flex;
  align-items: flex-start;
  justify-content: flex-end;
  padding-right: 8px; 
}

/* 对方消息的布局 */
.other-message .message-avatar {
  order: 1;
  margin-right: 8px;
  margin-left: 0;
}

.other-message .message-bubble {
  order: 2;
  margin-left: 0;
  margin-right: 0;
}

.other-message .message-bubble {
  background-color: #f0f0f0;
  color: #222;
}

/* 自己消息的布局 */
.my-message .message-avatar {
  order: 2;
  margin-left: 8px;
  margin-right: 0;
}

.my-message .message-bubble {
  order: 1;
  margin-left: 0;
  margin-right: 0;
}

.my-message .message-bubble {
  background-color: #a0e75a;
  color: #222;
}

.msg-image {
  max-width: 100%;
  height: auto;
  border-radius: 8px;
}

.emoji-img {
  width: 24px;
  height: 24px;
  vertical-align: middle;
}
</style>