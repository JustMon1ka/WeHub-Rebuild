<template>
  <div>
    <label class="block text-sm font-medium text-slate-400 mb-1">添加标签</label>
    <div class="border bg-slate-800 p-2 rounded flex flex-wrap items-center">
      <!-- ✅ 已完成的标签 -->
      <span v-for="tag in tags" :key="tag.name"
            class="tag-item bg-sky-600 text-white rounded-full px-2 py-1 mr-1 mb-1 relative group">
        {{ tag.name }}
        <span @click="remove(tag.name)"
              class="absolute right-0 top-0 -mr-1 -mt-1 bg-slate-700 rounded-full w-4 h-4 text-center text-xs
                     cursor-pointer opacity-0 group-hover:opacity-100 transition-opacity">
          ×
        </span>
      </span>

      <!-- ✅ 输入框显示普通 #xxx -->
      <input v-model="input"
             @keydown.enter.prevent="commitCurrent"
             @input="onInput"
             @keydown.backspace="handleBackspace"
             placeholder="#标签"
             class="bg-transparent outline-none flex-1 min-w-[80px]" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, defineExpose } from 'vue';
import { addTags } from '../public';

const props = defineProps<{ modelValue: string[] }>();
const emit = defineEmits(['update:modelValue']);

const tags = ref<{ name: string; tag_id?: number }[]>(props.modelValue.map(n => ({ name: n })));
const input = ref('#'); // ✅ 初始自动带一个#

onMounted(() => {
  if (!input.value.startsWith('#')) input.value = '#';
});

// ✅ 确认当前输入内容为标签
function commitCurrent() {
  const name = input.value.replace(/^#/, '').trim();
  if (name) {
    tags.value.push({ name });
    emit('update:modelValue', tags.value.map(t => t.name));
  }
  // ✅ 下一次输入自动带一个#
  input.value = '#';
}

// ✅ 当用户输入新的 #，提交前一个标签
function onInput(e: Event) {
  const val = (e.target as HTMLInputElement).value;
  // 检测用户是否输入了新的#，如果不是第一个字符，则表示分隔新标签
  if (val.lastIndexOf('#') > 0) {
    const parts = val.split('#').filter(Boolean);
    if (parts.length > 0) {
      const name = parts[0].trim();
      if (name) {
        tags.value.push({ name });
        emit('update:modelValue', tags.value.map(t => t.name));
      }
    }
    // ✅ 截取最后一个 # 后面的部分作为新输入，并在开头加#
    input.value = '#' + (parts[1] ?? '');
  }
}

// ✅ Backspace 删除最后一个标签
function handleBackspace(e: KeyboardEvent) {
  if (input.value === '#' && tags.value.length) {
    e.preventDefault();
    const last = tags.value.pop();
    emit('update:modelValue', tags.value.map(t => t.name));
    if (last) input.value = '#' + last.name;
  }
}

function remove(name: string) {
  tags.value = tags.value.filter(t => t.name !== name);
  emit('update:modelValue', tags.value.map(t => t.name));
}

// ✅ 暴露外部方法上传标签
async function commitTags() {
  commitCurrent();
  if (!tags.value.length) return [];

  const names = tags.value.map(t => t.name);
  const res = await addTags(names);
  tags.value = res.data.map((t: any) => ({ tag_id: t.tag_id, name: t.name }));
  emit('update:modelValue', tags.value.map(t => t.name));
  return tags.value;
}

defineExpose({ commitTags });
</script>

<style scoped>
.tag-item {
  display: inline-flex;
  align-items: center;
  padding-right: 0.5rem;
  position: relative;
}
</style>
