<template>
  <article class="w-full p-4 md:p-5 border-b-1 border-b-slate-800 bg-slate-900/30 mb-4">
    <!-- 标题（跳转浏览页） -->
    <h2 class="text-lg md:text-xl font-bold text-slate-100 hover:text-sky-400 cursor-pointer" @click="goDetail">{{
      post.title }}</h2>

    <!-- 次要信息：时间、标签、浏览、点赞 -->
    <div class="mt-2 flex flex-wrap items-center gap-2 text-xs md:text-sm text-slate-500">
      <time :datetime="post.createdAt">{{ createdAtLabel }}</time>
      <span v-if="post.tags?.length" class="mx-1">·</span>
      <div v-if="post.tags?.length" class="flex flex-wrap gap-1">
        <span v-for="t in post.tags" :key="t"
          class="px-2 py-0.5 rounded-full bg-slate-800 text-slate-300 text-[11px]">#{{ t }}</span>
      </div>
      <span class="mx-1">·</span>
      <span>浏览 {{ post.views ?? 0 }}</span>
      <span class="mx-1">·</span>
      <span>赞 {{ post.likes ?? 0 }}</span>
    </div>

    <!-- 操作区：隐藏/显示、编辑、删除 -->
    <div class="mt-3 flex items-center gap-2">
      <!-- 隐藏/显示切换（预留后端调用位置） -->
      <button @click="onToggleHidden" :disabled="busy"
        class="px-3 py-1 rounded-xl border border-slate-700 hover:bg-slate-800 disabled:opacity-60 inline-flex items-center gap-1">
        <svg v-if="localHidden" class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75"
            d="M3 3l18 18M10.584 10.575A3 3 0 0012 15a3 3 0 001.416-5.64M9.88 5.575C10.57 5.38 11.273 5.25 12 5.25c5.25 0 9 4.5 9 6.75-.282 1.026-1.19 2.457-2.684 3.747m-3.087 2.09A10.47 10.47 0 0112 18.75c-5.25 0-9-4.5-9-6.75 0-1.05.57-2.28 1.637-3.51" />
        </svg>
        <svg v-else class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75"
            d="M2.25 12s3.75-6.75 9.75-6.75 9.75 6.75 9.75 6.75-3.75 6.75-9.75 6.75S2.25 12 2.25 12z" />
          <circle cx="12" cy="12" r="3.25" />
        </svg>
        <span>{{ localHidden ? '已隐藏' : '公开中' }}</span>
      </button>

      <!-- 编辑 -->
      <button @click="goEdit"
        class="px-3 py-1 rounded-xl border border-slate-700 hover:bg-slate-800 inline-flex items-center gap-1">
        <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75"
            d="M4 20h4l10.5-10.5a2.121 2.121 0 10-3-3L5 17v3z" />
        </svg>
        <span>编辑</span>
      </button>

      <!-- 删除 -->
      <button @click="onDelete" :disabled="busy"
        class="px-3 py-1 rounded-xl border border-red-700 text-red-300 hover:bg-red-900/20 disabled:opacity-60 inline-flex items-center gap-1">
        <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75"
            d="M3 6h18M8 6V4.5A1.5 1.5 0 019.5 3h5A1.5 1.5 0 0116 4.5V6m-8 0h8m-9 0l1 14.25A1.5 1.5 0 009.5 21h5a1.5 1.5 0 001.5-1.25L17 6" />
        </svg>
        <span>删除</span>
      </button>
    </div>
  </article>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { formatTime } from '@/modules/core/utils/time'
import { deletePost, togglePostHidden } from '@/modules/post/api'
import type { PostListItem } from '@/modules/post/types';

const props = defineProps<{ post: PostListItem }>()
const emit = defineEmits<{ (e: 'deleted', id: number): void, (e: 'updated', p: PostListItem): void }>()

const createdAtLabel = computed(() => formatTime(String(props.post.createdAt)))
const localHidden = ref(!!props.post.isHidden)
const busy = ref(false)
const router = useRouter()

function goDetail() { router.push({ name: 'PostDetail', params: { id: props.post.postId } }) }
function goEdit() {
  if ((router as any).hasRoute && (router as any).hasRoute('PostEdit')) router.push({ name: 'PostEdit', params: { id: props.post.postId } })
  else router.push(`/post/edit/${props.post.postId}`)
}

async function onToggleHidden() {
  if (busy.value) return; busy.value = true
  const next = !localHidden.value
  try {
    // 预留后端调用位置：如需改端点，请在 api.ts 的 togglePostHidden 中修改
    await togglePostHidden(props.post.postId, next)
    // 可按需要校验 res.code===200
    localHidden.value = next
    emit('updated', { ...props.post, isHidden: next })
  } catch (e) { console.error('[toggle hidden] failed', e) }
  finally { busy.value = false }
}

async function onDelete() {
  if (busy.value) return
  if (!confirm('确认删除该帖子？此操作不可恢复。')) return
  busy.value = true
  try {
    const ok = await deletePost(props.post.postId)
    // 约定 deletePost unwrap 后返回 {code:200} 或 true，视你的实现而定
    emit('deleted', props.post.postId)
  } catch (e) { console.error('[delete post] failed', e) }
  finally { busy.value = false }
}
</script>
