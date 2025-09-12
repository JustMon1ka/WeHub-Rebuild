<template>
  <input
    class="search-input bg-slate-800 focus:bg-slate-700 rounded-xl"
    :value="modelValue"
    @input="handleInputChange"
    type="text"
    :placeholder="placeholder"
  />
</template>

<script setup lang="ts">
interface Props {
  modelValue: string
  placeholder?: string
}

defineProps<Props>()
const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const handleInputChange = (event: Event) => {
  const target = event.target as HTMLInputElement
  emit('update:modelValue', target.value)
}
</script>

<style scoped>
.search-input {
  width: 100%;
  max-width: 100%;
  margin: 16px 32px;
  padding: 8px 12px;
  border: 1px solid #6b7280;
  font-size: 14px;
  box-sizing: border-box;
}

.search-input:focus {
  outline: none;
  border-color: #4a9eff;
  box-shadow: 0 0 0 2px rgba(74, 158, 255, 0.2);
}

.search-input::placeholder {
  color: #999;
}

/* 响应式设计：在小屏幕上调整边距 */
@media (max-width: 768px) {
  .search-input {
    margin: 16px 16px;
  }
}

@media (max-width: 480px) {
  .search-input {
    margin: 16px 8px;
    padding: 6px 8px;
    font-size: 12px;
  }
}
</style>
