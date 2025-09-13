<template>
  <article class="w-full p-4 md:p-5 border-b-1 border-b-slate-800 bg-slate-900/30 mb-4">
    <!-- 标题（跳转浏览页） -->
    <h2 class="text-lg md:text-xl font-bold text-slate-100 hover:text-sky-400 cursor-pointer" @click="goDetail">{{
      post.title }}</h2>

    <!-- 次要信息：时间、标签、浏览、点赞 -->
    <div class="mt-2 flex flex-wrap items-center gap-2 text-xs md:text-sm text-slate-500">
      <time :datetime="post.createdAt">{{ createdAtLabel }}</time>
      <span class="mx-1">·</span>
      <span>浏览 {{ post.views ?? 0 }}</span>
      <span class="mx-1">·</span>
      <span>赞 {{ post.likes ?? 0 }}</span>
      <span v-if="post.tags?.length" class="mx-1">·</span>
      <div v-if="post.tags?.length" class="flex flex-wrap gap-1">
        <span v-for="t in post.tags" :key="t"
          class="px-2 py-0.5 rounded-full bg-slate-800 text-slate-300 text-[11px]">#{{ t }}</span>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { formatTime } from '@/modules/core/utils/time'
import type { PostListItem } from '@/modules/post/types';

const props = defineProps<{ post: PostListItem }>()

const createdAtLabel = computed(() => formatTime(String(props.post.createdAt)))
const router = useRouter()

function goDetail() { router.push({ name: 'PostDetail', params: { id: props.post.postId } }) }
</script>
