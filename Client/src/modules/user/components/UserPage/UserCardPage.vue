<script setup lang="ts">
import UserInfo from '@/modules/user/scripts/UserInfo.ts'
import styles from '@/modules/user/scripts/Styles.ts'
import { type Ref } from 'vue'
import PlaceHolder from '@/modules/user/components/PlaceHolder.vue'

const userInfo : Ref<UserInfo> = defineModel<UserInfo>('userInfo', { required: true });

const emit = defineEmits<{
  (e: 'editProfile'): void;
  (e: 'toFollowing'): void;
  (e: 'toFollower'): void;
}>();

</script>

<template>
  <!--个人资料区 -->
  <div>
    <!-- 背景横幅 -->
    <div class="h-72 bg-slate-700">
      <img v-if="!!userInfo.profilePictureUrl" v-bind:src="userInfo.profilePictureUrl"
           class="w-full h-full object-cover" alt="Profile banner">
      <img v-else src="@/modules/user/assets/default_background.svg"
           class="w-full h-full object-cover" alt="Profile banner">
    </div>
    <!-- 头像和操作按钮 -->
    <div class="px-4 -mt-16">
      <div class="flex justify-between items-end">
        <div class="w-32 h-32 rounded-full border-4 border-slate-900 bg-slate-800">
          <img v-if="!!userInfo.userAvatarUrl" v-bind:src="userInfo.userAvatarUrl"
               v-bind:class="styles.userPic" alt="User avatar">
          <PlaceHolder width="150"  height="150" :text="userInfo.nickName"
                       v-bind:class="styles.userPic"></PlaceHolder>
        </div>
        <button v-if="userInfo.isMe" @click="$emit('editProfile')"
                v-bind:class="styles.btnShape + styles.normalBtn">
          编辑个人资料
        </button>

      </div>
    </div>

    <!-- 用户信息 -->
    <div class="p-4">
      <h2 class="text-2xl font-bold"> {{ userInfo.nickName }}</h2>
      <p class="mt-4">📃 {{ !!userInfo.bio ? userInfo.bio : "这个用户很神秘，什么也没写~"}}</p>
      <div class="flex items-center space-x-4 mt-4 text-slate-500 text-sm">
        <span>📍 {{ !!userInfo.address ? userInfo.address : "不告诉你哦~" }}</span>
        <span>🎂 {{ userInfo.birthday }} </span>
      </div>
      <div class="flex items-center space-x-6 mt-4">
        <button @click="$emit('toFollowing')" class="hover:underline">
          <span class="font-bold text-white">{{ userInfo.followingCount }}</span>
          <span class="text-slate-500">正在关注</span>
        </button>
        <button @click="$emit('toFollower')" class="hover:underline">
          <span class="font-bold text-white">{{ userInfo.followersCount }}</span>
          <span class="text-slate-500">关注者</span>
        </button>
        <!--TODO: 接入关注者被关注者-->
      </div>
    </div>
  </div>
</template>
