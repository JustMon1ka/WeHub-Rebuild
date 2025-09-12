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
  <div class="px-6">
    <div class="gap-6">

      <!-- 中间主内容 (热门话题) -->
      <main class="flex">
        <div class="w-full max-w-2xl transform translate-y-100">
          <!-- 🔹 这里用 translate-y 调整上下位置 -->
          <HotTopics :topics="store.hotTopics" />
        </div>
      </main>
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
