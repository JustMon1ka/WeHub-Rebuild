<!--APP.vue 中的内容会应用于全局，谨慎添加！！！-->
<script setup lang="ts">
import { ref } from 'vue'
import { RouterView, useRoute } from 'vue-router'

import NavigationBar from '@/modules/core/components/NavigationBar.vue'
import NavigationBarMobile from '@/modules/core/components/NavigationBarMobile.vue'
import PostCreate from '@/modules/postCreate/views/CreatePostView.vue'
import LoginHover from '@/modules/auth/views/LoginHover.vue'
import RightAside from '@/modules/core/components/RightAside.vue'

const route = useRoute()
</script>

<script lang="ts">
export const showHoverLogin = ref(false)
export const showNavigationBar = ref(true)
export const showRecommendBar = ref(true)
</script>

<template>
  <div class="bg-slate-900 text-slate-200 w-screen h-screen md:overflow-hidden overflow-y-auto">
    <LoginHover v-model:show-hover="showHoverLogin" id="login-hover" class="fixed overflow-hidden z-9999"/>
    <div class="w-screen h-screen justify-center flex flex-col-reverse overflow-y-auto md:flex-row md:overflow-hidden">
      <NavigationBar v-show="showNavigationBar" id="navigation-pc" class="flex-none md:py-5 md:px-5 sr-only md:not-sr-only border-x border-slate-800 z-100"/>
      <div id="main" class="overflow-hidden w-full">
        <RouterView class="flex-auto h-dvh overflow-y-auto" />
        <PostCreate v-if="route.name === 'post-create'" class="h-dvh overflow-y-auto"/>
      </div>
      <RightAside
        v-show="showRecommendBar && route.name !== 'founding'"
        id="recommend-bar"
        class="flex-none h-24 overflow-hidden md:h-screen z-100"
      />
    </div>

    <!-- 移动端底部导航栏 -->
    <nav class="md:hidden fixed bottom-0 left-0 right-0 z-9999 bg-slate-900 border-t border-slate-800 flex justify-around p-2">
      <NavigationBarMobile id="navigation-mobile" />
    </nav>
  </div>
</template>
