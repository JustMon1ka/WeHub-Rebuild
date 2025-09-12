<script setup lang="ts">
import { ref } from "vue"
import PostList from "@/modules/post/components/PostList.vue"

// 用于操作 PostList 的引用
const listRef = ref<InstanceType<typeof PostList> | null>(null)

// 加载完成
function handleLoaded(list: any) {
  console.log("帖子加载完成:", list)
}

// 加载错误
function handleError(e: any) {
  console.error("帖子加载出错:", e)
}
</script>

<template>
  <PostList
    ref="listRef"
    :num="10"
    :tailPostId="null"
    @loaded="handleLoaded"
    @error="handleError"
  />
</template>
