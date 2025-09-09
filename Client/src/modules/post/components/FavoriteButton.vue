<template>
  <button class="fav-btn"
          :aria-pressed="isFavoritedLocal ? 'true' : 'false'"
          :disabled="pending"
          @click="onToggleFavorite">
    <!-- 收藏图标 -->
    <span class="icon" :class="{ favorited: isFavoritedLocal }">⭐</span>
    <!-- 收藏数，可选显示 -->
    <span v-if="showCount" class="count" :class="{ favorited: isFavoritedLocal }">{{ favoriteCountLocal }}</span>
  </button>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import { toggleFavorite } from "../../post/api";

const emit = defineEmits<{
  (e: "update:isFavorited", v: boolean): void;
  (e: "update:favoriteCount", v: number): void;
  (e: "error", err: unknown): void;
}>();

const props = defineProps<{
  postId: number;
  isFavorited: boolean;
  favoriteCount?: number;
  showCount?: boolean;
}>();

const isFavoritedLocal = ref(props.isFavorited);
const favoriteCountLocal = ref(props.favoriteCount ?? 0);
const pending = ref(false);

watch(() => props.isFavorited, v => isFavoritedLocal.value = v);
watch(() => props.favoriteCount, v => { if (v !== undefined) favoriteCountLocal.value = v; });

async function onToggleFavorite() {
  if (pending.value) return;
  pending.value = true;

  const next = !isFavoritedLocal.value;
  const prevFav = isFavoritedLocal.value;
  const prevCount = favoriteCountLocal.value;

  // 乐观更新
  isFavoritedLocal.value = next;
  favoriteCountLocal.value = next ? prevCount + 1 : Math.max(0, prevCount - 1);

  try {
    await toggleFavorite({ type: "post", target_id: props.postId, favorite: next });
    emit("update:isFavorited", isFavoritedLocal.value);
    emit("update:favoriteCount", favoriteCountLocal.value);
  } catch (e) {
    // 回滚
    isFavoritedLocal.value = prevFav;
    favoriteCountLocal.value = prevCount;
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
  gap: 4px;
  padding: 4px 8px;
  border: none;
  background: transparent; /* 透明底 */
  cursor: pointer;
}

.fav-btn:disabled {
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

/* 收藏后变黄 */
.icon.favorited,
.count.favorited {
  color: #f59e0b; /* 金黄色 */
}
</style>
