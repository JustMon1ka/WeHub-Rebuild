<template>
  <div class="fixed left-0 top-0 bottom-0 w-64 flex flex-col h-full z-30 bg-slate-900 p-4 border-r border-slate-800">
<!--    flex-none sr-only md:not-sr-only md:h-screen border-x border-slate-800 p-4-->
    <!-- Logo -->
    <div class="h-16 flex items-center m-2 mb-4">
      <img src="@/assets/logo.svg" alt="Logo" class="w-9 h-9">
      <span class="ml-2 text-xl font-bold p-2">WeHub</span>
    </div>
    <!-- 导航菜单 -->
    <nav class="flex-grow">
      <ul class="space-y-2">
        <li><router-link to="/mainpage" class="router-link">
          <img src="@/modules/core/assets/home.svg" alt="Home" class="svg">
          首页
        </router-link></li>
        <li><router-link to="/founding" class="router-link">
          <img src="@/modules/core/assets/discover.svg" alt="Discover" class="svg">
          发现
        </router-link></li>
        <li><router-link to="/community" class="router-link">
          <img src="@/modules/core/assets/community.svg" alt="Community" class="svg">
          社区
        </router-link></li>
        <li><router-link to="/notice" class="router-link">
          <img src="@/modules/core/assets/notifications.svg" alt="Notifications" class="svg">
          通知
        </router-link></li>
        <li><router-link to="/message" class="router-link">
          <img src="@/modules/core/assets/messages.svg" alt="Messages" class="svg">
          私信
        </router-link></li>
        <li><router-link to="/user_page/Me" class="router-link">
          <img src="@/modules/core/assets/profile.svg" alt="Profile" class="svg">
          个人
        </router-link></li>
      </ul>
    </nav>

    <!-- 发布新帖子按钮 -->
    <div class="my-4">
      <button
        ref="postButton"
        @click="openPostCreate"
        data-post-btn
        class="w-full flex items-center justify-center p-3 rounded-full bg-slate-800 hover:bg-slate-700 text-white font-semibold transition-colors duration-200"
      >
        <img src="@/modules/core/assets/add.svg" alt="Create Post" class="w-5 h-5 mr-2">
        新帖子
      </button>
    </div>

    <div class="mt-auto">
      <router-link to="/user_page/Me" class="flex items-center p-3 rounded-full hover:bg-slate-800 transition-colors duration-200">
        <img class="w-10 h-10 rounded-full" src="https://placehold.co/100x100/7dd3fc/0f172a?text=头像" alt="User Avatar">
        <div class="ml-3">
          <p class="font-semibold text-sm">用户名</p>
          <p class="text-slate-400 text-xs">@username</p>
        </div>
      </router-link>
    </div>

    <button v-if="logout" @click.prevent="User.getInstance()?.logout()" class=" bg-slate-800 text-slate-300 font-bold hover:bg-slate-700 transition-colors p-1 m-2 w-30 rounded-full text-xl ">
      退出
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { User } from '@/modules/auth/public.ts'

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

const logout = ref(false);
User.afterLoadCallbacks.push(() => {
  if (User.getInstance()) {
    logout.value = true;
  }
});
</script>

<style scoped>
.svg {
  width: 1.5rem;
  height: 1.5rem;
  margin-right: 1rem;
  fill: currentColor;
}

.router-link {
display: flex;
align-items: center;
padding: 0.75rem;
border-radius: 9999px;
transition-property: background-color;
transition-duration: 200ms;
}
.router-link:hover {
  background-color: #1e293b; /* 对应 bg-slate-800 */

}
</style>
