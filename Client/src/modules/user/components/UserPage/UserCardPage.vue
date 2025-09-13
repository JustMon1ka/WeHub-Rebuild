<script setup lang="ts">
import UserInfo from '@/modules/user/scripts/UserInfo.ts'
import styles from '@/modules/user/scripts/Styles.ts'
import PlaceHolder from '@/modules/user/components/PlaceHolder.vue'
import FollowButton from '@/modules/user/components/UserList/FollowButton.vue'
import { computed } from 'vue'

const { userInfo } = defineProps<{
  userInfo: UserInfo;
}>();

const emit = defineEmits<{
  (e: 'editProfile'): void;
  (e: 'toFollowing'): void;
  (e: 'toFollower'): void;
}>();

function numberFormat(num: number): string {
  if (num >= 1000000) {
    return (num / 1000000).toFixed(1) + 'M';
  } else if (num >= 1000) {
    return (num / 1000).toFixed(1) + 'K';
  }
  return num.toString();
}

const formattedFollowerCount = computed(() => numberFormat(userInfo.followerCount));
const formattedFollowingCount = computed(() => numberFormat(userInfo.followingCount));
</script>

<template>
  <!--个人资料区 -->
  <div>
    <!-- 背景横幅 -->
    <div class="h-72 bg-slate-700">
      <img v-if="!!userInfo.profileUrl" v-bind:src="userInfo.profileUrl"
           class="w-full h-full object-cover" alt="Profile banner">
      <img v-else src="@/modules/user/assets/default_background.svg"
           class="w-full h-full object-cover" alt="Profile banner">
    </div>
    <!-- 头像和操作按钮 -->
    <div class="px-4 -mt-16">
      <div class="flex justify-between items-end">
        <div class="w-32 h-32 rounded-full border-4 border-slate-900 bg-slate-800">
          <img v-if="!!userInfo.avatarUrl" v-bind:src="userInfo.avatarUrl"
               v-bind:class="styles.userPic" alt="User avatar">
          <PlaceHolder v-else width="150"  height="150" :text="userInfo.nickname"
                       v-bind:class="styles.userPic"></PlaceHolder>
        </div>
        <button v-if="userInfo.isMe" @click="$emit('editProfile')"
                v-bind:class="styles.btnShape + styles.normalBtn">
          编辑个人资料
        </button>
        <FollowButton v-else :user-id="userInfo.userId.toString()" class="w-24"
                      @followed="userInfo.followerCount += 1"
                      @unfollowed="userInfo.followerCount -= 1" />
      </div>
    </div>

    <!-- 用户信息 -->
    <div class="p-4">
      <h2 class="text-2xl font-bold"> {{ userInfo.nickname }}</h2>
      <p class="text-slate-500">@{{ userInfo.username }}</p>
      <p class="mt-4">📃 {{ !!userInfo.bio ? userInfo.bio : "这个用户很神秘，什么也没写~"}}</p>
      <div class="flex items-center space-x-4 mt-4 text-slate-500 text-sm">
        <span>📍 {{ !!userInfo.location ? userInfo.location : "不告诉你哦~" }}</span>
        <span>🎂 {{ userInfo.birthday }} </span>
        <span>📅 创建于 {{ userInfo.createdAt }}  </span>
      </div>
      <div class="flex items-center space-x-4 mt-4 text-slate-500 text-sm">

      </div>
      <div class="flex items-center space-x-6 mt-4">
        <button @click="$emit('toFollowing')" class="hover:underline">
          <span class="font-bold text-white">{{ formattedFollowingCount }}</span>
          <span class="text-slate-500">正在关注</span>
        </button>
        <button @click="$emit('toFollower')" class="hover:underline">
          <span class="font-bold text-white">{{ formattedFollowerCount }}</span>
          <span class="text-slate-500">关注者</span>
        </button>
      </div>
    </div>
  </div>
</template>
