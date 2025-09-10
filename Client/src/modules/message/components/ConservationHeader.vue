<template>
  <div class="avatar">
    <img
      :src="
        props.conversation.contactUser?.avatar ||
        'https://placehold.co/100x100/facc15/78350f?text=U'
      "
      :alt="props.conversation.contactUser?.nickname || `用户${props.conversation.OtherUserId}`"
    />
    <div class="nickname">
      {{ props.conversation.contactUser?.nickname || `用户${props.conversation.OtherUserId}` }}
    </div>
  </div>
</template>

<script setup lang="ts">
import type { conversation } from '../types'

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