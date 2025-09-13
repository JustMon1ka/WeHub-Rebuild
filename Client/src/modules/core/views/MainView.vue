<script setup lang="ts">
import PostList from "@/modules/post/components/PostList.vue"
import { ref } from "vue"

const listRef = ref<InstanceType<typeof PostList> | null>(null)
</script>

<template>
  <div class="container mx-auto max-w-screen-xl">
    <div class="w-full flex flex-col h-screen overflow-y-auto">
      <div class="flex flex-col w-full items-center py-10 space-y-6
            border-b border-slate-800 relative">
        <div class="background-pattern">
          <div class="pattern-container"></div>
        </div>
        <img src="@/assets/logo.svg" alt="logo" class="h-32 w-32"/>
        <label class="text-slate-100 font-bold text-2xl mb-2">
          欢迎来到 WeHub 社区
        </label>
      </div>

      <label class="w-full text-center p-4 border-b-sky-500 border-b text-lg font-bold
              sticky top-0 bg-slate-900/70 backdrop-blur-md z-10">
        帖子
      </label>

      <!-- 中间主内容区 (帖子流) -->
      <main class="border-x border-slate-800">
        <div class="divide-y divide-slate-800">
          <PostList
            ref="listRef"
            :num="10"
            :tailPostId="null"
            :PostMode="0"
            :tagName="null"
          />
        </div>
      </main>

    </div>
  </div>
</template>

<style scoped>
.background-pattern {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  opacity: 0.15;
  overflow: hidden;
  z-index: 0;
  --pattern-size: 150px;
  --pattern-color: rgba(255, 255, 255, 0.4);
}

.pattern-container {
  position: absolute;
  width: 200%;
  height: 200%;
  top: -3rem;
  left: -5rem;
  mask-image: repeating-linear-gradient(
    30deg,
    black var(--pattern-size),
    black calc(var(--pattern-size) * 2)
  );
  background-image:
    /* 使用伪元素创建SVG图案 */
    url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' color='rgba(255,255,255,0.3)'%3E%3Cpath d='M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5' stroke='white' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'/%3E%3C/svg%3E");
  background-size: calc(var(--pattern-size) * 0.7) calc(var(--pattern-size) * 0.7);
  transform: rotate(15deg);
  background-repeat: repeat;
  animation: slide 12s linear infinite;
}

@keyframes slide {
  0% {
    background-position: 0 0;
  }
  100% {
    background-position: var(--pattern-size) var(--pattern-size);
  }
}

</style>
