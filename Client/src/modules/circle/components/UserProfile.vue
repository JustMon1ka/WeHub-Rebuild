<template>
  <div class="bg-white rounded-lg shadow-sm p-6">
    <div class="flex items-center space-x-3">
      <div class="w-12 h-12 bg-gray-300 rounded-full flex items-center justify-center">
        <span class="text-gray-600 font-medium">{{ user.displayName[0] }}</span>
      </div>
      <div>
        <h3 class="font-medium text-gray-900">{{ user.displayName }}</h3>
        <p class="text-gray-500 text-sm">@{{ user.username }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { User } from '../types.ts'

defineProps<{
  user: User
}>()
</script>
