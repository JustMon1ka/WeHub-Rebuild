<script setup lang="ts">
import { useRoute, useRouter } from "vue-router"
import { ref, onMounted } from "vue"
import { fetchTopicCount } from "@/modules/Founding/api"  // ✅ 用统一 api

const route = useRoute()
const router = useRouter()

const topic = ref<string>("")
const count = ref<number>(0)

function goBack() {
  router.back()
}

onMounted(async () => {
  topic.value = route.params.topic as string
  try {
    count.value = await fetchTopicCount(topic.value)
  } catch (err) {
    console.error("获取帖子数失败:", err)
  }
})
</script>

<template>
  <div class="max-w-2xl mx-auto">
    <!-- 顶部：返回箭头 + 话题 -->
    <div class="flex items-center space-x-4 p-4 border-b border-slate-800 sticky top-0 bg-slate-900 z-10">
      <!-- 返回箭头 -->
      <button @click="goBack" class="p-2 hover:bg-slate-700 rounded-full">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-slate-300" fill="none"
             viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M15 19l-7-7 7-7" />
        </svg>
      </button>

      <!-- 话题信息 -->
      <div>
        <h1 class="text-xl font-bold">#{{ topic }}</h1>
        <p class="text-sm text-slate-400">{{ count }} 帖子</p>
      </div>
    </div>
  </div>
</template>
