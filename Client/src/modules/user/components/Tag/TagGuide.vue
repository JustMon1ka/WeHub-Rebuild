<script setup lang="ts">
import icons from '@/modules/user/scripts/icon.ts'
import type { Ref } from 'vue'

const counter = defineModel<number>(
  'counter', { required: true });

const selectedTags: Ref<Set<string>> = defineModel<Set<string>>(
  'selectedTags', { required: true, default: () => new Set<string>() });

const tags : Object = new Map<string, Array<string>>([
  ['游戏', ['角色扮演 (RPG)', '动作游戏 (ACT)', '射击游戏 (FPS)', '开放世界', '独立游戏', '任天堂', 'PlayStation', 'PC游戏']],
  ['科技数码', ['智能手机', '笔记本电脑', '编程开发', '人工智能', '数码产品', '虚拟现实 (VR)']],
  ['影音娱乐', ['电影', '电视剧', '动漫', '音乐', '综艺']],
  ['生活趣味', ['美食', '旅行', '摄影', '宠物', '时尚穿搭', '汽车']],
  ['运动健康', ['健身', '篮球', '足球', '户外运动']]
]);

function onTagClick(event: MouseEvent) {
  const target = event.target as HTMLButtonElement;
  let tagName:string = target.textContent?.trim().slice(2) || '';
  for (let ch of tagName) {
    if (ch === ' ') ch = '-';
    if (ch === '✓') ch = '';
  }

  if (!tagName) return;
  if (selectedTags.value.has(tagName)) {
    selectedTags.value.delete(tagName);
    target.classList.remove('selected');
    counter.value--;
  } else {
    selectedTags.value.add(tagName);
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
          <div v-for="([tagGroup, tagNames], index) of tags" :key="tagGroup">
            <h2 class="flex items-center text-xl font-semibold border-b-2 border-slate-700 pb-2 mb-4">
              <label v-html="icons[index]"></label>
              {{ tagGroup }}
            </h2>
            <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
              <button v-for="(tag, index) in tagNames" :key="tag"
                      @click="onTagClick" class="tag-card">
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
