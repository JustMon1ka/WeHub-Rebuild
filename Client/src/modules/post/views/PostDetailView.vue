<!-- 该文件处于测试阶段 -->
<script setup lang="ts">
import { ref } from 'vue';
import MarkdownViewer from '../components/MarkdownViewer.vue';

// 演示 Markdown（注意：不要用 $$...$$；行内公式用 \( ... \)，块级用 \[ ... \]）
const mdDemo = ref<string>(
`
# Demo

- 代码高亮

\`\`\`ts
export const hello = (name: string) => \`Hi, \${name}\`
\`\`\`

| 字符 | 名称 | ASCII码值（十进制） |
|---|---|---|
| '\\\\a' | 响铃(BEL) | 007 |
| '\\\\b' | 退格(BS) | 008 |
| '\\\\f' | 换页(FF) | 012 |
| '\\\\n' | 回车(CR) | 010 |
| '\\\\t' | 水平制表(HT) | 009 |
| '\\\\v' | 垂直制表(VT) | 011 |
| '\\\\\\\\' | 反斜杠 | 092 |
| '\\\\?' | 问号字符 | 063 |
| '\\\\'' | 单引号字符 | 039 |
| '\\\\"' | 双引号字符 | 034 |
| '\\\\0' | 空字符(NULL) | 000 |

这是数字112121和 $11121$  

> hdsjhafh

$$
e^{i\\pi}=-1
$$

$$
\\underline{\\textbf{优先级（由低到高）}}\\\\
\\begin{array}{ll}
\\uparrow \\textbf{long double}\\\\
\\uparrow \\textbf{double}\\\\
\\uparrow \\textbf{float}\\\\
\\uparrow \\textbf{unsigned long long}\\\\
\\uparrow \\textbf{long long}\\\\
\\uparrow \\textbf{unsigned long}\\\\
\\uparrow \\textbf{long}\\\\
\\uparrow \\textbf{unsigned}\\\\
\\uparrow \\textbf{int}\\ <-\\ \\textbf{char},\\textbf{unsigned char},\\textbf{short},\\textbf{unsigned short}
\\end{array}
$$

## 图片
![示意图](https://placehold.co/800x400/png)

## 视频直链（自动 \`<video>\`）  
https://www.w3schools.com/html/mov_bbb.mp4

## Bilibili（自动 \`<iframe>\`）  
https://www.bilibili.com/video/BV1xx411c7mD
`
);
</script>

<template>
  <div class="w-full max-w-none min-w-0 border border-slate-800 rounded-2xl bg-slate-900/30 p-4 md:p-6 max-h-[100dvh] overflow-y-auto">
    <MarkdownViewer :model-value="mdDemo" />
  </div>
</template>

<style scoped>

</style>
