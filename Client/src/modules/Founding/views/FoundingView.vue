<script setup lang="ts">
import HotTopics from "../components/HotTopics.vue"
import { useFoundingStore } from "../store"



const store = useFoundingStore()
store.loadAll()
</script>

<template>
  <div class="p-6">
    <div class="gap-6">
      <!-- 中间主内容 (热门话题) -->
      <div class="flex">
        <div class="w-full ">
          <!-- 🔹 这里用 translate-y 调整上下位置 -->
          <HotTopics :topics="store.hotTopics" />
        </div>
      </div>

      <!-- 左侧栏：只保留推荐帖子 -->
      <main class="p-4 sticky top-0 h-screen">
        <div class="flex flex-col h-full items-center"> <!-- 🔹靠右对齐 -->
          <!-- 推荐帖子固定模块 -->
          <div class="bg-slate-800 text-white px-3 py-2 rounded-2xl shadow w-fit">
            <h2 class="text-right text-lg font-bold">推荐帖子</h2>
          </div>
        </div>
      </main>
    </div>
  </div>
</template>
