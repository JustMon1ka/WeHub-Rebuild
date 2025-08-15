<script setup lang="ts">
const { width, height, text } = defineProps<{
  width: number;
  height: number;
  text: string;
}>();

const { bgColor, textColor } = generateAvatarColors(text);

function generateAvatarColors(text) {
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

  // 基于文本字符的哈希值选择颜色
  const charCode = text.charCodeAt(0) + (text.length > 1 ? text.charCodeAt(1) : 0);
  const colorIndex = charCode % colorPalette.length;
  return colorPalette[colorIndex];
}
</script>

<template>
<svg xmlns="http://www.w3.org/2000/svg" :width="width" :height="height" :viewBox="'0 0 ' + width + ' ' + height">
    <rect width="100%" height="100%" :fill="'#' + bgColor"/>
    <text x="50%" y="50%" :fill="'#' + textColor" font-family="Arial" :font-size="Math.min(width, height) * 0.6"
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
