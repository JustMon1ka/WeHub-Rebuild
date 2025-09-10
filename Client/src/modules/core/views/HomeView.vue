<script setup lang="ts">
import { loginForm } from "@/modules/auth/public.ts";
import { SearchBar } from "@/modules/post/public.ts";
</script>

<template>
  <!-- 顶部固定搜索栏 -->
  <div class="fixed top-0 left-0 w-full z-50 bg-slate-900 p-4">
    <!-- 内部容器宽度可控，mx-auto 保证水平居中 -->
    <div class="mx-auto w-3/10 h-12">
      <SearchBar class="w-full h-full"/>
    </div>
  </div>

  <!-- 主内容区 -->
  <div class="pt-20 flex flex-col px-6 flex-auto h-dvh overflow-y-auto">
    <loginForm class="mx-auto mt-6"/>
    <!-- 页面其他内容 -->
  </div>
</template>
