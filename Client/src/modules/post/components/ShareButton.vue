<template>
  <button class="share-btn" :disabled="pending" @click="onShare" title="分享这条帖子">
    <span class="icon">📤</span>
    <span class="label">分享</span>
  </button>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { getPostDetail } from "../../post/api";
import { stashOriginalPost } from "../../post/utils/sharePayload";

const props = defineProps<{ postId: number }>();

const router = useRouter();
const pending = ref(false);

async function onShare() {
  if (pending.value) return;
  pending.value = true;
  try {
    // 1) 拉取原帖信息
    const post = await getPostDetail(props.postId);

    // 2) 放到 sessionStorage，供发帖页读取
    stashOriginalPost(post);

    // 3) 跳转发帖页，并用 query 告知“分享模式 + 原帖ID”
    router.push({
      name: "PostCreate",               // 确保你的发帖路由名就是 PostCreate（下一步会检查）
      query: { shareFrom: String(props.postId) }
    });
  } catch (e) {
    console.error("获取帖子详情失败：", e);
    alert("获取原帖失败，稍后再试");
  } finally {
    pending.value = false;
  }
}
</script>

<style scoped>
.share-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 10px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  background: #fff;
  cursor: pointer;
}
.share-btn:disabled { opacity: 0.6; cursor: not-allowed; }
.icon { font-size: 16px; }
.label { font-size: 14px; }
</style>
