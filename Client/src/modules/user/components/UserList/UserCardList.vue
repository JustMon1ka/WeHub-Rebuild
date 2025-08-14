<script setup lang="ts">
const { bio }= defineProps<{
  bio?: boolean;
}>();
</script>

<template>
  <!-- 用户卡片 1 -->
  <div class="p-4 flex items-start space-x-4 hover:bg-slate-800/50 transition-colors duration-200">
    <img class="w-12 h-12 rounded-full" src="https://placehold.co/100x100/facc15/78350f?text=F" alt="User Avatar">
    <div class="flex-1">
      <div class="flex items-center justify-between">
        <div>
          <a href="#" class="font-bold hover:underline">旅行的青蛙</a>
          <p class="text-sm text-slate-500">@travel_frog</p>
        </div>
        <button class="bg-slate-200 hover:bg-white text-slate-900 font-semibold text-sm py-2 px-5 rounded-full transition-colors duration-200">关注</button>
      </div>
      <p v-if="bio" class="mt-2 text-slate-300">一个喜欢到处旅行和拍照的普通人。</p>
    </div>
  </div>
</template>

<style scoped>

</style>
