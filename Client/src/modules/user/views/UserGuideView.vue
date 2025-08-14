<script setup lang="ts">
import TagGuide from '@/modules/user/components/Guide/TagGuide.vue'

</script>


<template>
  <div class="w-full max-w-4xl flex flex-col  items-center justify-center p-4 sm:p-6">
    <!-- 头部 -->
    <div class="text-center mb-8">
      <svg class="w-12 h-12 text-sky-400 mx-auto" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
      </svg>
      <h1 class="text-3xl font-bold mt-4">选择你感兴趣的标签</h1>
      <p class="text-slate-400 mt-2">告诉我们你的偏好，以便为你推荐更精彩的内容。 (已选择 <span id="selected-count" class="text-sky-400 font-bold">3</span> 个)</p>
    </div>

    <TagGuide/>

    <!-- 底部操作栏 -->
    <div class="mt-8 flex flex-col sm:flex-row items-center justify-center gap-4">
      <button class="w-full sm:w-auto flex justify-center py-3 px-10 border border-slate-600 rounded-full shadow-sm text-sm font-bold hover:bg-slate-800 focus:outline-none transition-colors duration-200">
        跳过
      </button>
      <button class="w-full sm:w-auto flex justify-center py-3 px-10 border border-transparent rounded-full shadow-sm text-sm font-bold text-white bg-sky-500 hover:bg-sky-600 focus:outline-none transition-colors duration-200">
        完成
      </button>
    </div>
  </div>
</template>

