<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import MarkdownViewer from '@/modules/post/components/MarkdownViewer.vue';
import LikeButton from '@/modules/post/components/LikeButton.vue';
import ShareButton from '@/modules/post/components/ShareButton.vue';
import FavoriteButton from '@/modules/post/components/FavoriteButton.vue';
import { getPostDetail } from '@/modules/post/api';
import type { PostDetail } from '@/modules/post/types';
import UserInfo from '@/modules/user/scripts/UserInfo';
import { formatTime } from '@/modules/core/utils/time';

// 演示 Markdown（注意：不要用 $$...$$；行内公式用 \( ... \)，块级用 \[ ... \]）
const mdDemo = ref<string>(
`
## 图片
![示意图](https://placehold.co/800x400/png)

## 视频直链（自动 \`<video>\`）  
https://www.w3schools.com/html/mov_bbb.mp4

## Bilibili（自动 \`<iframe>\`）  
https://www.bilibili.com/video/BV1xx411c7mD
`
);

// —— 路由参数
const route = useRoute();
const postId = Number(route.params.id);

// —— 数据状态
const loading = ref(false);
const err = ref<unknown>(null);
const post = ref<PostDetail | null>(null);
const author = ref<UserInfo | null>(null);

// —— 按钮状态
const isLiked = ref(false);
const likeCount = ref(0);
const isFavorited = ref(false);
const favoriteCount = ref(0);

// —— 衍生显示
const createdAtLabel = computed(() =>
  post.value?.createdAt ? formatTime(String(post.value.createdAt)) : ''
);

// —— 拉取详情与作者信息
onMounted(async () => {
  loading.value = true;
  try {
    const detail = await getPostDetail(postId);
    post.value = detail;
    
    // 设置按钮状态
    isLiked.value = detail.isLiked || false;
    likeCount.value = detail.likes || 0;
    isFavorited.value = detail.isFavorited || false;

    
    author.value = new UserInfo(String(detail.userId));
    await author.value.loadUserData();
  } catch (e) {
    err.value = e;
    // 这里不抛出，让页面仍能用 mdDemo 回退渲染
    console.error('[PostDetail] load failed:', e);
  } finally {
    loading.value = false;
  }
});

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
      <p v-if="post?.views!==undefined || post?.likes!==undefined" class="mt-2 text-sm text-slate-500">
        <span v-if="post?.views!==undefined">阅读 {{ post.views }}</span>
        <span v-if="post?.views!==undefined && post?.likes!==undefined" class="mx-2">·</span>
        <span v-if="post?.likes!==undefined">点赞 {{ post.likes }}</span>
      </p>
    </div>

    <!-- 帖主个人信息 -->
    <div class="border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6">
      <div class="flex items-center gap-3">
        <img v-if="author?.avatarUrl" :src="author.avatarUrl" class="w-12 h-12 rounded-full" alt="avatar">
        <div v-else class="w-12 h-12 rounded-full bg-slate-800 flex items-center justify-center text-slate-300 text-sm">U</div>
        <div class="min-w-0">
          <div class="font-medium text-slate-200 truncate">
            {{ author?.nickname || ('用户'+(post?.userId??'')) || '帖主昵称' }}
          </div>
          <div class="text-xs text-slate-500">
            {{ createdAtLabel || '' }}
          </div>
        </div>
      </div>
    </div>
    
    <!-- 帖子内容 -->
    <div class="border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6">
      <MarkdownViewer :model-value="post?.content ?? mdDemo" />
    </div>
    
    <!-- 标签 -->
    <div class="border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6">
      <div v-if="post?.tags?.length" class="flex flex-wrap gap-2">
        <span v-for="t in post.tags" :key="t" class="px-2 py-1 rounded-full bg-slate-800 text-slate-300 text-xs">#{{ t }}</span>
      </div>
      <div v-else class="text-slate-500 text-sm">无标签</div>
    </div>

    <!-- 点赞分享举报等 -->
    <div class="border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6">
      <div class="flex items-center justify-center gap-4">
        <!-- 点赞按钮 -->
        <LikeButton
          :postId="postId"
          :isLiked="isLiked"
          :likeCount="likeCount"
          @update:isLiked="handleLikeUpdate"
          @update:likeCount="handleLikeCountUpdate"
          @error="handleError"
        />
        
        <!-- 分享按钮 -->
        <ShareButton :postId="postId" />
        
        <!-- 收藏按钮 -->
        <FavoriteButton
          :postId="postId"
          :isFavorited="isFavorited"
          :favoriteCount="favoriteCount"
          :showCount="true"
          @update:isFavorited="handleFavoriteUpdate"
          @update:favoriteCount="handleFavoriteCountUpdate"
          @error="handleError"
        />
      </div>
    </div>

    <!-- 评论区 -->
    <div class="border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6">
    </div>
  </div>
</template>

<style scoped>

</style>