<!--
Markdown渲染组件

用法：
<script>中：
import MarkdownViewer from ...
<template>中：
<MarkdownViewer ... />

参数：
:model-value  # 必要参数；string类型；渲染需要的Markdown文本
content-theme # 默认参数；值仅可为'light'或'dark'；明暗主题；缺省为'dark'
code-theme    # 默认参数；string类型；代码渲染主题名称；缺省为'github'
:after-render # 可选参数；void函数类型；Markdown渲染后调用的钩子函数
-->
<template>
  <!-- 消除外部CSS影响 -->
  <div class="md-sandbox-frame md-sandbox">
    <!-- 内层渲染根：仅挂给 Vditor，保留 vditor-reset 以复用主题 -->
    <article ref="rootEl" class="vditor-reset"></article>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import Vditor from 'vditor';
import 'vditor/dist/index.css';

const props = withDefaults(
  defineProps<{
    modelValue: string,
    contentTheme?: 'light' | 'dark',
    codeTheme?: string,
    afterRender?: () => void
  }>(), {
    contentTheme: 'dark',
    codeTheme: 'github'
  }
);

const rootEl = ref<HTMLDivElement | null>(null);

function render(md: string) {
  const el = rootEl.value;
  if (!el) return;

  Vditor.preview(el, md, {
    mode: props.contentTheme,
    // 内容主题（light/dark）
    theme: { current: props.contentTheme },
    // 代码高亮
    hljs: { style: props.codeTheme },
    // 图片懒加载
    lazyLoadImage: 'loading="lazy"',
    // 渲染后钩子
    after: () => {
      props.afterRender?.();
    }
  })
}

onMounted(() => render(props.modelValue));
watch(() => props.modelValue, (v) => render(v));
</script>

<style lang="postcss">
/* —— 让内部内容回退到浏览器/库自己的样式，避免被全局样式污染 —— */
.md-sandbox{
  /* 普遍可用：把几乎所有属性回退到 UA/上级层；Tailwind 全局字体、line-height 等不会再渗透进来 */
  all: revert;
  /* 若浏览器支持分层回退，优先使用（能更好抵御 @layer utilities 的覆盖） */
}
@supports (all: revert-layer){
  .md-sandbox{ all: revert-layer; }
}
/* 占满中间列可用宽度 */
.md-sandbox-frame { width: 100%; min-width: 0; }
.md-sandbox { width: 100%; max-width: none; }
/* 回退后补一些基础可用性：盒模型统一、媒体自适应 */
.md-sandbox{ box-sizing:border-box; }
.md-sandbox *{ box-sizing:inherit; }
.md-sandbox img,
.md-sandbox video,
.md-sandbox table{ max-width:100%; height:auto; }

/* 可调的媒体最大像素宽度（绝对宽度） */
.md-sandbox-frame { --md-media-max: 800px; } /* 你可改成 720px/900px 等 */

/* video / iframe：小屏时占满容器；容器更宽时限制为最大像素宽度并居中 */
.md-sandbox :where(video, iframe) {
  width: 100%;
  max-width: var(--md-media-max);
  display: block;
  margin-left: auto;
  margin-right: auto;
}

/* 让 iframe 维持常见 16:9 比例（也可改 4:3/21:9） */
.md-sandbox :where(iframe) {
  aspect-ratio: 16 / 9;
  /* 如需最小高度，可解开下一行：
  min-height: 320px;
  */
}

/* 如果之前给过 video 的 max-height，保留以防过高；也可按需调整或移除 */
.md-sandbox :where(video) {
  max-height: 70vh;
}

/* 若 vditor 把媒体包在 figure 里，也一并居中处理 */
.md-sandbox :where(figure > video, figure > iframe) {
  margin-left: auto;
  margin-right: auto;
}
</style>
