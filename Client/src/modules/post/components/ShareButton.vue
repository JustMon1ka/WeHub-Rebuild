<template>
  <button
    class="share-btn"
    :class="[
      'text-slate-500 hover:bg-slate-800 hover:text-yellow-400'
    ]"
    :disabled="pending"
    @click="onShare"
    title="分享这条帖子"
  >
    <span class="icon">
      <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"></path>
      </svg>
    </span>
    <span class="label">分享</span>
  </button>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { getPostDetail } from "../../post/api";
import { stashOriginalPost } from "../../post/utils/sharePayload";
import { sharePost } from "../../post/api";

const props = defineProps<{ postId: number }>();

const router = useRouter();
const pending = ref(false);

async function onShare() {
  if (pending.value) return;
  pending.value = true;
  try {
    // 1) 调用后端接口，写 Redis 通知
    await sharePost(props.postId, "");

    // 2) 拉取原帖信息
    const post = await getPostDetail(props.postId);

    // 3) 放到 sessionStorage，供发帖页读取
    stashOriginalPost(post);

    // 4) 跳转发帖页 - 使用 path 跳转
    router.push({
      path: '/post/create',
      query: { shareFrom: String(props.postId) }
    });
  } catch (e) {
    console.error("分享失败：", e);
    alert("分享失败，请重试");
  } finally {
    pending.value = false;
  }
}
</script>

<style scoped>
.share-btn {
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

.share-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.icon {
  display: flex;
  align-items: center;
}

.label {
  font-size: 14px;
  font-weight: 500;
}
</style>