<template>
  <div>
    <!-- Toolbar: 保持原有布局 -->
    <div
      class="toolbar flex space-x-1 p-2 bg-slate-800 rounded-t-lg border-x border-t border-slate-700"
    >
      <div class="flex items-center space-x-2 px-3 py-2 border-b border-slate-700 bg-slate-800">
        <!-- 上传资源 -->
        <button
          type="button"
          class="p-2 rounded hover:bg-slate-700 text-slate-200"
          title="上传资源"
          @click="triggerUpload"
        >
          <FolderIcon class="w-4 h-4" />
        </button>

        <!-- 粗体 -->
        <button
          type="button"
          :class="['p-2 rounded text-slate-200', isBoldActive ? 'bg-sky-600' : 'hover:bg-slate-700']"
          title="加粗"
          @click="toggleBold"
        >
          <BoldIcon class="w-4 h-4" />
        </button>

        <!-- 斜体 -->
        <button
          type="button"
          :class="['p-2 rounded text-slate-200', isItalicActive ? 'bg-sky-600' : 'hover:bg-slate-700']"
          title="斜体"
          @click="toggleItalic"
        >
          <ItalicIcon class="w-4 h-4" />
        </button>

        <!-- 链接 -->
        <button
          type="button"
          class="p-2 rounded hover:bg-slate-700 text-slate-200"
          title="插入链接"
          @click="insertLink"
        >
          <LinkIcon class="w-4 h-4" />
        </button>

        <!-- 代码块 -->
        <button
          type="button"
          class="p-2 rounded hover:bg-slate-700 text-slate-200"
          title="插入代码块"
          @click="insertCodeBlock"
        >
          <CodeIcon class="w-4 h-4" />
        </button>

        <!-- 引用 -->
        <button
          type="button"
          class="p-2 rounded hover:bg-slate-700 text-slate-200"
          title="引用"
          @click="insertQuote"
        >
          <QuoteIcon class="w-4 h-4" />
        </button>
      </div>
      <input
        ref="fileInput"
        type="file"
        class="hidden"
        accept="image/*,video/*,audio/*"
        @change="handleFileChange"
      />
    </div>

    <!-- CodeMirror 编辑区 -->
    <div ref="cmContainer" class="bg-slate-800 p-4 rounded-b-lg min-h-[300px] text-white"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { EditorState, EditorSelection, type Range } from '@codemirror/state';
import type { Node } from 'unist';
import {
  EditorView,
  keymap,
  Decoration,
  type DecorationSet,
  WidgetType,
  ViewPlugin,
  type ViewUpdate,
} from '@codemirror/view';
import { defaultKeymap, insertNewlineAndIndent } from '@codemirror/commands';
import { markdown, markdownLanguage } from '@codemirror/lang-markdown';
import { unified } from 'unified';
import remarkParse from 'remark-parse';
import remarkGfm from 'remark-gfm';
import { visitParents , SKIP } from 'unist-util-visit-parents';
import type { Link, Heading, Strong, Emphasis, InlineCode, Delete } from 'mdast';
import { syntaxHighlighting, HighlightStyle } from '@codemirror/language';
import { tags as t } from '@lezer/highlight';
import { LanguageDescription } from '@codemirror/language';
import { createVNode, render, h, type VNode } from 'vue';
import axios from 'axios';

import {
  FolderIcon,
  CodeBracketIcon as CodeIcon,
  LinkIcon,
  BoldIcon,
  ItalicIcon,
  ChatBubbleLeftIcon as QuoteIcon,
} from '@heroicons/vue/24/outline';

// 导入新的预览组件
import AudioPreview from './AudioPreview.vue';
import ImagePreview from './ImagePreview.vue';
import VideoPreview from './VideoPreview.vue';

defineExpose({
  uploadMedia
});

const emit = defineEmits<{ (e: 'update:modelValue', value: string): void }>();
const props = defineProps<{ modelValue: string }>();

const mdText = ref(props.modelValue || '');
watch(() => props.modelValue, v => {
  if (view && v !== view.state.doc.toString()) {
    view.dispatch({
      changes: { from: 0, to: view.state.doc.length, insert: v || '' }
    });
  }
});

