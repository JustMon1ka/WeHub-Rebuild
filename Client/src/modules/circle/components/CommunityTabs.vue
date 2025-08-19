<template>
  <div class="border-b mb-6">
    <nav class="flex space-x-8">
      <button
        v-for="tab in tabs"
        :key="tab.key"
        @click="$emit('update:activeTab', tab.key)"
        :class="[
          'py-2 px-1 border-b-2 font-medium text-sm',
          activeTab === tab.key
            ? 'border-blue-500 text-blue-600'
            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
        ]"
      >
        {{ tab.label }}
      </button>
    </nav>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  activeTab: string
  tabs: Array<{ key: string; label: string }>
}>()

defineEmits<{
  'update:activeTab': [value: string]
}>()
</script>
