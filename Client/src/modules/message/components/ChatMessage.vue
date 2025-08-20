<template>
  <div class="chat-message-wrapper">
    <div :class="isSelf ? 'my-message' : 'other-message'">
      <!-- 对方消息：头像在左 -->
      <router-link v-if="!isSelf" to="/otherUserHomepage">
        <img class="message-avatar" :src="props.message.sender.avatar" alt="对方头像" />
      </router-link>

      <div class="message-bubble" @contextmenu.prevent="handleContextMenuShow">
        <template v-if="props.message.type === 'image'">
          <img class="msg-image" :src="props.message.content" alt="图片显示失败" />
        </template>
        <template v-else>
          <span v-html="renderContent(props.message.content)"></span>
        </template>
      </div>

      <!-- 自己消息：头像在右 -->
      <div v-if="isSelf" class="avatar-wrapper">
        <router-link to="/personalHomepage">
          <img class="message-avatar" :src="props.message.sender.avatar" alt="我的头像" />
        </router-link>
      </div>

      <!-- 右键菜单 -->
      <ContextMenu
        :visible="contextMenuVisible"
        :x="contextMenuX"
        :y="contextMenuY"
        :menuItems="contenxtMenuItems"
        @close="handleContextMenuHide"
        @select="handleContextMenuItemClick"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import type { message } from '../types'
import ContextMenu, { type MenuItem } from './ContextMenu.vue'

const props = defineProps<{
  message: message
  isSelf: boolean
  myUserId: number
}>()

const emit = defineEmits<{
  (e: 'messageAction', action: string, message: message): void
}>()

const contextMenuVisible = ref(false)
const contextMenuX = ref(0)
const contextMenuY = ref(0)

const contextMenuPosition = computed(() => {
  return {
    x: contextMenuX.value,
    y: contextMenuY.value,
  }
})

const contenxtMenuItems = computed((): MenuItem[] => {
  const items: MenuItem[] = []

  items.push({
    key: 'copy',
    text: '复制',
    icon: 'copy',
  })

  // items.push({
  //   key: "delete",
  //   text: "删除",
  //   icon: "delete",
  // });

  // items.push({
  //   key: "reply",
  //   text: "回复",
  //   icon: "reply",
  // });

  // items.push({
  //   key: "forward",
  //   text: "转发",
  //   icon: "forward",
  // });

  items.push({
    key: 'report',
    text: '举报',
    icon: 'report',
  })
  return items
})

// 右键显示菜单
const handleContextMenuShow = (event: MouseEvent) => {
  contextMenuX.value = event.clientX
  contextMenuY.value = event.clientY
  contextMenuVisible.value = true
}

// 隐藏菜单
const handleContextMenuHide = () => {
  contextMenuVisible.value = false
}

// 处理菜单项点击
const handleContextMenuItemClick = (item: MenuItem) => {
  handleContextMenuHide()
  emit('messageAction', item.key, props.message)
}

// 替换表情为图片
function renderContent(content: string) {
  let html = content.replace(/\[emoji:(\w+)\]/g, (_match, p1) => {
    return `<img src="/emoji/${p1}.png" alt="${p1}" class="emoji-img" />`
  })
  return html
}

// 处理点击事件
const handleLeftMouseClick = (event: MouseEvent) => {
  if (contextMenuVisible.value) {
    const target = event.target as Element
    if (!target.closest('.context-menu-overview')) {
      handleContextMenuHide()
    }
  }
}

const handleRightMouseClickOnOtherMessage = (event: MouseEvent) => {
  if (contextMenuVisible.value) {
    const target = event.target as Element
    if (!target.closest('.chat-message-wrapper')) {
      handleContextMenuHide()
    }
  }
}

onMounted(() => {
  document.addEventListener('click', handleLeftMouseClick)
  document.addEventListener('contextmenu', handleRightMouseClickOnOtherMessage)
})

onUnmounted(() => {
  document.removeEventListener('click', handleLeftMouseClick)
  document.removeEventListener('contextmenu', handleRightMouseClickOnOtherMessage)
})
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

.my-message router-link,
.other-message router-link {
  display: contents;
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
.my-message .avatar-wrapper {
  order: 2;
  margin-left: 8px;
  margin-right: 0;
  display: flex;
  align-items: center;
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