<template>
  <button class="like-btn"
          :aria-pressed="isLikedLocal ? 'true' : 'false'"
          :disabled="pending"
          @click="onToggleLike">
    <span class="icon">👍</span>
    <span class="count">{{ likeCountLocal }}</span>
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
    border: 1px solid #ccc;
    border-radius: 6px;
    background: #fff;
    cursor: pointer;
  }

    .like-btn[aria-pressed="true"] {
      color: #2563eb;
      border-color: #2563eb;
    }

    .like-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

  .icon {
    font-size: 16px;
  }

  .count {
    font-size: 14px;
  }
</style>
