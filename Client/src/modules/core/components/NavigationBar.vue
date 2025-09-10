<template>
  <div class="fixed left-0 top-0 bottom-0 w-64 flex flex-col h-full z-30 bg-slate-900 p-4 border-r border-slate-800">
<!--    flex-none sr-only md:not-sr-only md:h-screen border-x border-slate-800 p-4-->
    <!-- Logo -->

    <div class="h-16 flex items-center mb-4">
      <img src="@/assets/logo.svg" alt="Logo" class="w-9 h-9">
      <span class="ml-2 text-xl font-bold">WeHub</span>
    </div>
    <!-- 导航菜单 -->
    <nav class="flex-grow">
      <ul class="space-y-2">
        <li><router-link to="/" class="flex items-center p-3 rounded-full hover:bg-slate-800 transition-colors duration-200">
          <svg class="w-6 h-6 mr-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"></path></svg>
          首页
        </router-link></li>
        <li><a href="#" class="flex items-center p-3 rounded-full hover:bg-slate-800 transition-colors duration-200">
          <svg class="w-6 h-6 mr-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 20l4-16m2 16l4-16M6 9h14M4 15h14"></path></svg>
          发现
        </a></li>
        <li><a href="#" class="flex items-center p-3 rounded-full hover:bg-slate-800 transition-colors duration-200">
          <svg class="w-6 h-6 mr-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path></svg>
          社区
        </a></li>
        <li><a href="#" class="flex items-center p-3 rounded-full hover:bg-slate-800 transition-colors duration-200">
          <svg class="w-6 h-6 mr-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"></path></svg>
          通知
        </a></li>
        <li><a href="#" class="flex items-center p-3 rounded-full bg-slate-800 font-semibold">
          <svg class="w-6 h-6 mr-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path></svg>
          私信
        </a></li>
        <li><router-link :to="{ name: 'UserPage', params: { userId_p: 'Me' } }"
                         class="flex items-center p-3 rounded-full hover:bg-slate-800 transition-colors duration-200">
          <svg class="w-6 h-6 mr-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
          个人
        </router-link></li>
      </ul>
    </nav>

    <!-- 发布新帖子按钮 -->
    <div class="my-4 px-2">
      <button
        ref="postButton"
        @click="openPostCreate"
        data-post-btn
        class="w-full flex items-center justify-center p-3 rounded-full bg-slate-800 hover:bg-slate-700 text-white font-semibold transition-colors duration-200"
      >
        <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M12 4v16m8-8H4"/>
        </svg>
        发布新帖子
      </button>
    </div>

    <div class="mt-auto">
      <a
        href="#"
        class="flex items-center p-3 rounded-full hover:bg-slate-700/50 transition-colors duration-200"
      >
        <img
          class="w-10 h-10 rounded-full"
          src="https://placehold.co/100x100/7dd3fc/0f172a?text=头像"
          alt="User Avatar"
        />
        <div class="ml-3">
          <p class="font-semibold text-sm">用户名</p>
          <p class="text-slate-400 text-xs">@username</p>
        </div>
        <svg
          class="w-5 h-5 ml-auto text-slate-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M5 12h.01M12 12h.01M19 12h.01M6 12a1 1 0 11-2 0 1 1 0 012 0zm7 0a1 1 0 11-2 0 1 1 0 012 0zm7 0a1 1 0 11-2 0 1 1 0 012 0z"
          ></path>
        </svg>
      </a>
    </div>

    <button v-if="logout" @click.prevent="User.getInstance()?.logout()" class=" bg-slate-800 text-slate-300 font-bold hover:bg-slate-700 transition-colors p-1 m-2 w-30 rounded-full text-xl ">
      退出
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'

const router = useRouter()
const route = useRoute()

const postButton = ref<HTMLButtonElement | null>(null)

const openPostCreate = () => {
  if (route.name === 'post-create') {
    window.dispatchEvent(new Event('close-post-create'))
    return
  }

  if (postButton.value) {
    const rect = postButton.value.getBoundingClientRect()
    router.push({
      name: 'post-create',
      query: {
        x: rect.right.toString(),
        y: (rect.top + rect.height / 2).toString()
      }
    })
  } else {
    router.push({ name: 'post-create' })
  }
}
import { User } from '@/modules/auth/public.ts'

const logout = ref(false);
User.afterLoadCallbacks.push(() => {
  if (User.getInstance()) {
    logout.value = true;
  }
});
</script>

<style scoped>
</style>
