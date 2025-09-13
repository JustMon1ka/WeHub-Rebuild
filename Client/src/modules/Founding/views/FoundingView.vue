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
    </div>
  </div>
</template>
