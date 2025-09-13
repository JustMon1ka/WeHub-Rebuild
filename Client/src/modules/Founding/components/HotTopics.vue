<script setup lang="ts">
import type { HotTopic } from "../types"
import PostList from "@/modules/post/components/PostList.vue"

// 热门话题数据
defineProps<{ topics: HotTopic[] }>()
</script>

<template>
  <div class="w-full max-w-2xl space-y-6">
    <!-- 🔹 热门话题 -->
    <div class="border border-slate-800 rounded-2xl p-6">
      <h2 class="text-lg font-bold mb-4 text-center">热门话题</h2>
      <div class="grid grid-cols-2 gap-4">
        <router-link
          v-for="topic in topics"
          :key="topic.topic"
          :to="`/topic/${topic.topic.replace('#', '')}`"
          class="w-full block p-4 bg-slate-800 rounded-xl hover:bg-slate-700 transition-colors cursor-pointer"
        >
          <p class="font-bold text-sky-400">{{ topic.topic }}</p>
          <p class="text-sm text-slate-300 mt-1">{{ topic.desc }}</p>
          <p class="text-xs text-slate-500 mt-2">{{ topic.count }} 帖子</p>
        </router-link>
      </div>
    </div>

    <!-- 🔹 帖子列表 -->
    <div class="border border-slate-800 rounded-2xl p-6">
      <h2 class="text-lg font-bold mb-4 text-center">帖子发现</h2>
      <PostList :num="10" :tailPostId="null" :PostMode="4" />
    </div>
  </div>
</template>
