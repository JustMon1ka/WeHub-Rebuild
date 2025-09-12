<script setup lang="ts">
import type { RecommendUser } from "../types"
// 导入 FollowButton
import FollowButton from "@/modules/user/components/UserList/FollowButton.vue"

defineProps<{ users: RecommendUser[] }>()

// 随机头像背景色
function avatarColor(id: number) {
  const colors = ["#ec4899", "#8b5cf6", "#06b6d4", "#f59e0b", "#10b981"]
  return colors[id % colors.length]
}
</script>

<template>
  <div class="bg-slate-800 rounded-2xl p-4 w-72">
    <h2 class="text-lg font-bold mb-4">推荐关注</h2>
    <ul class="space-y-4">
      <li v-for="u in users" :key="u.user_id" class="flex items-center">
        <!-- 用户头像 -->
        <router-link
          :to="`/user_page/${u.user_id}`"
          class="w-10 h-10 rounded-full flex items-center justify-center text-white font-bold cursor-pointer"
          :style="{ backgroundColor: avatarColor(u.user_id) }"
        >
          {{ u.username.slice(0, 3) }}
        </router-link>

        <!-- 用户信息 -->
        <div class="ml-3 flex-1 overflow-hidden">
          <!-- username 可点击，hover 下划线 -->
          <router-link :to="`/user_page/${u.user_id}`">
            <p class="font-semibold text-sm truncate hover:underline">
              {{ u.username }}
            </p>
          </router-link>
          <!-- @user_id 不带下划线 -->
          <p class="text-slate-400 text-xs truncate">@{{ u.user_id }}</p>
        </div>

        <!-- 动态关注按钮 -->
        <FollowButton :user-id="String(u.user_id)" />
      </li>
    </ul>
  </div>
</template>
