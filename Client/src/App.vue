<!--APP.vue 中的内容会应用于全局，谨慎添加！！！-->
<script setup lang="ts">
import NavigationBar from '@/modules/core/components/NavigationBar.vue'
import NavigationBarMobile from '@/modules/core/components/NavigationBarMobile.vue'
import RecommendBar from '@/modules/core/components/RecommendBar.vue'
import { RouterView } from 'vue-router'
import LoginHover from '@/modules/auth/views/LoginHover.vue'
</script>

<script lang="ts">
import { ref } from 'vue'

const showHoverLogin = ref(false);

export function toggleLoginHover() {
  showHoverLogin.value = !showHoverLogin.value;
}
</script>

<template>

<body>
  <LoginHover v-model:show-hover="showHoverLogin" id="login-hover" class="fixed overflow-hidden"/>
  <div class="fixed bg-slate-900 text-slate-200 w-screen h-screen md:overflow-hidden overflow-y-auto">
    <div class="w-screen h-screen justify-center flex flex-col-reverse overflow-y-auto md:flex-row  md:overflow-hidden">
      <NavigationBar id="navigation-pc"  class="flex-none md:py-5 md:px-5 sr-only md:not-sr-only border-x border-slate-800"/>
      <RouterView id="main" class="flex-auto h-dvh overflow-y-auto"/>
      <RecommendBar id="recommend" class="flex-none h-24 overflow-hidden md:h-screen px-5 py-5"/>
    </div>
    <!-- 移动端底部导航栏 -->
    <nav class="md:hidden fixed bottom-0 left-0 right-0 z-9999 bg-slate-900 border-t border-slate-800 flex justify-around p-2">
      <NavigationBarMobile id="navigation-mobile" />
    </nav>
  </div>
</body>
</template>
