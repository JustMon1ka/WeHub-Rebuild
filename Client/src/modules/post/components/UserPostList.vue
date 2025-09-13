<template>
  <section class="w-full">
    <div v-if="loading" class="py-6 text-center text-slate-500">正在加载…</div>
    <div v-else-if="error" class="py-6 text-center text-red-400">{{ error }}</div>
    <template v-else>
      <UserPostCard v-for="p in posts" :key="p.postId" :post="p" />
      <div v-if="!posts.length" class="py-6 text-center text-slate-500">暂无帖子</div>
    </template>
  </section>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { type PostListItem } from '@/modules/post/types'
import UserPostCard from '@/modules/post/components/UserPostCard.vue'
import { getPosts } from '@/modules/post/api'

const props = defineProps<{
  userId: string
}>()

const posts = ref<PostListItem[]>([])
const loading = ref(true)
const error = ref('')

function normalize(p: any): PostListItem {
  return { postId: p.postId, userId: p.userId, title: p.title, content: p.content, tags: p.tags || [], createdAt: p.createdAt, views: p.views ?? 0, likes: p.likes ?? 0, isHidden: p.isHidden ?? (p.IS_HIDDEN ?? false) }
}

async function load() {
  loading.value = true; error.value = ''
  try {
    // 后端若无需 uid 参数（从 token 判定），getMyPosts 会直接按当前用户返回
    const list = await getPosts(undefined, Number(props.userId));
    posts.value = Array.isArray(list) ? list.map(normalize) : []
  } catch (e: any) { error.value = e?.message || '加载失败' }
  finally { loading.value = false }
}

onMounted(load)
</script>