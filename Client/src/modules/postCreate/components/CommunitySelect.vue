<template>
  <div>
    <label class="block text-sm font-medium text-slate-400 mb-1">选择一个社区</label>
    <select v-model="selected" @change="emitChange"
            class="block w-full sm:w-72 bg-slate-800 border border-slate-700 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-sky-500 focus:border-sky-500 sm:text-sm">
      <option v-for="circle in circles" :key="circle.circleId" :value="circle.circleId">
        {{ circle.name }}
      </option>
    </select>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { getCircles } from '../public';

const props = defineProps<{ modelValue: number | null }>();
const emit = defineEmits(['update:modelValue']);

const selected = ref<number | null>(props.modelValue);
const circles = ref<Array<{ circleId: number; name: string }>>([]);

onMounted(async () => {
  const res = await getCircles();
  circles.value = res.data;
});

watch(selected, value => {
  emit('update:modelValue', value);
});

function emitChange() {
  emit('update:modelValue', selected.value);
}
</script>
