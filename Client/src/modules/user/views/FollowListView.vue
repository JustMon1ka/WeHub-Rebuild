<script setup lang="ts">
import UserList from '@/modules/user/components/UserList/UserList.vue'
</script>

<template>
  <main class="col-span-1 md:col-span-8 lg:col-span-7 border-x border-slate-800 min-h-screen">
    <!-- 页面头部 -->
    <div class="sticky top-0 z-10 bg-slate-900/80 backdrop-blur-md flex items-center p-4 border-b border-slate-800">
      <button class="p-2 rounded-full hover:bg-slate-800 mr-4">
        <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"></path></svg>
      </button>
      <div>
        <h1 class="text-xl font-bold">用户名</h1>
        <p class="text-sm text-slate-500">@username</p>
      </div>
    </div>

    <!-- 内容切换 Tab -->
    <div class="flex border-b border-slate-800">
      <a href="#" class="w-1/2 text-center p-4 font-semibold border-b-2 border-sky-500">关注者</a>
      <a href="#" class="w-1/2 text-center p-4 text-slate-400 hover:bg-slate-800 transition-colors duration-200">正在关注</a>
    </div>

    <UserList/>
  </main>
</template>
