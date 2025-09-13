<template>
  <div class="avatar">
    <img v-if="!!props.conversation.contactUser?.avatar" class="w-10 h-10 rounded-full"
      :src="props.conversation.contactUser?.avatar" alt="user" />
    <PlaceHolder v-else width="100" height="100" :text="props.conversation.contactUser?.nickname || props.conversation.OtherUserId"
                  class="w-10 h-10 rounded-full" />
    <div class="nickname">
      {{ props.conversation.contactUser?.nickname || props.conversation.OtherUserId }}
    </div>
  </div>
</template>

<script setup lang="ts">
import type { conversation } from '../types'
import PlaceHolder from '@/modules/user/components/PlaceHolder.vue'

const props = defineProps<{
  conversation: conversation
}>()
</script>

<style scoped>
.avatar {
  display: flex;
  align-items: center;
  padding: 1rem;
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