// 用于解析自定义媒体标签的正则表达式
const mediaRegex = /<type:\s*(audio|video|image),\s*fileId:\s*['"]?([\w\d-]+)['"]?(\s*,\s*length:\s*(\d+))?\s*>/;

// 辅助函数：将 AST 节点转换为 HTML，现在包括了对自定义媒体标签的特别处理
function nodeToHtml(node: any): string {
  if (node.type === 'text') {
    // 检查文本内容是否匹配媒体标签
    if (mediaRegex.test(node.value)) {
      const match = node.value.match(mediaRegex);
      const type = match[1];
      const fileId = match[2];

      // 返回一个占位符，之后会通过 WidgetType 替换
      return `<div data-media-type="${type}" data-file-id="${fileId}"></div>`;
    }
    return node.value.replace(/</g, '&lt;').replace(/>/g, '&gt;');
  }
  const childrenHtml = node.children ? node.children.map(nodeToHtml).join('') : (node.value || '');
  switch (node.type) {
    case 'strong': return `<strong>${childrenHtml}</strong>`;
    case 'emphasis': return `<em>${childrenHtml}</em>`;
    case 'delete': return `<del>${childrenHtml}</del>`;
    case 'inlineCode': return `<code>${childrenHtml.replace(/</g, '&lt;').replace(/>/g, '&gt;')}</code>`;
    case 'link': return `<a href="${node.url}" class="text-blue-400" target="_blank" rel="noopener noreferrer">${childrenHtml}</a>`;
    case 'heading': return `<span class="heading heading-${node.depth} prose-invert">${childrenHtml}</span>`;
    case 'paragraph': return childrenHtml;
    case 'root': return node.children.map(nodeToHtml).join('');

    default: return childrenHtml;
  }
}

const fileInput = ref<HTMLInputElement | null>(null);
const processor = unified().use(remarkParse).use(remarkGfm);

// 一个用于渲染 Vue 组件的 WidgetType
class VueWidget extends WidgetType {
  constructor(private vnode: VNode) {
    super();
  }

  toDOM(view: EditorView): HTMLElement {
    const wrap = document.createElement('span');
    wrap.className = 'cm-vue-widget';
    // 使用 Vue 的 render 函数将 VNode 渲染到 DOM 元素
    render(this.vnode, wrap);
    return wrap;
  }
  ignoreEvent() { return true; } // 忽略事件，让 CodeMirror 不处理内部点击等事件
}

const hybridPlugin = ViewPlugin.fromClass(
  class {
    decorations: DecorationSet;

    constructor(public view: EditorView) {
      this.decorations = this.buildDecos(view);
    }

    update(update: ViewUpdate) {
      if (update.docChanged || update.selectionSet || update.viewportChanged) {
        this.decorations = this.buildDecos(update.view);
      }
    }

    buildDecos(view: EditorView): DecorationSet {
      const decorations: Range<Decoration>[] = [];
      const tree = processor.parse(view.state.doc.toString());
      const { from, to } = view.state.selection.main;

      // 2. 使用 unist-util-visit-parents
      // 它会提供第二个参数 `ancestors`，即当前节点的父节点数组
      visitParents(
        tree,
        (node: Node, ancestors: Node[]) => { // <-- 注意这里的第二个参数 ancestors
          const nodeStart = node.position?.start?.offset;
          const nodeEnd = node.position?.end?.offset;
          if (nodeStart === undefined || nodeEnd === undefined) return;

          // 3. 判断是否在代码块内部的方式改变了
          // 我们通过检查祖先节点来判断，不再需要 inCodeBlock 标志
          const isInsideCodeBlock = ancestors.some(p => p.type === 'code');

          // 如果光标或选区在节点内部，不进行渲染，保持可编辑
          if (from >= nodeStart && to <= nodeEnd) {
            return;
          }

          // --- 修复代码块的渲染逻辑 ---
          // 这部分逻辑基本不变，但我们不再需要设置 inCodeBlock = true
          if (node.type === 'code') {
            const startLine = view.state.doc.lineAt(nodeStart);
            const endLine = view.state.doc.lineAt(nodeEnd);

            // 隐藏开始和结束围栏... (这部分代码和你原来的一样)
            decorations.push(
              Decoration.mark({ class: 'cm-codeblock-fence-start' }).range(startLine.from, startLine.to)
            );
            if (startLine.number !== endLine.number) {
              decorations.push(
                Decoration.mark({ class: 'cm-codeblock-fence-end' }).range(endLine.from, endLine.to)
              );
            }
            // 添加行级装饰器... (这部分代码和你原来的一样)
            for (let i = startLine.number; i <= endLine.number; i++) {
              const line = view.state.doc.line(i);
              let classes = 'cm-codeblock-line';
              if (i > startLine.number && i < endLine.number) classes += ' cm-codeblock-content-line';
              if (i === startLine.number) classes += ' cm-codeblock-first-line';
              if (i === endLine.number) classes += ' cm-codeblock-last-line';
              decorations.push(Decoration.line({ class: classes }).range(line.from));
            }
            // 使用 SKIP 阻止访问代码块的内部文本子节点
            return SKIP;
          }

          // --- 处理自定义媒体标签 ---
          const textContent = view.state.doc.sliceString(nodeStart, nodeEnd);
          const mediaMatch = textContent.match(mediaRegex);
          // 4. 使用新的 isInsideCodeBlock 进行判断
          if (mediaMatch && !isInsideCodeBlock) {
            const type = mediaMatch[1];
            const fileId = mediaMatch[2];
            let component;
            let props = { fileId };

            switch (type) {
              case 'audio': component = AudioPreview; break;
              case 'video': component = VideoPreview; break;
              case 'image': component = ImagePreview; break;
            }

            if (component) {
              const vnode = createVNode(component, props);
              decorations.push(
                Decoration.replace({
                  widget: new VueWidget(vnode),
                }).range(nodeStart, nodeEnd)
              );
            }
            return SKIP;
          }

          // --- 处理其他行内元素（保持不变） ---
          // 如果当前节点在代码块里，我们不应该应用其他装饰（如加粗、斜体等）
          if (isInsideCodeBlock) {
            return;
          }

          const isInline = ['strong', 'emphasis', 'inlineCode', 'link', 'heading'].includes(node.type);
          if (isInline) {
            const html = nodeToHtml(node);
            if (html) {
              decorations.push(
                Decoration.replace({
                  widget: new (class extends WidgetType {
                    toDOM() {
                      const wrap = document.createElement('span');
                      wrap.innerHTML = html;
                      return wrap;
                    }
                  })(),
                }).range(nodeStart, nodeEnd)
              );
              return SKIP;
            }
          }
        }
      );
      return Decoration.set(decorations, true);
    }
  },
  { decorations: v => v.decorations }
);


let view: EditorView;
const cmContainer = ref<HTMLElement>();

onMounted(() => {
  // 定义一个自定义的语法高亮样式
  const myHighlightStyle = HighlightStyle.define([
    { tag: t.keyword, color: '#c699e3' },
    { tag: t.comment, color: '#8898a3' },
    { tag: t.variableName, color: '#56c6ff' },
    { tag: t.string, color: '#86c268' },
    { tag: t.number, color: '#f38e6e' },
    // 你可以根据需要添加更多标签
  ]);

  const extensionsForHighlight = [
    LanguageDescription.of({
      name: 'javascript',
      alias: ['js', 'jsx'],
      load: () => import('@codemirror/lang-javascript').then(m => m.javascript()),
    }),
    // 你可以添加其他语言，例如：
    LanguageDescription.of({
      name: 'typescript',
      alias: ['ts', 'tsx'],
      load: () => import('@codemirror/lang-javascript').then(m => m.javascript({ jsx: true, typescript: true })),
    }),
    LanguageDescription.of({
      name: 'css',
      load: () => import('@codemirror/lang-css').then(m => m.css()),
    }),
    LanguageDescription.of({
      name: 'html',
      load: () => import('@codemirror/lang-html').then(m => m.html()),
    }),
  ];

  view = new EditorView({
    parent: cmContainer.value!,
    state: EditorState.create({
      doc: mdText.value,
      extensions: [
        keymap.of([...defaultKeymap, { key: 'Enter', run: insertNewlineAndIndent }]),
        markdown({ base: markdownLanguage, codeLanguages: extensionsForHighlight }),
        hybridPlugin,
        EditorView.updateListener.of(u => {
          if (u.docChanged) {
            const s = u.state.doc.toString();
            mdText.value = s;
            emit('update:modelValue', s);
          }
        }),
        EditorView.lineWrapping,
        // ** 添加语法高亮扩展 **
        syntaxHighlighting(myHighlightStyle),
      ],
    }),
  });
});

function insertAroundSelection(prefix: string, suffix: string = '') {
  const { from, to, empty } = view.state.selection.main;
  if (empty) {
    view.dispatch({
      changes: { from, to, insert: prefix + suffix },
      selection: EditorSelection.cursor(from + prefix.length),
    });
  } else {
    const selectedText = view.state.doc.sliceString(from, to);
    view.dispatch({
      changes: { from, to, insert: `${prefix}${selectedText}${suffix}` },
      selection: EditorSelection.range(from + prefix.length, to + prefix.length),
    });
  }
  view.focus();
}

const isBoldActive = computed(() => { if (!view) return false; const s = view.state.selection.main; return view.state.doc.sliceString(s.from - 2, s.to + 2) === '****'; });
const isItalicActive = computed(() => { if (!view) return false; const s = view.state.selection.main; return view.state.doc.sliceString(s.from - 1, s.to + 1) === '**'; });

function toggleBold() { insertAroundSelection('**', '**'); }
function toggleItalic() { insertAroundSelection('*', '*'); }
function insertLink() { insertAroundSelection('[', '](url)'); }
function insertCodeBlock() { insertAroundSelection('\n```\n', '\n```\n'); }
function insertQuote() { insertAroundSelection('\n> ', '\n'); }

// 导出上传函数
async function uploadMedia(file: File) {
  const form = new FormData();
  form.append('file', file);
  return axios.post('/media/upload', form)
    .then(res => res.data);
}

/**
 * @description: 触发文件上传，并通过文件类型调用不同的处理逻辑
 */
function triggerUpload() {
  fileInput.value?.click();
}

/**
 * @description: 处理文件选择事件，并根据文件类型进行上传
 */
async function handleFileChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;

  const fileType = file.type.split('/')[0]; // "image", "video", "audio"

  try {
    // 插入一个临时的占位符，表示正在上传
    const placeholderText = `<Uploading ${file.name}...>`;
    insertAroundSelection(placeholderText, '');

    const res = await uploadMedia(file);

    if (res.code === 200 && res.data?.fileId) {
      // 上传成功，生成并替换为最终的媒体标记
      const mediaTag = `<type: ${fileType}, fileId: '${res.data.fileId}'>`;
      const doc = view.state.doc.toString();
      const newDoc = doc.replace(placeholderText, mediaTag);
      view.dispatch({
        changes: { from: doc.indexOf(placeholderText), to: doc.indexOf(placeholderText) + placeholderText.length, insert: mediaTag },
        selection: EditorSelection.cursor(doc.indexOf(placeholderText) + mediaTag.length),
      });
      view.focus();
    } else {
      // 上传失败
      alert('上传失败：' + res.msg || '未知错误');
      // 移除占位符
      const doc = view.state.doc.toString();
      const newDoc = doc.replace(placeholderText, '');
      view.dispatch({
        changes: { from: doc.indexOf(placeholderText), to: doc.indexOf(placeholderText) + placeholderText.length, insert: '' },
      });
    }
  } catch (error) {
    alert('上传失败，请检查网络或服务器');
    // 移除占位符
    const doc = view.state.doc.toString();
    const newDoc = doc.replace(`<Uploading ${file.name}...>`, '');
    view.dispatch({
      changes: { from: doc.indexOf(`<Uploading ${file.name}...>`), to: doc.indexOf(`<Uploading ${file.name}...>`) + `<Uploading ${file.name}...>`.length, insert: '' },
    });
    console.error(error);
  } finally {
    // 重置文件输入框，以便下次可以触发 change 事件
    if (fileInput.value) {
      fileInput.value.value = '';
    }
  }
}
</script>

