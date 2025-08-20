<template>
  <input
    class="search-input"
    :value="modelValue"
    @input="handleInputChange"
    type="text"
    :placeholder="placeholder"
  />
</template>

<script setup lang="ts">
interface Props {
  modelValue: string;
  placeholder?: string;
}

defineProps<Props>();
const emit = defineEmits<{
  "update:modelValue": [value: string];
}>();

const handleInputChange = (event: Event) => {
  const target = event.target as HTMLInputElement;
  emit("update:modelValue", target.value);
};
</script>

<style scoped>
.search-input {
  margin: 16px 32px;
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
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
</style>
