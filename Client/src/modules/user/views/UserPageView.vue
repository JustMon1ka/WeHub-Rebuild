<script setup lang="ts">

import UserCardPage from '@/modules/user/components/UserPage/UserCardPage.vue'
</script>

<template>
  <main class="col-span-1 md:col-span-8 lg:col-span-7 min-h-screen w-full">
    <UserCardPage/>

    <!-- 内容切换 Tab -->
    <div class="flex border-b border-slate-800">
      <a href="#" class="w-1/3 text-center p-4 font-semibold border-b-2 border-sky-500">帖子</a>
      <a href="#" class="w-1/3 text-center p-4 text-slate-400 hover:bg-slate-800 transition-colors duration-200">回复</a>
      <a href="#" class="w-1/3 text-center p-4 text-slate-400 hover:bg-slate-800 transition-colors duration-200">赞</a>
    </div>

    <!-- 帖子列表 -->
    <div class="divide-y divide-slate-800">
      <!-- 帖子卡片 1 -->
      <article class="p-4 flex space-x-4 hover:bg-slate-800/50 transition-colors duration-200 cursor-pointer">
        <img class="wd-12 h-12 rounded-full flex-shrink-0" src="https://placehold.co/100x100/7dd3fc/0f172a?text=头像" alt="User Avatar">
        <div class="flex-1">
          <div class="flex items-baseline space-x-2">
            <p class="font-bold">用户名</p>
            <p class="text-slate-400 text-sm">@username · 2小时前</p>
          </div>
          <p class="mt-2 text-slate-300">
            刚玩了两个小时《赛博纪元》新DLC，感觉地图设计和剧情都相当不错，就是优化有点问题，我的3070都有点卡。你们的体验如何？
          </p>
          <div class="flex justify-between text-slate-500 mt-4 max-w-sm">
            <button class="flex items-center space-x-2 hover:text-sky-400">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"></path></svg>
              <span>125</span>
            </button>
            <button class="flex items-center space-x-2 hover:text-green-400">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h5M20 20v-5h-5M4 20h5v-5M20 4h-5v5"></path></svg>
              <span>32</span>
            </button>
            <button class="flex items-center space-x-2 hover:text-red-400">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"></path></svg>
              <span>1.1k</span>
            </button>
            <button class="flex items-center space-x-2 hover:text-yellow-400">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"></path></svg>
            </button>
          </div>
        </div>
      </article>
    </div>
  </main>
</template>
