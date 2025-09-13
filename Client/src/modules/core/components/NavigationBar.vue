<template>
  <div
    class="fixed left-0 top-0 bottom-0 w-64 flex flex-col h-full z-30 bg-slate-900 p-4 border-r border-slate-800"
  >
    <!-- Logo -->
    <div class="h-16 flex items-center m-2 mb-4">
      <img src="@/assets/logo.svg" alt="Logo" class="w-9 h-9" />
      <span class="ml-2 text-xl font-bold p-2">WeHub</span>
    </div>
    <!-- 导航菜单 -->
    <nav class="flex-grow">
      <ul class="space-y-2">
        <li>
          <router-link to="/" class="router-link">
            <img src="@/modules/core/assets/home.svg" alt="Home" class="svg" />
            首页
          </router-link>
        </li>
        <li>
          <router-link to="/founding" class="router-link">
            <img src="@/modules/core/assets/discover.svg" alt="Discover" class="svg" />
            发现
          </router-link>
        </li>
        <li>
          <router-link to="/community" class="router-link">
            <img src="@/modules/core/assets/community.svg" alt="Community" class="svg" />
            社区
          </router-link>
        </li>
        <li>
          <router-link to="/notice" class="router-link">
            <img src="@/modules/core/assets/notifications.svg" alt="Notifications" class="svg" />
            通知
          </router-link>
        </li>
        <li>
          <router-link to="/message" class="router-link">
            <img src="@/modules/core/assets/messages.svg" alt="Messages" class="svg" />
            私信
          </router-link>
        </li>
        <li>
          <router-link to="/user_page/Me" class="router-link">
            <img src="@/modules/core/assets/profile.svg" alt="Profile" class="svg" />
            个人
          </router-link>
        </li>
      </ul>
      <!-- 发布新帖子按钮 -->
      <div class="my-4">
        <button
          ref="postButton"
          @click="openPostCreate"
          data-post-btn
          class="w-full flex items-center justify-center p-3 rounded-full bg-slate-800 hover:bg-slate-700 text-white font-semibold transition-colors duration-200"
        >
          <img src="@/modules/core/assets/add.svg" alt="Create Post" class="w-5 h-5 mr-2" />
          新帖子
        </button>
      </div>
    </nav>

    <div>
      <div v-if="userInfo?.value.profileLoaded" class="mt-auto user relative">
        <div
          class="bg-slate-950/20 backdrop-blur-lg border-2 border-slate-800 rounded-2xl p-2 absolute bottom-16 logout"
        >
          <button
            @click.prevent="User.getInstance()?.logout()"
            class="bg-red-500 text-slate-50 font-bold p-1 m-2 w-30 rounded-full text-l"
          >
            登出
          </button>
        </div>
        <router-link to="/user_page/Me" class="flex items-center py-3 rounded-full">
          <img
            v-if="!!userInfo?.value.avatarUrl"
            :src="userInfo?.value.avatarUrl"
            class="w-12 h-12 rounded-full"
            alt="User Avatar"
          />
          <PlaceHolder
            v-else
            width="100"
            :text="userInfo?.value.nickname"
            height="100"
            class="w-12 h-12 rounded-full"
          />
          <div class="ml-3">
            <p class="font-semibold text-sm truncate w-20">{{ userInfo?.value.nickname }}</p>
            <p class="text-slate-400 text-xs">@{{ userInfo?.value.username }}</p>
          </div>
        </router-link>
      </div>

      <div v-else class="mt-auto relative">
        <router-link to="/login" class="flex items-center py-3 rounded-full">
          <div class="w-12 h-12 bg-black border-2 border-slate-700 rounded-full" />
          <div class="ml-3">
            <p class="font-semibold text-l">登录</p>
            <p class="text-slate-400 text-xs">点击登录</p>
          </div>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { type Ref, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { User } from '@/modules/auth/public.ts'
import { UserInfo } from '@/modules/user/public.ts'
import PlaceHolder from '@/modules/user/components/PlaceHolder.vue'

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
        y: (rect.top + rect.height / 2).toString(),
      },
    })
  } else {
    router.push({ name: 'post-create' })
  }
}

let userInfo: Ref<Ref<UserInfo> | undefined> = ref(undefined)

if (User.loading) {
  User.afterLoadCallbacks.push(() => {
    userInfo.value = User.getInstance()?.userInfo
  })
} else {
  userInfo.value = User.getInstance()?.userInfo
}
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

.user:hover .logout {
  display: block;
}

.logout:hover {
  display: block;
}

.logout {
  display: none;
}
</style>
