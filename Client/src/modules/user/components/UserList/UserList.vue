<script setup lang="ts">
</script>

<template>
  <!-- 用户列表 -->
  <div class="divide-y divide-slate-800">

    <!-- 用户卡片 2 -->
    <div class="p-4 flex items-start space-x-4 hover:bg-slate-800/50 transition-colors duration-200">
      <img class="w-12 h-12 rounded-full" src="https://placehold.co/100x100/a78bfa/1e1b4b?text=A" alt="User Avatar">
      <div class="flex-1">
        <div class="flex items-center justify-between">
          <div>
            <a href="#" class="font-bold hover:underline">user_gamer</a>
            <p class="text-sm text-slate-500">@user_gamer</p>
          </div>
          <button class="following-btn border-2 border-slate-600 hover:border-red-500/50 hover:bg-red-500/10 hover:text-red-500 font-bold text-sm py-2 px-4 rounded-full transition-colors duration-200">
            <span class="text-following">正在关注</span>
            <span class="text-unfollow">取消关注</span>
          </button>
        </div>
        <p class="mt-2 text-slate-300">游戏爱好者，主玩主机游戏。</p>
      </div>
    </div>
    <!-- 用户卡片 3 -->
    <div class="p-4 flex items-start space-x-4 hover:bg-slate-800/50 transition-colors duration-200">
      <img class="w-12 h-12 rounded-full" src="https://placehold.co/100x100/f87171/7f1d1d?text=M" alt="User Avatar">
      <div class="flex-1">
        <div class="flex items-center justify-between">
          <div>
            <a href="#" class="font-bold hover:underline">社区管理员</a>
            <p class="text-sm text-slate-500">@admin</p>
          </div>
          <button class="bg-slate-200 hover:bg-white text-slate-900 font-semibold text-sm py-2 px-5 rounded-full transition-colors duration-200">关注</button>
        </div>
        <p class="mt-2 text-slate-300">负责维护社区秩序。</p>
      </div>
    </div>
  </div>
</template>
