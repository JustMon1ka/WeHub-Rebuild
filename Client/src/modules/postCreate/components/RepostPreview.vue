<template>
  <a
    :href="postUrl"
    target="_blank"
    rel="noopener noreferrer"
    class="repost-preview-container"
    title="点击查看原帖"
  >
    <div class="repost-icon-wrapper">
      <svg
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
        stroke-width="1.5"
        stroke="currentColor"
        class="w-5 h-5"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          d="M7.217 10.907a2.25 2.25 0 1 0 0 2.186m0-2.186c.18.324.283.696.283 1.093s-.103.77-.283 1.093m0-2.186 9.566-5.314m-9.566 7.5 9.566 5.314m0 0a2.25 2.25 0 1 0 3.935 2.186 2.25 2.25 0 0 0-3.935-2.186Zm0-12.814a2.25 2.25 0 1 0 3.933-2.186 2.25 2.25 0 0 0-3.933 2.186Z"
        />
      </svg>
    </div>
    <div class="repost-content">
      <div class="repost-label">转发帖子</div>
      <div class="repost-title">{{ title }}</div>
    </div>
  </a>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  postId: string;
  title: string;
}>();

// 此处假设您的帖子路由为 /posts/:id，请根据您的实际情况修改
const postUrl = computed(() => `/posts/${props.postId}`);
</script>

<style scoped>
.repost-preview-container {
  display: inline-flex;
  align-items: center;
  background-color: #334155; /* slate-700 */
  border: 1px solid #475569; /* slate-600 */
  border-radius: 8px;
  padding: 0.5rem 0.75rem; /* p-2 pr-3 */
  margin: 0.5rem 0;
  max-width: 100%;
  text-decoration: none;
  color: inherit;
  cursor: pointer;
  transition: background-color 0.2s;
  box-sizing: border-box;
}

.repost-preview-container:hover {
  background-color: #475569; /* slate-600 */
}

.repost-icon-wrapper {
  flex-shrink: 0;
  margin-right: 0.75rem; /* mr-3 */
  color: #cbd5e1; /* slate-300 */
}

.repost-content {
  min-width: 0; /* 防止 flex 布局中文本溢出 */
}

.repost-label {
  font-size: 0.75rem; /* text-xs */
  font-weight: 600; /* font-semibold */
  color: #94a3b8; /* slate-400 */
  line-height: 1;
}

.repost-title {
  color: #f1f5f9; /* slate-100 */
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-top: 0.25rem;
  font-size: 0.875rem; /* text-sm */
  line-height: 1.25;
}
</style>
