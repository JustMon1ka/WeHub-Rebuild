<!--APP.vue 中的内容会应用于全局，谨慎添加！！！-->

<script setup lang="ts">
import NavigationBar from '@/modules/core/components/NavigationBar.vue'
import NavigationBarMobile from '@/modules/core/components/NavigationBarMobile.vue'
import RecommendBar from '@/modules/core/components/RecommendBar.vue'
import { RouterView, useRoute } from 'vue-router'
import PostCreate from '@/modules/postCreate/views/CreatePostView.vue'

const route = useRoute()
</script>

<template>
<body class="bg-slate-900 text-slate-200 w-screen h-screen md:overflow-hidden overflow-y-auto">
<!-- 侧边栏固定 -->
  <NavigationBar id="navigation-pc" class="z-100"/>

  <!-- 主内容 + 推荐栏 的容器，整体往右错开 4rem -->
  <div class="flex flex-col-reverse md:flex-row w-full h-full pl-64 overflow-hidden">
    <!-- 主内容区 -->
    <RouterView
      id="main"
      class="flex-auto h-dvh overflow-y-auto px-10"
    />

    <!-- 推荐栏 -->
    <RecommendBar
      id="recommend"
      class="flex-none h-24 md:h-full p-10 z-100"
    />
  </div>

<!--   全局悬浮层：通过 Teleport 挂在 body 外层-->
  <teleport to="body">
    <!-- 只有在路由 name 为 post-create 时渲染 -->
    <PostCreate v-if="route.name === 'post-create'" />
  </teleport>

  <!-- 移动端底部导航栏 -->
  <nav class="md:hidden fixed bottom-0 left-0 right-0 z-9999 bg-slate-900 border-t border-slate-800 flex justify-around p-2">
    <NavigationBarMobile id="navigation-mobile" />
  </nav>
</body>
</template>

