

<template>
  <!-- 页面头部 -->
  <div class="p-4 border-b border-slate-800">
    <h1 class="text-xl font-bold">用户名</h1>
    <p class="text-sm text-slate-500">1,234 帖子</p>
  </div>

  <!-- 个人资料区 -->
  <div>
    <!-- 背景横幅 -->
    <div class="h-48 bg-slate-700">
      <img src="https://placehold.co/600x200/1e293b/94a3b8?text=Banner" class="w-full h-full object-cover" alt="Profile banner">
    </div>
    <!-- 头像和操作按钮 -->
    <div class="px-4 -mt-16">
      <div class="flex justify-between items-end">
        <div class="w-32 h-32 rounded-full border-4 border-slate-900 bg-slate-800">
          <img src="https://placehold.co/150x150/7dd3fc/0f172a?text=头像" class="w-full h-full rounded-full object-cover" alt="User avatar">
        </div>
        <button class="border-2 border-slate-600 hover:bg-slate-800 font-bold py-2 px-4 rounded-full transition-colors duration-200">
          编辑个人资料
        </button>
      </div>
    </div>
    <!-- 用户信息 -->
    <div class="p-4">
      <h2 class="text-2xl font-bold">用户名</h2>
      <p class="text-slate-500">@username</p>
      <p class="mt-4">一个热爱游戏、代码和摄影的普通人。欢迎交流！</p>
      <div class="flex items-center space-x-4 mt-4 text-slate-500 text-sm">
        <span>📍 上海</span>
        <a href="#" class="text-sky-400 hover:underline">example.com</a>
        <span>📅 2022年1月 加入</span>
      </div>
      <div class="flex items-center space-x-6 mt-4">
        <p><span class="font-bold text-white">105</span> <span class="text-slate-500">正在关注</span></p>
        <p><span class="font-bold text-white">1,580</span> <span class="text-slate-500">关注者</span></p>
      </div>
    </div>
  </div>
</template>
