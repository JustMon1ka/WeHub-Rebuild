<script setup lang="ts">
import SearchBar from "@/modules/post/components/SearchBar.vue"   // 你的搜索框路径
import TodayHot from "@/modules/Founding/components/TodayHot.vue"
import RecommendUsers from "@/modules/Founding/components/RecommendUsers.vue"
import { useFoundingStore } from "@/modules/Founding/store"

// 使用 Pinia Store
const store = useFoundingStore()
store.loadAll()
</script>

<template>
  <aside class="hidden md:block flex-none md:h-screen px-5 py-5 z-100 w-96">
    <div class="space-y-6">
      <!-- 搜索框 -->
      <SearchBar />

      <!-- 热门话题 -->
      <TodayHot :items="store.todayHot" />

      <!-- 推荐关注 -->
      <RecommendUsers :users="store.recommendUsers" />
    </div>
  </aside>
</template>

<style scoped>
/* 可以在这里单独调整右侧栏的布局样式 */
</style>
