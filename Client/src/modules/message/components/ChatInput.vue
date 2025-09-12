<template>
  <div ref="chatInputRef" class="chat-input">
    <div class="input-tool">
      <button ref="emojiButtonRef" class="emoji-button" @click="showEmoji = !showEmoji">😊</button>
      <button class="image-button" @click="handleImageClick">🖼️</button>
      <input
        ref="fileInput"
        type="file"
        accept="image/*"
        style="display: none"
        @change="handleImageChange"
      />
    </div>
    <div v-if="showEmoji" ref="emojiListRef" class="emoji-list">
      <span v-for="emoji in emojis" :key="emoji" @click="addEmoji(emoji)">{{ emoji }}</span>
    </div>
    <div class="input-text">
      <textarea
        ref="textareaRef"
        v-model="text"
        placeholder="输入消息"
        class="input-area"
        rows="1"
        @input="autoResize"
        @keydown.enter.exact.prevent="handleSendClick"
      ></textarea>
    </div>
    <div class="send-button-row">
      <button
        class="send-button"
        :disabled="!text.trim()"
        :class="{ active: text.trim() }"
        type="button"
        @click="handleSendClick"
      >
        发送
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick, onMounted, onUnmounted } from 'vue'

const text = ref('')
const textareaRef = ref<HTMLTextAreaElement | null>(null)
const showEmoji = ref(false)
const emojis = [
  '😊',
  '😂',
  '😍',
  '👍',
  '🎉',
  '😢',
  '😡',
  '😎',
  '😭',
  '😱',
  '🤔',
  '😴',
  '🤗',
  '😋',
  '🤩',
  '😇',
  '🥰',
  '😘',
  '😉',
  '😌',
  '😏',
  '😒',
  '😞',
  '😔',
  '😤',
  '😠',
  '😡',
  '🤬',
  '😈',
  '👿',
  '💀',
  '👻',
]
const fileInput = ref<HTMLInputElement | null>(null)
const emit = defineEmits<{
  sendMessage: [content: string, type: 'text' | 'image']
}>()
const emojiListRef = ref<HTMLDivElement | null>(null)
const emojiButtonRef = ref<HTMLButtonElement | null>(null)
const chatInputRef = ref<HTMLDivElement | null>(null)
let resizeObserver: ResizeObserver | null = null

// 自动调整高度
function autoResize() {
  nextTick(() => {
    const el = textareaRef.value
    const container = chatInputRef.value
    if (el && container) {
      // 获取容器高度
      const containerHeight = container.clientHeight

      // 计算可用高度（减去工具区域和发送按钮区域的高度）
      const toolHeight = 50 // 工具区域高度（包括padding）
      const sendButtonHeight = 50 // 发送按钮区域高度（包括padding）
      const availableHeight = containerHeight - toolHeight - sendButtonHeight

      // 设置最小高度和最大高度，添加安全边距
      const minHeight = 40
      const maxHeight = Math.max(availableHeight - 10, minHeight) // 减去10px安全边距

      // 限制最大高度，防止溢出
      const absoluteMaxHeight = 200 // 绝对最大高度限制
      const finalMaxHeight = Math.min(maxHeight, absoluteMaxHeight)

      el.style.height = 'auto' // 先重置高度
      el.style.height = Math.min(el.scrollHeight, finalMaxHeight) + 'px'
      el.style.overflowY = el.scrollHeight > finalMaxHeight ? 'auto' : 'hidden'
    }
  })
}

onMounted(() => {
  autoResize()
})

// 插入表情
function addEmoji(emoji: string) {
  text.value += emoji
  showEmoji.value = false
  autoResize()
}

// 发送图片
function handleImageClick() {
  fileInput.value?.click()
}

// 处理图片上传
function handleImageChange(e: Event) {
  const files = (e.target as HTMLInputElement).files
  if (files && files[0]) {
    const file = files[0]
    const reader = new FileReader()
    reader.onload = () => {
      emit('sendMessage', reader.result as string, 'image')
    }
    reader.readAsDataURL(file)
    ;(e.target as HTMLInputElement).value = ''
  }
}

// 发送消息
function handleSendClick() {
  if (text.value.trim()) {
    emit('sendMessage', text.value.trim(), 'text')
    text.value = ''
    showEmoji.value = false
    autoResize()
  }
}

// 点击外部区域隐藏表情列表
function handleClickOutside(e: Event) {
  const target = e.target as Element
  const emojiList = emojiListRef.value
  const emojiButton = emojiButtonRef.value

  if (showEmoji.value && emojiList && emojiButton) {
    if (!emojiList.contains(target) && !emojiButton.contains(target)) {
      showEmoji.value = false
    }
  }
}

onMounted(() => {
  autoResize()
  document.addEventListener('click', handleClickOutside)

  // 监听容器大小变化
  if (chatInputRef.value && window.ResizeObserver) {
    resizeObserver = new ResizeObserver(() => {
      autoResize()
    })
    resizeObserver.observe(chatInputRef.value)
  }
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  if (resizeObserver) {
    resizeObserver.disconnect()
  }
})
</script>

<style scoped>
.chat-input {
  position: relative;
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden; /* 防止内容溢出 */
}

.input-tool {
  display: flex;
  gap: 8px;
  padding: 8px 16px 0 16px;
  flex-shrink: 0; /* 防止工具区域被压缩 */
  min-height: 40px; /* 确保工具区域有足够空间 */
}

.emoji-button,
.image-button {
  background: none;
  border: none;
  font-size: 16px;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  transition: background 0.2s;
}

.emoji-button:hover,
.image-button:hover {
  background: #f0f0f0;
}

.input-text {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding-top: 8px;
  padding-bottom: 8px;
  padding-left: 12px;
  padding-right: 12px;
  min-height: 0; /* 允许flex子元素收缩 */
  overflow: hidden; /* 防止内容溢出 */
}

.input-area {
  border: none;
  outline: none;
  width: 100%;
  min-height: 40px;
  background: transparent;
  resize: none;
  font-size: 16px;
  box-sizing: border-box;
  overflow-y: auto; /* 允许垂直滚动 */
  transition: height 0.1s;
}

.send-button-row {
  display: flex;
  justify-content: flex-end;
  padding: 8px 16px 8px 0;
  flex-shrink: 0; /* 防止发送按钮区域被压缩 */
  min-height: 40px; /* 确保发送按钮区域有足够空间 */
}

.send-button {
  background: #00aeec;
  border: none;
  border-radius: 8px;
  padding: 8px 18px;
  cursor: pointer;
  font-size: 16px;
  font-weight: 600;
  transition: background 0.2s;
}

.send-button.active {
  background: #00aeec;
}

.send-button:disabled {
  background: #5a6478;
  cursor: not-allowed;
}

.emoji-list {
  display: flex;
  grid-template-columns: repeat(8, 1fr);
  width: 320px;
  flex-wrap: wrap;
  gap: 4px;
  margin: 0px 0 8px 16px;
  border: 1px solid #ddd;
  border-radius: 4px;
  background: #fff;
  padding: 4px;
  position: absolute;
  bottom: 100%;
  z-index: 10;
}

.emoji-list span {
  cursor: pointer;
  font-size: 20px;
  padding: 2px 4px;
  border-radius: 4px;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
}
.emoji-list span:hover {
  background: #f0f0f0;
}
</style>
