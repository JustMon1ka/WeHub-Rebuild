<template>
  <div class="bg-slate-800 border border-slate-700 rounded-lg shadow-sm p-6">
    <div class="flex items-center space-x-3">
      <div class="w-12 h-12 bg-slate-600 rounded-full flex items-center justify-center">
        <span class="text-slate-200 font-medium">{{ user.displayName[0] }}</span>
      </div>
      <div>
        <h3 class="font-medium text-slate-100">{{ user.displayName }}</h3>
        <p class="text-slate-400 text-sm">@{{ user.username }}</p>
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
