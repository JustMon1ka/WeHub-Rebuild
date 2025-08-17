<script setup lang="ts">
import icons from '@/modules/user/scripts/icon.ts'
import tags from '@/modules/user/scripts/tags.json' // TODO: 这里需要替换为实际的标签id
import type { Ref } from 'vue'

const counter = defineModel<number>(
  'counter', { required: true });

const selectedTags: Ref<Map<string, string>> = defineModel<Map<string, string>>(
  'selectedTags', { required: true, default: () => new Set<string>() });

const tagMap: Map<string, Map<string, string>> = new Map();

for (const [tagGroup, tagNames] of Object.entries(tags)) {
  if (!tagMap.has(tagGroup)) {
    tagMap.set(tagGroup, new Map<string, string>());
  }
  for (const [tagId, tagName] of Object.entries(tagNames)) {
    tagMap.get(tagGroup)?.set(tagId, tagName);
  }
}

function onTagClick(event: MouseEvent) {
  const target = event.target as HTMLButtonElement;
  let tagName:string = target.textContent?.trim().slice(2) || '';
  for (let ch of tagName) {
    if (ch === ' ') ch = '-';
    if (ch === '✓') ch = '';
  }

  const tagId = target.id;
  if (!tagName) return;
  if (selectedTags.value.has(tagName)) {
    selectedTags.value.delete(tagName);
    target.classList.remove('selected');
    counter.value--;
  } else {
    selectedTags.value.set(tagId, tagName);
    target.classList.add('selected');
    counter.value++;
  }
}
</script>

<template>
  <!-- 标签选择区 -->
  <div class="bg-slate-800 p-6 sm:p-8 rounded-2xl shadow-lg">
    <div class="space-y-8">

      <!-- 标签选择区 -->
      <div class="bg-slate-800 p-6 sm:p-8 rounded-2xl shadow-lg">
        <div class="space-y-8">
          <div v-for="([tagGroup, tagNames], index) of tagMap" :key="tagGroup">
            <h2 class="flex items-center text-xl font-semibold border-b-2 border-slate-700 pb-2 mb-4">
              <label v-html="icons[index]"></label>
              {{ tagGroup }}
            </h2>
            <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
              <button v-for="([id, tag], index) in tagNames" :key="id"
                      @click.prevent="onTagClick" class="tag-card">
                <span class="checkmark">✓</span>
                {{ tag }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>


<style scoped>
/* 标签卡片基础样式 */
.tag-card {
  position: relative;
  padding: 1rem;
  border-radius: 0.75rem; /* 12px */
  font-weight: 600;
  border-width: 2px;
  transition: all 0.2s ease-in-out;
  cursor: pointer;
  text-align: center;
}
/* 隐藏默认的 checkmark */
.tag-card .checkmark {
  display: none;
}
/* 选中状态下显示 checkmark */
.tag-card.selected .checkmark {
  display: flex;
}
/* 选中标记的样式 */
.checkmark {
  position: absolute;
  top: -10px;
  right: -10px;
  width: 24px;
  height: 24px;
  background-color: #0ea5e9; /* sky-500 */
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: bold;
  border: 2px solid #1e293b; /* slate-800 */
}
.tag-card:not(.selected):hover {
  border-color: #64748b; /* slate-500 */
  background-color: rgba(71, 85, 105, 0.7);
}
</style>
