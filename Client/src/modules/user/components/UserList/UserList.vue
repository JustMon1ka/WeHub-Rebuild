<script setup lang="ts">
import { ref } from 'vue'
import PlaceHolder from '@/modules/user/components/PlaceHolder.vue'

const { bio, generator }= defineProps<{
  bio: boolean;
  generator: Generator;
}>();

const items = ref([
  { nickName: '旅行的青蛙', userName: '@travel_frog', bio: "一二三四五六七八九十一二三四五六七八九十"},
  { nickName: 'user_gamer', userName: '@user_gamer', bio: "热爱游戏的玩家"},
  { nickName: 'tech_enthusiast', userName: '@tech_enthusiast', bio: "科技爱好者"},
]);
</script>

<template>
  <!-- 用户列表 -->
  <div class="divide-y divide-slate-800">
    <div v-for="({ nickName, userName, bio }, index) in items" :key="index" class="p-4 flex items-start space-x-4 hover:bg-slate-800/50 transition-colors duration-200">
      <PlaceHolder width="100" :text="nickName" height="100" class="w-12 h-12 rounded-full"/>
      <div class="flex-1">
        <div class="flex items-center justify-between">
          <div>
            <label class="font-bold">{{ nickName }}</label>
          </div>
          <button class="bg-slate-200 hover:bg-white text-slate-900 font-semibold text-sm py-2 px-5 rounded-full transition-colors duration-200">关注</button>
        </div>
        <p v-if="bio" class="mt-2 text-slate-300 truncate w-70">{{ bio }}</p>
      </div>
    </div>
  </div>
</template>
