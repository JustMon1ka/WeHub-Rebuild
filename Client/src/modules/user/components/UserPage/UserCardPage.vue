<script setup lang="ts">
import UserInfo from '@/modules/user/scripts/UserInfo.ts'
import styles from '@/modules/user/scripts/Styles.ts'
import type { Ref } from 'vue'

const userInfo : Ref<UserInfo> = defineModel<UserInfo>('userInfo', { required: true });
</script>

<template>
  <!-- 个人资料区 -->
  <div>
    <!-- 背景横幅 -->
    <div class="h-72 bg-slate-700">
      <img v-if="!!userInfo?.profilePictureUrl?.value" v-bind:src="userInfo?.profilePictureUrl?.value"
           class="w-full h-full object-cover" alt="Profile banner">
      <img v-else src="@/modules/user/assets/default_background.svg"
           class="w-full h-full object-cover" alt="Profile banner">
    </div>
    <!-- 头像和操作按钮 -->
    <div class="px-4 -mt-16">
      <div class="flex justify-between items-end">
        <div class="w-32 h-32 rounded-full border-4 border-slate-900 bg-slate-800">
          <img v-if="!!userInfo?.userAvatarUrl?.value" v-bind:src="userInfo?.userAvatarUrl?.value"
               v-bind:class="styles.userPic" alt="User avatar">
          <img v-else src="@/modules/user/assets/default_user.svg"
               v-bind:class="styles.userPic" alt="User avatar">
        </div>
        <button @click="$emit('editProfile')"
                class="border-2 border-slate-600 hover:bg-slate-800 font-bold py-2 px-4 rounded-full transition-colors duration-200">
          编辑个人资料
        </button>
      </div>
    </div>

    <!-- 用户信息 -->
    <div class="p-4">
      <h2 class="text-2xl font-bold">{{ userInfo.userName }}</h2>
      <p class="mt-4"> {{ !!userInfo?.bio?.value ? userInfo.bio.value : "暂无简介"}}</p>
      <div class="flex items-center space-x-4 mt-4 text-slate-500 text-sm">
        <span v-if="!!userInfo?.address?.value">📍 {{ userInfo.address.value }}</span>
        <a v-if="!userInfo?.website?.value" v-bind:href="userInfo?.website?.value" class="text-sky-400 hover:underline">{{ userInfo?.website?.value }}</a>
        <span>📅 {{ userInfo.createdAt }} </span>
      </div>
      <div class="flex items-center space-x-6 mt-4">
        <p>
          <span class="font-bold text-white">{{ userInfo.followingCount }}</span>
          <span class="text-slate-500">正在关注</span>
        </p>
        <p>
          <span class="font-bold text-white">{{ userInfo.followersCount }}</span>
          <span class="text-slate-500">关注者</span>
        </p>
        <!--TODO: 接入关注者被关注者-->
      </div>
    </div>
  </div>
</template>
