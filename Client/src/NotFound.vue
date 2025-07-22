<script setup lang = "ts">

</script>

<template>
  <div class="flex items-center justify-center h-screen bg-slate-900 text-slate-200">
    <div class="text-center">
      <h1 class="text-5xl font-bold mb-4">404</h1>
      <p class="text-lg mb-6">页面未找到</p>
      <p class="text-slate-400 mb-8">抱歉，您访问的页面不存在或已被删除。</p>
      <router-link to="/" class="bg-slate-800 hover:bg-slate-700 text-white font-semibold py-2 px-4 rounded transition-colors duration-200">
        返回首页
      </router-link>
    </div>
  </div>
</template>
