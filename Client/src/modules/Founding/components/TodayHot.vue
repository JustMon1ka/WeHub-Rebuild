<script setup lang="ts">
import type { TodayHot } from "../types"

defineProps<{ items: TodayHot[] }>()
</script>

<template>
  <div class="bg-slate-800 rounded-2xl p-4 w-72">
    <h2 class="text-lg font-bold mb-4">今日热点</h2>
    <ul class="space-y-3">
      <li v-for="item in items" :key="item.topic">
        <router-link
          :to="`/topic/${item.topic.replace('#', '')}`"
          class="block hover:bg-slate-700/50 p-2 rounded-md cursor-pointer"
        >
          <p v-if="item.category" class="text-sm text-slate-400">
            {{ item.category }} · 热门
          </p>
          <p class="font-semibold text-base">{{ item.topic }}</p>
          <p class="text-sm text-slate-400">{{ item.count }} 帖子</p>
        </router-link>
      </li>
    </ul>
  </div>
</template>
