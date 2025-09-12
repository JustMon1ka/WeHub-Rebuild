<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import MarkdownViewer from '@/modules/post/components/MarkdownViewer.vue';
import LikeButton from '@/modules/post/components/LikeButton.vue';
import ShareButton from '@/modules/post/components/ShareButton.vue';
import FavoriteButton from '@/modules/post/components/FavoriteButton.vue';
import CommentList from '@/modules/post/components/CommentList.vue'; // 导入评论组件
import { getPostDetail, increaseViewsById } from '@/modules/post/api';
import type { PostDetail } from '@/modules/post/types';
import UserInfo from '@/modules/user/scripts/UserInfo';
import { formatTime } from '@/modules/core/utils/time';
import { checkLike } from '@/modules/post/api';

// 路由参数
const route = useRoute()
const postId = Number(route.params.id)

// 状态
const loading = ref(true)
const errorText = ref('')
const post = ref<PostDetail | null>(null)
const author = ref<UserInfo | null>(null)

// —— 按钮状态
const isLiked = ref(false);
const likeCount = ref(0);
const isFavorited = ref(false);
const favoriteCount = ref(0);
const commentCount = ref(0); // 添加评论数状态

// 仅在“非代码块”中把 $$...$$ → \[...\]，避免 KaTeX 报错
function sanitizeMath(md: string): string {
  if (!md) return '';
  const parts = md.split(/(```[\s\S]*?```)/g);
  return parts.map(seg => {
    const isCode = seg.startsWith('```') && seg.endsWith('```');
    if (isCode) return seg;
    return seg.replace(/\$\$([\s\S]*?)\$\$/g, (_m, g1) => `\\[${g1}\\]`);
  }).join('');
}

// 衍生显示
const createdAtLabel = computed(() =>
  post.value?.createdAt ? formatTime(String(post.value.createdAt)) : ''
)
const ready = computed(() => !!post.value && typeof post.value.content === 'string');
const contentMd = computed(() => sanitizeMath(post.value?.content || ''));

async function load() {
  loading.value = true;
  errorText.value = '';
  try {
    const detail = await getPostDetail(postId);
    const isLikedResp = await checkLike('post', postId);
    post.value = detail;
    post.value.views++;
    likeCount.value = detail.likes || 0;
    isLiked.value = isLikedResp;
    author.value = new UserInfo(String(detail.userId));
    await author.value.loadUserData();
  } catch (e: any) {
    console.error('[PostDetail] load failed:', e);
    errorText.value = e?.message || '加载失败，请稍后重试';
  } finally {
    loading.value = false;
    await increaseViewsById(postId);
  }
}
function reload() { load(); }

onMounted(load);

// 处理评论数变化事件
function handleCommentCountChange(newCount: number) {
  console.log('🔄 接收到评论数更新:', newCount);
  commentCount.value = newCount;
}

// 处理点赞状态更新
function handleLikeUpdate(newValue: boolean) {
  isLiked.value = newValue;
}

// 处理点赞数更新
function handleLikeCountUpdate(newCount: number) {
  likeCount.value = newCount;
}

// 处理收藏状态更新
function handleFavoriteUpdate(newValue: boolean) {
  isFavorited.value = newValue;
}

// 处理收藏数更新
function handleFavoriteCountUpdate(newCount: number) {
  favoriteCount.value = newCount;
}

// 处理评论数更新
function handleCommentAdded() {
  commentCount.value = (commentCount.value || 0) + 1;
}

function handleCommentDeleted() {
  commentCount.value = Math.max(0, (commentCount.value || 0) - 1);
}

// 处理错误
function handleError(error: unknown) {
  console.error('操作失败:', error);
  // 可以在这里添加用户提示
}
</script>

