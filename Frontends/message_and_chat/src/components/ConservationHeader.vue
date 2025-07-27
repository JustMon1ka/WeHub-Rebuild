<template>
  <div class="avatar">
    <img :src="props.conversation.contactUser.avatar"
         :alt="props.conversation.contactUser.nickname"/>
    <div class="nickname">{{ props.conversation.contactUser.nickname }}</div>
  </div>

</template>

<script setup lang="ts">
import type { conversation } from '../types/message'

const props = defineProps<{
     conversation: conversation
}>()
</script>

<style scoped>
.avatar {
  display: flex;
  align-items: center;
  padding-left: 8px;
  padding-right: 8px;
}

.avatar img {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  object-fit: cover;
  display: block;
}

.nickname {
  font-weight: bold;
  font-size: 18px;
  margin-left: 12px;
  text-align: left;
}
</style>