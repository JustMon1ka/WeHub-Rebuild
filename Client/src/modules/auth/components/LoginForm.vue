<script setup lang="ts">
</script>

<template>
  <!-- 登录表单 -->
  <div class="bg-slate-800 p-8 rounded-2xl shadow-lg">
    <form class="space-y-6">
      <div>
        <label for="email" class="block text-sm font-medium text-slate-300">邮箱或用户名</label>
        <input type="email" id="email" name="email" placeholder="you@example.com" required class="mt-1 block w-full bg-slate-700/50 border border-slate-600 rounded-md shadow-sm py-3 px-4 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-sky-500 sm:text-sm">
      </div>

      <div>
        <div class="flex justify-between items-baseline">
          <label for="password" class="block text-sm font-medium text-slate-300">密码</label>
          <a href="#" class="text-sm text-sky-400 hover:underline">忘记密码？</a>
        </div>
        <input type="password" id="password" name="password" required class="mt-1 block w-full bg-slate-700/50 border border-slate-600 rounded-md shadow-sm py-3 px-4 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-sky-500 sm:text-sm">
      </div>

      <div>
        <button type="submit" class="w-full flex justify-center py-3 px-4 border border-transparent rounded-full shadow-sm text-sm font-bold text-white bg-sky-500 hover:bg-sky-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-slate-900 focus:ring-sky-500 transition-colors duration-200">
          登 录
        </button>
      </div>
    </form>

    <div class="mt-6 text-center">
      <p class="text-sm text-slate-400">
        还没有账户？
        <a href="#" class="font-medium text-sky-400 hover:underline">
          立即注册
        </a>
      </p>
    </div>
  </div>
</template>
