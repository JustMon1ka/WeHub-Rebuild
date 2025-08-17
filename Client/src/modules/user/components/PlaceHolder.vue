<script setup lang="ts">
import { computed } from 'vue'

const { width, height, text } = defineProps<{
  width: string;
  height: string;
  text: string;
}>();

function simpleHash(str: string): number {
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    hash += str.charCodeAt(i) * (i + 1); // 使用字符的ASCII码和位置来计算哈希值
  }
  return hash; // 返回正数
}

function generateAvatarColors(text: string): { bgColor: string; textColor: string } {
  // 预定义的色板（从参考HTML中提取的颜色组合）
  const colorPalette = [
    { bgColor: '7dd3fc', textColor: '0f172a' }, // 蓝-深蓝
    { bgColor: 'ec4899', textColor: '831843' }, // 粉-深粉
    { bgColor: '8b5cf6', textColor: '4c1d95' }, // 紫-深紫
    { bgColor: '34d399', textColor: '064e3b' }, // 绿-深绿
    { bgColor: 'facc15', textColor: '78350f' }, // 黄-深橙
    { bgColor: 'a78bfa', textColor: '1e1b4b' }, // 浅紫-深紫
    { bgColor: 'f472b6', textColor: '831843' }, // 浅粉-深粉
    { bgColor: '60a5fa', textColor: '1e40af' }, // 浅蓝-深蓝
  ];

  // 如果没有文本，返回默认颜色
  if (!text || text.length === 0) {
    return { bgColor: '7dd3fc', textColor: '0f172a' };
  }

  const charCode = simpleHash(text);
  const colorIndex = charCode % colorPalette.length;
  return colorPalette[colorIndex];
}

const color = computed(() => generateAvatarColors(text) );
</script>

<template>
<svg xmlns="http://www.w3.org/2000/svg" :width="width" :height="height" :viewBox="'0 0 ' + width + ' ' + height">
    <rect width="100%" height="100%" :fill="'#' + color.bgColor"/>
    <text x="50%" y="50%" :fill="'#' + color.textColor" font-family="Arial" :font-size="Math.min(Number(width), Number(height)) * 0.6"
          text-anchor="middle" dominant-baseline="central" class="font-[550]">{{ text[0] }}</text>
  </svg>
</template>

<style scoped>
text{
  cursor: default;
  user-select: none;
  caret-color: transparent;
}
</style>
