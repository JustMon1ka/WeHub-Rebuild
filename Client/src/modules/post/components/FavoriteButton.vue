<template>
  <button class="fav-btn"
          :aria-pressed="isFavoritedLocal ? 'true' : 'false'"
          :disabled="pending"
          @click="onToggle">
    <span class="icon">⭐</span>
  </button>
</template>

<script setup lang="ts">
  import { ref, watch } from "vue";
  import { toggleFavorite } from "../../post/api";

  const emit = defineEmits<{
    (e: "update:isFavorited", v: boolean): void;
    (e: "error", err: unknown): void;
  }>();

  const props = defineProps<{
    postId: number;
    isFavorited: boolean;
  }>();

  const isFavoritedLocal = ref(props.isFavorited);
  const pending = ref(false);

  watch(() => props.isFavorited, (v) => (isFavoritedLocal.value = v));

  async function onToggle() {
    if (pending.value) return;
    pending.value = true;

    const prev = isFavoritedLocal.value;
    isFavoritedLocal.value = !prev; // 乐观更新

    try {
      await toggleFavorite(props.postId);
      emit("update:isFavorited", isFavoritedLocal.value);
    } catch (e) {
      isFavoritedLocal.value = prev; // 回滚
      emit("error", e);
    } finally {
      pending.value = false;
    }
  }
</script>

<style scoped>
  .fav-btn {
    display: inline-flex;
    align-items: center;
    padding: 4px 8px;
    border: 1px solid #ccc;
    border-radius: 6px;
    background: #fff;
    cursor: pointer;
  }

    .fav-btn[aria-pressed="true"] {
      color: #d97706;
      border-color: #d97706;
    }

    .fav-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

  .icon {
    font-size: 16px;
  }
</style>
