<template>
  <div class="p-6 space-y-4">
    <h1 class="text-2xl font-bold">测试点赞 & 收藏组件</h1>

    <LikeButton
      :post-id="123"
      :is-liked="false"
      :like-count="5"
    />

    <FavoriteButton
      :post-id="123"
      :is-favorited="false"
    />
  </div>
</template>

<script setup lang="ts">
// 引入你在 post/public.ts 里导出的组件
import { LikeButton, FavoriteButton } from "../public";
</script>
