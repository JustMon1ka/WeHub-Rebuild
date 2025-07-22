<template>
  <div class="about flex flex-col items-center justify-center p-4">
    <div class="flex flex-col items-center justify-center p-6 m-6">
      <h1 class="font-bold text-4xl">This is an about page</h1>
      <p class="text-slate-400 mt-2">欢迎来到 WeHub 社交平台！</p>
    </div>
    <div class="flex flex-col items-center justify-center p-4">
      <p>本项目旨在开发一个社交平台，提供用户注册、登录、发帖、评论等功能。</p>
      <p>使用 Vue 3、TypeScript、Pinia、Vue Router、Axios 等技术栈。</p>
      <p>后端使用 Node.js、Express、MongoDB 等技术栈。（我不知道，我是写前端的）</p>
      <p>如果您对本项目感兴趣，欢迎参与开发！项目地址：</p>
      <p>https://github.com/JustMon1ka/Summer2025-SE-DB-WeHub</p>
    </div>

  </div>
</template>

<style>
@media (min-width: 1024px) {
  .about {
    min-height: 100vh;
    display: flex;
    align-items: center;
  }
}
</style>
<script setup lang="ts">
</script>
