<script setup lang="ts">
import { type Ref, ref } from 'vue'
import UserCardList from '@/modules/user/components/UserList/UserCardList.vue'

const { generator }= defineProps<{
  bio: boolean;
  generator: Generator;
  followBtn?: boolean;
}>();

const users : Ref<Array<string>> = ref(['Me', 'Me', 'Me', 'Me', 'Me', 'Me', 'Me', 'Me', 'Me', 'Me']);
</script>

<template>
  <!-- 用户列表 -->
  <div class="relative divide-y divide-slate-800">
    <div v-for="(user, index) in users" :key="index"
         class="p-4 flex items-start space-x-4 hover:bg-slate-800/50 transition-colors duration-200">
      <UserCardList :user-id="user" :follow-btn="followBtn"/>
    </div>
  </div>
</template>