<style scoped>
   /*
    * =========================================================================
    * CodeMirror 主样式
    * =========================================================================
    */
 :deep(.cm-content) {
   caret-color: white;
   color: white;
   white-space: pre-wrap;
   word-break: break-word;
 }
:deep(.cm-editor) {
  outline: none !important;
}


/*
 * =========================================================================
 * Markdown 行内元素样式
 * =========================================================================
 */
:deep(span.heading-1.prose-invert) {
  font-size: 2.5em;
  color: #facc15;
  font-weight: bold;
}
:deep(span.heading-2.prose-invert) {
  font-size: 2.0em;
  color: #93c5fd;
  font-weight: bold;
}
:deep(span.heading-3.prose-invert) {
  font-size: 1.5em;
  color: #a7f3d0;
  font-weight: bold;
}
:deep(span.heading-4.prose-invert) {
  font-size: 1.3em;
  color: #fbcfe8;
  font-weight: bold;
}
:deep(span.heading-5.prose-invert) {
  font-weight: bold;
}
:deep(span.heading-6.prose-invert) {
  font-weight: bold;
}

:deep(strong) {
  font-weight: bold;
}
:deep(em) {
  font-style: italic;
}
:deep(del) {
  text-decoration: line-through;
}
:deep(code) {
  background: #1f2937;
  padding: 0.1em 0.3em;
  border-radius: 4px;
  font-family: monospace;
}
:deep(a) {
  text-decoration: underline;
}


