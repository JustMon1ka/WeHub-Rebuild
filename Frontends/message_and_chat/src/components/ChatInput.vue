<template>
  <div class="chat-input">
    <div class = "input-tool">
      <button ref="emojiButtonRef" class="emoji-button" @click="showEmoji = !showEmoji">😊</button>
      <button class="image-button" @click="onImageClick">🖼️</button>
      <input ref="fileInput" type="file" accept="image/*" style="display: none;"
        @change="onImageChange"/>
    </div>
    <div v-if="showEmoji" ref="emojiListRef" class="emoji-list">
      <span v-for="emoji in emojis" :key="emoji" @click="addEmoji(emoji)">{{ emoji }}</span>
    </div>
    <div class="input-text">
      <textarea ref="textareaRef" v-model="text" placeholder="输入消息" class="input-area" rows="1"
      @input="autoResize"
      @keydown.enter.exact.prevent="send"
      ></textarea>
    </div>
    <div class="send-button-row">
      <button class="send-button" :disabled="!text.trim()" :class="{active: text.trim()}"  type="button"
      @click="send">
      发送
    </button>
    </div>

  </div>
  
</template>

<script setup lang="ts">
import { ref, nextTick, onMounted,onUnmounted } from 'vue'

const text = ref('')
const textareaRef = ref<HTMLTextAreaElement | null>(null)
const showEmoji = ref(false);
const emojis = [
  '😊', '😂', '😍', '👍', '🎉', '😢', '😡', '😎',
  '😭', '😱', '🤔', '😴', '🤗', '😋', '🤩', '😇',
  '🥰', '😘', '😉', '😌', '😏', '😒', '😞', '😔',
  '😤', '😠', '😡', '🤬', '😈', '👿', '💀', '👻',
];
const fileInput = ref<HTMLInputElement | null>(null)
const imageUrl = ref<string | null>(null)
const emit = defineEmits<{
  sendMessage: [content: string, type: 'text' | 'image']
}>()
const emojiListRef = ref<HTMLDivElement | null>(null)
const emojiButtonRef = ref<HTMLButtonElement | null>(null)

// 自动调整高度
function autoResize() {
  nextTick(() => {
    const el = textareaRef.value
    if (el) {
      el.style.height = 'auto' // 先重置高度
      el.style.height = Math.min(el.scrollHeight, 120) + 'px'
      el.style.overflowY = el.scrollHeight > 120 ? 'auto' : 'hidden'
    }
  })
}

onMounted(() => {
  autoResize()
})
 
// 插入表情
function addEmoji(emoji: string) { 
  text.value += emoji;
  showEmoji.value = false;
  autoResize();
}

// 发送图片
function onImageClick() {
  fileInput.value?.click();
}

// 处理图片上传
function onImageChange(e: Event) {
  const files = (e.target as HTMLInputElement).files;
  if (files && files[0]) {
    const file = files[0];
    const reader = new FileReader();
    reader.onload = (e) => {
      emit('sendMessage',reader.result as string,'image')
    }
    reader.readAsDataURL(file);
    (e.target as HTMLInputElement).value = '';
  }
}

// 发送消息
function send() {
  if (text.value.trim()) {
    emit('sendMessage', text.value.trim(), 'text')
    text.value = ''
    showEmoji.value = false
    autoResize();
  }
}

// 点击外部区域隐藏表情列表
function handleClickOutside(e: Event) {
  const target = e.target as Element;
  const emojiList = emojiListRef.value;
  const emojiButton = emojiButtonRef.value;

  if (showEmoji.value && emojiList && emojiButton) {
    if (!emojiList.contains(target) && !emojiButton.contains(target)) {
      showEmoji.value = false;
    }
  }
}

onMounted(() => {
  autoResize();
  document.addEventListener('click', handleClickOutside);
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside);
})
</script>

<style scoped>
.chat-input{
  position: relative;
  min-height: 100%;
}

.input-tool{
  display: flex;
  gap: 8px;
  padding: 8px 16px 0 16px;
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

.input-text{
  flex:1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding-top:8px;
  padding-bottom:8px;
  padding-left:12px;
  padding-right:12px;
}

.input-area{
  border:none;
  outline:none;
  width: 100%;
  min-height: 40px;
  max-height: 100px;
  background:transparent;
  resize: none;
  font-size: 16px;
  box-sizing: border-box;
  overflow-y: hidden;
  transition: height 0.1s;
}

.send-button-row {
  display: flex;
  justify-content: flex-end; 
  margin-top: 8px;
}


.send-button{
  position:absolute;
  right: 8px;
  bottom: 8px;
  background:#e0e0e0;
  border: none;
  border-radius: 8px;
  padding: 8px 18px;
  cursor: pointer;
  font-size: 16px;
}

.send-button.active{
  background: #4fc3f7; 
}

.send-button:disabled {
  background: #e0e0e0;
  cursor: not-allowed;
}

.emoji-list{
  display: flex;
  grid-template-columns: repeat(8, 1fr);
  width: 320px;
  flex-wrap: wrap;
  gap:4px;
  margin:0px 0 8px 16px;
  border:1px solid #ddd;
  border-radius: 4px;
  background: #fff;
  padding: 4px;
  position:absolute;
  bottom: 100%;
  z-index:10;
}

.emoji-list span{
  cursor: pointer;
  font-size: 20px;
  padding: 2px 4px;
  border-radius: 4px;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
}
.emoji-list span:hover{ 
  background: #f0f0f0;
}

</style>