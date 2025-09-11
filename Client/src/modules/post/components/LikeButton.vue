<template>
  <button
    class="like-btn"
    :class="[
      isLikedLocal 
        ? 'text-red-400 bg-red-400/10 hover:bg-red-400/20' 
        : 'text-slate-500 hover:bg-slate-800 hover:text-red-400'
    ]"
    :aria-pressed="isLikedLocal ? 'true' : 'false'"
    :disabled="pending"
    @click="onToggleLike"
  >
    <!-- 点赞图标 -->
    <span class="icon" :class="{ liked: isLikedLocal }">
      <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" :class="{ 'fill-current': isLikedLocal }">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"></path>
      </svg>
    </span>
    <!-- 点赞数 -->
    <span class="count" :class="{ liked: isLikedLocal }">{{ likeCountLocal }}</span>
  </button>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import { toggleLike } from "../../post/api";

const emit = defineEmits<{
  (e: "update:isLiked", v: boolean): void;
  (e: "update:likeCount", v: number): void;
  (e: "error", err: unknown): void;
}>();

const props = defineProps<{
  postId: number;
  isLiked: boolean;
  likeCount: number;
}>();

const isLikedLocal = ref(props.isLiked);
const likeCountLocal = ref(props.likeCount);
const pending = ref(false);

watch(() => props.isLiked, (v) => (isLikedLocal.value = v));
watch(() => props.likeCount, (v) => (likeCountLocal.value = v));

async function onToggleLike() {
  if (pending.value) return;
  pending.value = true;

  const next = !isLikedLocal.value;
  const prevLiked = isLikedLocal.value;
  const prevCount = likeCountLocal.value;

  // 乐观更新
  isLikedLocal.value = next;
  likeCountLocal.value = next ? prevCount + 1 : Math.max(0, prevCount - 1);

  try {
    await toggleLike({ type: "post", target_id: props.postId, like: next, user_id: 100247 }); // TODO: 替换为实际用户ID
    emit("update:isLiked", isLikedLocal.value);
    emit("update:likeCount", likeCountLocal.value);
  } catch (e) {
    // 失败回滚
    isLikedLocal.value = prevLiked;
    likeCountLocal.value = prevCount;
    emit("error", e);
  } finally {
    pending.value = false;
  }
}
</script>

<style scoped>
.like-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px 16px;
  border: none;
  border-radius: 8px;
  background: transparent;
  cursor: pointer;
  transition: all 0.2s ease;
  min-width: 80px;
}

.like-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* 默认样式 */
.icon {
  display: flex;
  align-items: center;
  transition: color 0.2s ease;
}

.count {
  font-size: 14px;
  font-weight: 500;
  transition: color 0.2s ease;
}

/* 点赞后样式 */
.icon.liked,
.count.liked {
  color: inherit;
}
</style>