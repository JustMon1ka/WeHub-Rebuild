<script setup lang="ts">
import type { HotTopic } from "../types"

defineProps<{ topics: HotTopic[] }>()
</script>

<template>
  <div class="w-full max-w-2xl border border-slate-800 rounded-2xl p-6">
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
</template>
