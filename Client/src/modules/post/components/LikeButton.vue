<template>
  <button class="like-btn"
          :aria-pressed="isLikedLocal ? 'true' : 'false'"
          :disabled="pending"
          @click="onToggleLike">
    <!-- 点赞图标 -->
    <span class="icon" :class="{ liked: isLikedLocal }">❤️</span>
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
    await toggleLike({ type: "post", target_id: props.postId, like: next });
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
  gap: 4px;
  padding: 4px 8px;
  border: none;
  background: transparent;
  cursor: pointer;
}

.like-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* 默认灰色 */
.icon {
  font-size: 18px;
  color: #999;
  transition: color 0.2s ease;
}
.count {
  font-size: 14px;
  color: #555;
  transition: color 0.2s ease;
}

/* 点赞后变红 */
.icon.liked,
.count.liked {
  color: #e0245e; /* 推特红色 */
}
</style>
