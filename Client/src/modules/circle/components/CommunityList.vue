<template>
  <div class="space-y-4 mb-8">
    <div
      v-for="community in communities"
      :key="community.id"
      class="border border-gray-200 rounded-lg p-6 hover:shadow-md transition-shadow"
    >
      <div class="flex items-start justify-between">
        <div class="flex-1">
          <h3 class="text-lg font-semibold text-gray-900 mb-2">
            {{ community.name }}
          </h3>
          <p class="text-gray-600 mb-3">{{ community.description }}</p>
          <p class="text-sm text-gray-500">{{ formatMemberCount(community.memberCount) }} 成员</p>
        </div>

        <button
          @click="handleToggleJoin(community)"
          :class="[
            'ml-4 px-6 py-2 rounded-lg font-medium transition-colors',
            community.isJoined
              ? 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              : 'bg-blue-600 text-white hover:bg-blue-700',
          ]"
        >
          {{ community.isJoined ? '已加入' : '加入' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Community } from '../types.ts'

defineProps<{
  communities: Community[]
}>()

const emit = defineEmits<{
  join: [communityId: string]
  leave: [communityId: string]
}>()

const formatMemberCount = (count: number): string => {
  if (count >= 10000) {
    return (count / 10000).toFixed(1) + '万'
  }
  return count.toLocaleString()
}

const handleToggleJoin = (community: Community) => {
  if (community.isJoined) {
    emit('leave', community.id)
  } else {
    emit('join', community.id)
  }
}
</script>
