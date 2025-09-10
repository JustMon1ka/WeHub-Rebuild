<template>
  <button
    class="fav-btn"
    :class="[
      isFavoritedLocal 
        ? 'text-yellow-400 bg-yellow-400/10 hover:bg-yellow-400/20' 
        : 'text-slate-500 hover:bg-slate-800 hover:text-yellow-400'
    ]"
    :aria-pressed="isFavoritedLocal ? 'true' : 'false'"
    :disabled="pending"
    @click="onToggleFavorite"
  >
    <!-- 收藏图标 -->
    <span class="icon" :class="{ favorited: isFavoritedLocal }">
      <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" :class="{ 'fill-current': isFavoritedLocal }">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z"></path>
      </svg>
    </span>
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

.fav-btn:disabled {
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

/* 收藏后样式 */
.icon.favorited,
.count.favorited {
  color: inherit;
}
</style>