<template>
  <div class="bg-white rounded-lg shadow-sm p-6">
    <h2 class="text-lg font-semibold text-gray-900 mb-4">热门趋势</h2>

    <div class="space-y-4">
      <div
        v-for="topic in topics"
        :key="topic.id"
        class="cursor-pointer hover:bg-gray-50 p-3 rounded-lg transition-colors"
      >
        <div class="text-sm text-gray-500 mb-1">{{ topic.category }} · 热门</div>
        <div class="font-medium text-gray-900 mb-1">#{{ topic.tag }}</div>
        <div class="text-sm text-gray-500">{{ formatPostCount(topic.postCount) }} 帖子</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { TrendingTopic } from '../types.ts'

defineProps<{
  topics: TrendingTopic[]
}>()

const formatPostCount = (count: number): string => {
  if (count >= 10000) {
    return (count / 10000).toFixed(1) + '万'
  }
  return count.toLocaleString()
}
</script>
