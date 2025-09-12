<script setup lang="ts">
import SearchBar from "@/modules/post/components/SearchBar.vue"
import HotTopics from "../components/HotTopics.vue"
import TodayHot from "../components/TodayHot.vue"
import RecommendUsers from "../components/RecommendUsers.vue"
import { useFoundingStore } from "../store"



const store = useFoundingStore()
store.loadAll()
</script>

<template>
  <div class="w-full px-6">
    <div class="grid grid-cols-12 gap-6">
      <!-- 中间主内容 (热门话题) -->
      <main
        class="col-span-12 lg:col-span-8 lg:col-start-3 flex justify-center min-h-screen"
      >
        <div class="w-full max-w-2xl transform translate-y-10">
          <!-- 🔹 这里用 translate-y 调整上下位置 -->
          <HotTopics :topics="store.hotTopics" />
        </div>
      </main>

      <!-- 🔹 右侧栏 (搜索框 + 今日热点 + 推荐关注，参考你的代码) -->
      <aside class="hidden md:block flex-none md:h-screen px-5 py-5 z-100 w-96">
        <div class="space-y-6">
          <!-- 搜索框 -->
          <SearchBar />

          <!-- 今日热点 -->
          <TodayHot :items="store.todayHot" />

          <!-- 推荐关注 -->
          <RecommendUsers :users="store.recommendUsers" />
        </div>
      </aside>

    </div>
  </div>
</template>

<style scoped>
/* ✅ 右侧栏保持固定在页面顶部 */
aside {
  position: sticky;
  top: 0;
}
</style>
