<script setup lang="ts">

</script>

<template>
  <div class="space-y-6">
    <!-- 搜索框 -->
    <div class="sticky top-0">
      <svg class="w-5 h-5 text-slate-500 absolute top-1/2 left-3 -translate-y-1/2" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path></svg>
      <input type="text" placeholder="搜索..." class="w-full bg-slate-800 border border-slate-700 rounded-full py-2 pl-10 pr-4 focus:outline-none focus:ring-2 focus:ring-sky-500">
    </div>

    <div class=" space-y-6 h-fit">
      <!-- 热门趋势 -->
      <div class="bg-slate-800 rounded-2xl p-4">
        <h2 class="text-lg font-bold mb-4">热门趋势</h2>
        <ul class="space-y-3">
          <li>
            <a href="#" class="block hover:bg-slate-700/50 p-2 rounded-md">
              <p class="text-sm text-slate-400">科技圈 · 热门</p>
              <p class="font-semibold">#AI编程助手</p>
              <p class="text-sm text-slate-400">1.5万 帖子</p>
            </a>
          </li>
          <li>
            <a href="#" class="block hover:bg-slate-700/50 p-2 rounded-md">
              <p class="text-sm text-slate-400">电影 · 热门</p>
              <p class="font-semibold">#科幻电影新纪元</p>
              <p class="text-sm text-slate-400">8,923 帖子</p>
            </a>
          </li>
          <li>
            <a href="#" class="block hover:bg-slate-700/50 p-2 rounded-md">
              <p class="font-semibold">#夏日旅行计划</p>
              <p class="text-sm text-slate-400">5,102 帖子</p>
            </a>
          </li>
        </ul>
      </div>

      <!-- 推荐关注 -->
      <div class="bg-slate-800 rounded-2xl p-4">
        <h2 class="text-lg font-bold mb-4">推荐关注</h2>
        <ul class="space-y-3">
          <li class="flex items-center">
            <img class="w-10 h-10 rounded-full" src="https://placehold.co/100x100/ec4899/831843?text=B" alt="User Avatar">
            <div class="ml-3 flex-1">
              <p class="font-semibold text-sm">设计圈大佬</p>
              <p class="text-slate-400 text-xs">@designer_pro</p>
            </div>
            <button class="bg-slate-200 hover:bg-white text-slate-900 font-semibold text-sm py-1 px-4 rounded-full transition-colors duration-200">关注</button>
          </li>
          <li class="flex items-center">
            <img class="w-10 h-10 rounded-full" src="https://placehold.co/100x100/8b5cf6/4c1d95?text=E" alt="User Avatar">
            <div class="ml-3 flex-1">
              <p class="font-semibold text-sm">前端技术周刊</p>
              <p class="text-slate-400 text-xs">@frontend_weekly</p>
            </div>
            <button class="bg-slate-200 hover:bg-white text-slate-900 font-semibold text-sm py-1 px-4 rounded-full transition-colors duration-200">关注</button>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<style scoped>

</style>