<template>
  <!-- 显示区域规定 -->
  <div class="w-full max-w-none min-w-0 pt-16 scroll-pt-16 max-h-[calc(100dvh-4rem)] overflow-y-auto">
    <!-- 帖子标题 -->
    <div class="border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6">
      <h1 class="text-2xl font-bold text-slate-100 leading-snug">
        {{ post?.title || '帖子标题' }}
      </h1>
      <p v-if="post?.views !== undefined || post?.likes !== undefined || commentCount !== undefined"
        class="mt-2 text-sm text-slate-500">
        <span v-if="post?.views !== undefined">阅读 {{ post.views }}</span>
        <span v-if="post?.views !== undefined && post?.likes !== undefined" class="mx-2">·</span>
        <span v-if="post?.likes !== undefined">点赞 {{ post.likes }}</span>
        <span v-if="(post?.views !== undefined || post?.likes !== undefined) && commentCount !== undefined"
          class="mx-2">·</span>
        <span v-if="commentCount !== undefined">评论 {{ commentCount }}</span>
      </p>
    </div>

    <!-- 帖主个人信息 -->
    <div class="border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6">
      <div class="flex items-center gap-3">
        <img v-if="author?.avatarUrl" :src="author.avatarUrl" class="w-12 h-12 rounded-full" alt="avatar">
        <div v-else class="w-12 h-12 rounded-full bg-slate-800 flex items-center justify-center text-slate-300 text-sm">
          U</div>
        <div class="min-w-0">
          <div class="font-medium text-slate-200 truncate">
            {{ author?.nickname || ('用户' + (post?.userId ?? '')) || '帖主昵称' }}
          </div>
          <div class="text-xs text-slate-500">
            {{ createdAtLabel || '' }}
          </div>
        </div>
      </div>
    </div>

    <!-- 帖子内容 -->
    <div v-if="ready" class="border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6">
      <MarkdownViewer :model-value="contentMd" />
    </div>
    <div v-else-if="loading" class="border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6 text-slate-500">
      正在加载…
    </div>
    <div v-else class="border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6 text-red-400">
      {{ errorText }}
      <button class="ml-3 px-3 py-1 rounded-xl border border-red-400 hover:bg-red-400/10" @click="reload">重试</button>
    </div>

    <!-- 标签 -->
    <div class="border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6">
      <div v-if="post?.tags?.length" class="flex flex-wrap gap-2">
        <span v-for="t in post.tags" :key="t" class="px-2 py-1 rounded-full bg-slate-800 text-slate-300 text-xs">#{{ t
          }}</span>
      </div>
      <div v-else class="text-slate-500 text-sm">无标签</div>
    </div>

    <!-- 点赞分享举报等 -->
    <div class="border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6">
      <div class="flex items-center justify-center gap-4">
        <!-- 点赞按钮 -->
        <LikeButton :postId="postId" :isLiked="isLiked" :likeCount="likeCount" @update:isLiked="handleLikeUpdate"
          @update:likeCount="handleLikeCountUpdate" @error="handleError" />

        <!-- 分享按钮 -->
        <ShareButton :postId="postId" />

        <!-- 收藏按钮 -->
        <FavoriteButton :postId="postId" :isFavorited="isFavorited" :favoriteCount="favoriteCount" :showCount="true"
          @update:isFavorited="handleFavoriteUpdate" @update:favoriteCount="handleFavoriteCountUpdate"
          @error="handleError" />
      </div>
    </div>

    <!-- 评论区 -->
    <div class="border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6">
      <h3 class="text-xl font-bold text-slate-100 mb-4">评论 ({{ commentCount }})</h3>

      <!-- 评论列表组件 -->
      <CommentList :post-id="postId" @comment-added="handleCommentAdded" @comment-deleted="handleCommentDeleted"
        @comment-count-change="handleCommentCountChange" />
    </div>
  </div>
</template>

<style scoped>
/* 确保评论区域样式协调 */
.comment-section {
  background-color: transparent;
}

.comment-input-container {
  border-color: #1e293b;
}

.comments-list {
  border-color: #1e293b;
}

.comment-item {
  border-color: #1e293b;
}

.reply-input-container {
  border-color: #1e293b;
}
</style>