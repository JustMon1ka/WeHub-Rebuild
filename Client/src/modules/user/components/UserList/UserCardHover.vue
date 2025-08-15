<script setup lang="ts">

import type { Ref } from 'vue'
import UserInfo from '@/modules/user/scripts/UserInfo.ts'
import PlaceHolder from '@/modules/user/components/PlaceHolder.vue'

const userInfo : Ref<UserInfo> = defineModel<UserInfo>('userInfo', { required: true });

</script>

<template>
  <div class="w-sm h-1/2  bg-slate-900/70 backdrop-blur-md rounded-lg inset-shadow-sm inset-shadow-slate-500/50 border border-slate-800">
    <!-- 悬浮窗 -->
    <div class="p-4">
      <div class="flex justify-between items-start">
        <img v-if="!!userInfo.userAvatarUrl" :src="userInfo.userAvatarUrl"
             class="w-12 h-12 rounded-full" alt="User Avatar">
        <PlaceHolder v-else width="100" :text="userInfo.nickName" height="100" class="w-12 h-12 rounded-full"/>
        <!-- TODO：正确跳转 -->
        <router-link to="user_page" class="border-2 text-white border-slate-600 hover:bg-slate-700 font-bold py-1 px-4 rounded-full transition-colors duration-200 text-sm">
          查看资料
        </router-link>
      </div>
      <div class="mt-4">
        <h3 class="font-bold text-lg text-white">{{ userInfo.nickName }}</h3>
        <p class="mt-2 text-slate-300 overflow-hidden">📃 {{ !!userInfo.bio ? userInfo.bio : "这个用户很神秘，什么也没写~"}}</p>
        <div class="flex items-center space-x-4 mt-3 text-sm text-slate-400">
          <span>📍 {{ !!userInfo.address ? userInfo.address : "不告诉你哦~" }}</span>
          <span>🎂 {{ userInfo.birthday }} </span>
        </div>
        <div class="flex items-center space-x-6 mt-4">
          <p><span class="font-bold text-white">{{ userInfo.followingCount }}</span>  <span class="text-slate-500">正在关注</span></p>
          <p><span class="font-bold text-white">{{ userInfo.followerCount }}</span>  <span class="text-slate-500">关注者</span></p>
        </div>
      </div>
    </div>
    <div class="border-t border-slate-700 p-3 flex justify-center">
      <button class="w-full bg-sky-500 hover:bg-sky-600 text-white font-semibold py-2 px-4 rounded-full transition-colors duration-200">
        关注
      </button>
    </div>
  </div>
</template>
