<script setup lang="ts">
import SearchBar from "@/modules/post/components/SearchBar.vue"
import TodayHot from "@/modules/Founding/components/TodayHot.vue"
import RecommendUsers from "@/modules/Founding/components/RecommendUsers.vue"
import { useFoundingStore } from "@/modules/Founding/store"
import { watch } from "vue"
import { currentUserId } from "@/modules/Founding/store/CurrentUser"

// 使用 Pinia Store
const store = useFoundingStore()

// 如果启动时已经有 userId，则直接加载
if (currentUserId.value) {
  store.loadAll()
}

// 监听登录状态变化，重新加载
watch(currentUserId, (newVal) => {
  if (newVal) {
    console.log("✅ 右侧栏组件刷新数据，userId:", newVal)
    store.loadAll()
  }
})
</script>

<template>
  <aside class="hidden md:block flex-none md:h-screen px-5 py-5 z-100 w-96">
    <div class="space-y-6">
      <!-- 搜索框 -->
      <SearchBar />

      <!-- 热门话题 -->
      <TodayHot :items="store.todayHot" />

      <!-- 推荐关注 -->
      <RecommendUsers
        :users="store.recommendUsers"
        @updated="store.loadAll()"   
      />
    </div>
  </aside>
</template>
