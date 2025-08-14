<script setup lang="ts">
import UserInfoForm from '@/modules/user/components/UserPage/UserInfoForm.vue'
</script>

<template>
  <main class="col-span-1 md:col-span-8 lg:col-span-7 border-x border-slate-800 min-h-screen">
    <!-- 页面头部 -->
    <div class="sticky top-0 z-10 bg-slate-900/80 backdrop-blur-md flex items-center p-4 border-b border-slate-800">
      <button class="p-2 rounded-full hover:bg-slate-800 mr-4">
        <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
      </button>
      <h1 class="text-xl font-bold flex-grow">编辑个人资料</h1>
      <button class="bg-white hover:bg-slate-200 text-slate-900 font-bold py-2 px-5 rounded-full transition-colors duration-200">
        保存
      </button>
    </div>

    <!-- 个人资料编辑区 -->
    <div>
      <!-- 背景横幅 -->
      <div class="h-48 bg-slate-700 relative">
        <img src="https://placehold.co/600x200/1e293b/94a3b8?text=Banner" class="w-full h-full object-cover" alt="Profile banner">
        <div class="absolute inset-0 bg-black/50 flex items-center justify-center">
          <button class="p-3 bg-slate-900/50 rounded-full hover:bg-slate-900/75 transition-colors">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 13a3 3 0 11-6 0 3 3 0 016 0z"></path></svg>
          </button>
        </div>
      </div>
      <!-- 头像 -->
      <div class="px-4 -mt-16">
        <div class="w-32 h-32 rounded-full border-4 border-slate-900 bg-slate-800 relative">
          <img src="https://placehold.co/150x150/7dd3fc/0f172a?text=头像" class="w-full h-full rounded-full object-cover" alt="User avatar">
          <div class="absolute inset-0 bg-black/50 flex items-center justify-center rounded-full">
            <button class="p-3 bg-slate-900/50 rounded-full hover:bg-slate-900/75 transition-colors">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 13a3 3 0 11-6 0 3 3 0 016 0z"></path></svg>
            </button>
          </div>
        </div>
      </div>

      <UserInfoForm/>
    </div>
  </main>
</template>
