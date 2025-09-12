<script setup lang="ts">
import { useRoute, useRouter } from "vue-router"
import { ref, onMounted } from "vue"
import { fetchTopicCount } from "@/modules/Founding/api"   // ✅ 统一 API
import PostList from "@/modules/post/components/PostList.vue"

const route = useRoute()
const router = useRouter()

// 当前话题 & 帖子数
const topic = ref<string>("")
const count = ref<number>(0)

// PostList 用到的引用
const listRef = ref<InstanceType<typeof PostList> | null>(null)

// 返回上一页
function goBack() {
  router.back()
}

// 初始化加载
onMounted(async () => {
  topic.value = route.params.topic as string
  try {
    count.value = await fetchTopicCount(topic.value)
  } catch (err) {
    console.error("获取帖子数失败:", err)
  }
})

// 帖子加载完成
function handleLoaded(list: any) {
  console.log("帖子加载完成:", list)
}

// 帖子加载失败
function handleError(e: any) {
  console.error("帖子加载失败:", e)
}
</script>

<template>
  <div class="max-w-2xl mx-auto">
    <!-- 顶部：返回 + 话题信息 -->
    <div class="flex items-center space-x-4 p-4 border-b border-slate-800 sticky top-0 bg-slate-900 z-10">
      <button @click="goBack" class="p-2 hover:bg-slate-700 rounded-full">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-slate-300" fill="none"
             viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M15 19l-7-7 7-7" />
        </svg>
      </button>
      <div>
        <h1 class="text-xl font-bold">#{{ topic }}</h1>
        <p class="text-sm text-slate-400">{{ count }} 帖子</p>
      </div>
    </div>

    <!-- 🔹 帖子列表 -->
    <div class="mt-4">
      <h2 class="text-lg font-semibold px-4 text-slate-200 mb-2">最新帖子</h2>
      <PostList
        v-if="topic"
        ref="listRef"
        :num="10"
        :tailPostId="null"
        :PostMode="1"
        :tagName="topic"
        @loaded="handleLoaded"
        @error="handleError"
      />
    </div>
  </div>
</template>
