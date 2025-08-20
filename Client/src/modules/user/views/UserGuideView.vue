<script setup lang="ts">
import TagGuide from '@/modules/user/components/Tag/TagGuide.vue'
import { type Ref, ref } from 'vue'
import { User } from '@/modules/auth/public.ts'
import router from '@/router.ts'

const counter = ref(0);
const selectedTags : Ref<Map<string, string>> = ref(new Map<string, string>());
async function onSave() {
  if (User.getInstance()?.userInfo?.userTags)
    User.getInstance().userInfo.userTags = selectedTags.value;
  User.getInstance()?.userInfo?.updateTags();
  await router.push('/');
}
</script>


<template>
  <div class="w-full h-full flex flex-col items-center justify-center-safe p-4 sm:p-6">
    <!-- 头部 -->
    <div class="text-center mb-8">
      <img src="@/assets/logo.svg" alt="Logo" class="w-16 h-16 mx-auto mb-4">
      <h1 class="text-3xl font-bold mt-4">选择你感兴趣的标签</h1>
      <p class="text-slate-400 mt-2">
        告诉我们你的偏好，以便为你推荐更精彩的内容。 (已选择
        <span id="selected-count" class="text-sky-400 font-bold">{{ counter }}</span> 个)
      </p>
    </div>

    <TagGuide v-model:counter="counter" v-model:selected-tags="selectedTags"/>

    <!-- 底部操作栏 -->
    <div class="mt-8 flex flex-col sm:flex-row items-center justify-center gap-4">
      <router-link to="/" class="w-full sm:w-auto flex justify-center py-3 px-10 border border-slate-600 rounded-full shadow-sm text-sm font-bold hover:bg-slate-800 focus:outline-none transition-colors duration-200">
        跳过
      </router-link>
      <button @click="onSave" class="w-full sm:w-auto flex justify-center py-3 px-10 border border-transparent rounded-full shadow-sm text-sm font-bold text-white bg-sky-500 hover:bg-sky-600 focus:outline-none transition-colors duration-200">
        完成
      </button>
    </div>
  </div>
</template>
