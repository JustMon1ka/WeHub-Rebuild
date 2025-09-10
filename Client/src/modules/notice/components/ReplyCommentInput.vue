<template>
  <div class="comment-input-wrapper" v-if="visible">
    <div class="comment-input-container">
      <img v-if="useAvater" :src="useAvater" :alt="'用户头像'" class="user-avater" />
    </div>

    <div class="comment-input-section">
      <textarea
        ref="textareaRef"
        v-model="commentText"
        class="comment-textarea"
        :placeholder="placeholder"
      ></textarea>
    </div>

    <button class="submit-button">发表评论</button>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, nextTick } from 'vue'

interface Rrops {
  visible: boolean
  placeholder: string
  useAvater: string
  replyToUserId: number
}

interface Emits {
  (e: 'submit', comment: string): void
  (e: 'cancel'): void
}

const props = defineProps<Rrops>()
const emits = defineEmits<Emits>()

const commentText = ref('')
const textareaRef = ref<HTMLTextAreaElement | null>(null)

watch(
  () => props.visible,
  (visible) => {
    if (visible) {
      nextTick(() => {
        textareaRef.value?.focus()
      })
    }
  }
)
</script>

<style scoped>
.comment-input-wrapper {
  display: flex;
  gap: 12px;
  margin-top: 24px;
}

.comment-input-container {
  align-items: center;
  justify-content: center;
}

.user-avater {
  display: flex;
  width: 48px;
  height: 48px;
  border-radius: 100%;
  object-fit: cover;
}

.comment-input-section {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.comment-textarea {
  border: 2px solid #ddd;
  border-radius: 6px;
  padding: 12px;
}

.submit-button {
  padding: 8px 16px;
  border: 1px solid rgb(0, 174, 236);
  border-radius: 6px;
  background: rgb(0, 174, 236);
  color: white;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.2s;
}

.submit-button:hover:not(:disabled) {
  background: rgb(0, 174, 236);
  border-color: rgb(0, 174, 236);
}

.submit-button:disabled {
  background: #ccc;
  border-color: #ccc;
  cursor: not-allowed;
}
</style>