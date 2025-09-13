<script setup lang="ts">
import { ref, onMounted, watch } from "vue"
import { useRoute } from "vue-router"
import PostInfoCard from "@/modules/post/components/PostInfoCard.vue"
import SearchBar from "@/modules/post/components/SearchBar.vue"   // ✅ 引入你写好的 SearchBar
import { searchPosts } from "@/modules/post/api"
import type { PostListItem } from "@/modules/post/types"

const route = useRoute()
const query = ref<string>("")
const results = ref<PostListItem[]>([])
const loading = ref(false)

async function doSearch() {
  if (!query.value) {
    results.value = []
    return
  }
  loading.value = true
  try {
    results.value = await searchPosts(query.value, 30)
  } catch (e) {
    console.error("搜索失败", e)
    results.value = []
  } finally {
    loading.value = false
  }
}

// 监听路由参数变化（如 /search?q=xxx）
watch(() => route.query.q, (q) => {
  query.value = String(q || "")
  doSearch()
})

// 首次挂载时执行
onMounted(() => {
  query.value = String(route.query.q || "")
  doSearch()
})
</script>

<template>
  <div class="container mx-auto max-w-3xl">
    <!-- 顶部：返回箭头 + 搜索框 -->
    <div class="flex items-center mt-10 mb-6 space-x-3">
      <!-- 返回按钮 -->
      <button @click="$router.back()" class="transition">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
             stroke="currentColor" class="w-6 h-6 hover:stroke-slate-600">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M15 19l-7-7 7-7" />
        </svg>
      </button>

      <!-- 复用 SearchBar -->
      <SearchBar class="flex-1" />
    </div>

    <h2 class="text-xl font-bold mb-4">搜索结果：{{ query }}</h2>

    <div v-if="loading" class="text-center text-slate-400 py-4">搜索中…</div>
    <div v-else-if="!results.length" class="text-center text-slate-400 py-4">没有找到相关帖子</div>

    <div v-else class="space-y-4">
      <PostInfoCard v-for="p in results" :key="p.postId" :post="p" />
    </div>
  </div>
</template>
