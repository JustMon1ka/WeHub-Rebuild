<script setup lang="ts">
import PostList from "@/modules/post/components/PostList.vue"
import { ref } from "vue"

const listRef = ref<InstanceType<typeof PostList> | null>(null)
</script>

<template>
  <div class="container mx-auto max-w-screen-xl">
    <div class="grid grid-cols-1 md:grid-cols-10 lg:grid-cols-12 gap-x-4">

      <!-- 左侧栏：只保留推荐帖子 -->
      <aside class="hidden md:block md:col-span-2 lg:col-span-3 p-4 sticky top-0 h-screen">
        <div class="flex flex-col h-full items-end"> <!-- 🔹靠右对齐 -->
            <!-- 推荐帖子固定模块 -->
            <div class="bg-slate-800 text-white px-3 py-2 rounded-2xl shadow w-fit">
            <h2 class="text-right text-lg font-bold">推荐帖子</h2>
            </div>
        </div>
      </aside>

      <!-- 中间主内容区 (帖子流) -->
      <main class="col-span-1 md:col-span-8 lg:col-span-9 border-x border-slate-800 h-screen overflow-y-auto">
        <div class="divide-y divide-slate-800">
          <PostList
            ref="listRef"
            :num="10"
            :tailPostId="null"
            :PostMode="3"
            :tagName="null"
          />
        </div>
      </main>

    </div>
  </div>
</template>


<!-- 为主页面 -->