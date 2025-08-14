<template>
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

    <!-- 表单 -->
    <form class="p-4 space-y-6">
      <div>
        <label for="name" class="block text-sm font-medium text-slate-400">昵称</label>
        <input type="text" id="name" value="用户名" class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-sky-500 focus:border-sky-500 sm:text-sm">
      </div>
      <div>
        <label for="bio" class="block text-sm font-medium text-slate-400">个人简介</label>
        <textarea id="bio" rows="3" class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-sky-500 focus:border-sky-500 sm:text-sm">一个热爱游戏、代码和摄影的普通人。欢迎交流！</textarea>
      </div>
      <div>
        <label for="location" class="block text-sm font-medium text-slate-400">位置</label>
        <input type="text" id="location" value="上海" class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-sky-500 focus:border-sky-500 sm:text-sm">
      </div>
      <div>
        <label for="website" class="block text-sm font-medium text-slate-400">网站</label>
        <input type="url" id="website" value="https://example.com" class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-sky-500 focus:border-sky-500 sm:text-sm">
      </div>
    </form>
  </div>
</template>
