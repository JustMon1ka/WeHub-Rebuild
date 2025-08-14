<script setup lang="ts">
import UserInfoForm from '@/modules/user/components/UserPage/UserInfoForm.vue'
</script>

<template>
  <div class="sticky top-0 z-10 bg-slate-900/80 backdrop-blur-md flex items-center p-4 border-b border-slate-800">
    <button @click="$emit('editOver')" class="p-2 rounded-full hover:bg-slate-800 mr-4">
      <img src="@/assets/close.svg" alt="关闭编辑" class="w-6 h-6">
    </button>
    <h1 class="text-xl font-bold flex-grow">编辑个人资料</h1>
    <button @click="$emit('editSave')" class="bg-white hover:bg-slate-200 text-slate-900 font-bold
                  py-2 px-5 rounded-full transition-colors duration-200">
      保存
    </button>
  </div>
  <UserInfoForm/>
</template>