/*
 * =========================================================================
 * 代码块渲染样式
 * 该样式通过为每一行添加类来构建一个完整的代码块容器。
 * =========================================================================
 */

/* 隐藏原始的代码块标记文本 */
:deep(.cm-codeblock-fence-start),
:deep(.cm-codeblock-fence-end) {
  font-size: 0;
  opacity: 0;
  display: none;
}

/* 通过伪元素重新显示代码块的开始标记 */
:deep(.cm-codeblock-fence-start)::before {
  content: '```';
  font-size: 16px;
  opacity: 1;
  display: block;
  color: #94a3b8;
  margin-left: -1em; /* 微调位置，使其对齐 */
  margin-bottom: 0.5em; /* 与第一行代码的间距 */
}

/* 通过伪元素重新显示代码块的结束标记 */
:deep(.cm-codeblock-fence-end)::after {
  content: '```';
  font-size: 16px;
  opacity: 1;
  display: block;
  color: #94a3b8;
  margin-left: -1em; /* 微调位置，使其对齐 */
  margin-top: 0.5em; /* 与最后一行代码的间距 */
}

/* 代码块每一行的基础样式 */
:deep(.cm-codeblock-line) {
  background-color: #0f172a;
  border-left: 1px solid #334155;
  border-right: 1px solid #334155;
  padding: 0 1em; /* 左右内边距 */
  white-space: pre; /* 确保代码块内部的空格和换行得到保留 */
  line-height: 1.5;
}

/* 代码块第一行的特殊样式（顶部边框和圆角） */
:deep(.cm-codeblock-first-line) {
  border-top-left-radius: 8px;
  border-top-right-radius: 8px;
  border-top: 1px solid #334155;
  margin-top: 1em;
}

/* 代码块最后一行的特殊样式（底部边框和圆角） */
:deep(.cm-codeblock-last-line) {
  border-bottom-left-radius: 8px;
  border-bottom-right-radius: 8px;
  border-bottom: 1px solid #334155;
  margin-bottom: 1em;
}

/* 移除 CodeMirror 默认的行高，让行内容自己撑开高度 */
:deep(.cm-line) {
  line-height: normal;
}

 :deep(.cm-vue-widget) {
   /* 确保 Vue 组件容器的显示行为与行内元素一致 */
   display: inline-block;
   vertical-align: top; /* 避免与行内文字基线对齐 */
   max-width: 100%; /* 防止图片、视频超出容器 */
 }

 /* 媒体预览容器的通用样式 */
 :deep(.media-preview) {
   margin: 0.5em 0;
   border: 1px solid #334155;
   border-radius: 8px;
   background-color: #1f2937;
   padding: 0.5em;
 }
</style>
