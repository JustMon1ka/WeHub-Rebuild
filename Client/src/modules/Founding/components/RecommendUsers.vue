<script setup lang="ts">
import type { RecommendUser } from "../types"
import FollowButton from "@/modules/user/components/UserList/FollowButton.vue"
import PlaceHolder from "@/modules/user/components/PlaceHolder.vue"

defineProps<{ users: RecommendUser[] }>()

// ✅ 定义一个 updated 事件，可以往上透传
const emit = defineEmits<{
  (e: "updated"): void
}>()
</script>

<template>
  <div class="bg-slate-800 rounded-2xl p-4 w-72">
    <h2 class="text-lg font-bold mb-4">推荐关注</h2>
    <ul class="space-y-4">
      <li v-for="u in users" :key="u.user_id" class="flex items-center">
        <!-- 用户头像 -->
        <router-link
          :to="`/user_page/${u.user_id}`"
          class="w-10 h-10 rounded-full flex items-center justify-center overflow-hidden"
        >
          <!-- 有头像 URL -->
          <img
            v-if="u.avatar_url"
            :src="u.avatar_url"
            alt="avatar"
            class="w-full h-full object-cover"
          />
          <!-- 无头像，用 PlaceHolder -->
          <PlaceHolder
            v-else
            width="40"
            height="40"
            :text="u.nickname || u.username"
          />
        </router-link>

        <!-- 用户信息 -->
        <div class="ml-3 flex-1 overflow-hidden">
          <!-- 显示昵称（若为空则回退到用户名） -->
          <router-link :to="`/user_page/${u.user_id}`">
            <p class="font-semibold text-sm truncate hover:underline">
              {{ u.nickname || u.username }}
            </p>
          </router-link>
          <!-- 副标题显示用户名 -->
          <p class="text-slate-400 text-xs truncate">@{{ u.username }}</p>
        </div>

        <!-- 动态关注按钮 -->
        <FollowButton 
          :user-id="String(u.user_id)" 
          @updated="$emit('updated')" 
        />
      </li>
    </ul>
  </div>
</template>
