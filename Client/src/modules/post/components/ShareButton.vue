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
    // 1) 调用后端分享接口 - 添加错误处理
    await handleSharePost(props.postId);

    // 2) 拉取原帖信息
    const post = await getPostDetail(props.postId);

    // 3) 放到 sessionStorage，供发帖页读取
    stashOriginalPost(post);

    // 4) 跳转发帖页
    await router.push({
      path: '/post/create',
      query: { shareFrom: String(props.postId) }
    });

  } catch (e) {
    handleShareError(e);
  } finally {
    pending.value = false;
  }
}

// 处理分享API调用
async function handleSharePost(postId: number) {
  try {
    // 直接调用现有的sharePost API
    return await sharePost(postId, "");
  } catch (error: any) {
    // 如果是405错误，可能是HTTP方法不对
    if (error.response?.status === 405) {
      console.warn("POST方法不被允许，尝试其他方法");
      return await tryAlternativeShareMethods(postId);
    }
    throw error;
  }
}

// 尝试其他分享方法
async function tryAlternativeShareMethods(postId: number) {
  try {
    // 方法1: 尝试使用GET请求
    const response = await fetch(`/api/posts/${postId}/share`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      }
    });

    if (!response.ok) {
      throw new Error(`GET请求失败: ${response.status}`);
    }

    return await response.json();
  } catch (getError) {
    console.warn("GET方法也失败，尝试备用方案", getError);

    // 方法2: 使用navigator.share作为备用
    if (navigator.share) {
      try {
        const post = await getPostDetail(postId);
        await navigator.share({
          title: post.title || '分享帖子',
          text: '来看看这个有趣的帖子',
          url: window.location.href
        });
        return { success: true, method: 'navigator.share' };
      } catch (shareError) {
        return;
      }
    }

    // 方法3: 复制链接到剪贴板
    try {
      await navigator.clipboard.writeText(window.location.href);
      alert('链接已复制到剪贴板，您可以手动分享');
      return { success: true, method: 'clipboard' };
    } catch (clipboardError) {
      console.error('复制到剪贴板失败:', clipboardError);
      throw new Error('所有分享方法都失败了');
    }
  }
}

// 处理分享错误
function handleShareError(error: any) {
  if (error.response?.status === 405) {
    alert("分享功能暂时遇到问题，已尝试备用方案");
  } else if (error.response?.status === 485) {
    alert("分享参数错误，请稍后再试");
  } else {
    alert("分享失败，请重试");
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
