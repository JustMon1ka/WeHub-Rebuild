<!--
用法：
<script>
import PostList from '@/modules/post/components/PostList.vue';
const listRef = ref();
<script>

<template>
  <PostList ref="listRef" :num="10" :tailPostId="null" @loaded="(list)=>{}" @error="(e)=>{}"/>
</template>
-->
<template>
  <section class="w-full">
    <!-- 列表 -->
    <PostInfoCard v-for="p in posts" :key="p.postId" :post="p" />

    <!-- 加载状态/空态 -->
    <div v-if="loading" class="py-6 text-center text-slate-500">加载中…</div>
    <div v-else-if="!posts.length" class="py-6 text-center text-slate-500">暂无帖子</div>

    <!-- 加载更多 -->
    <div v-if="showLoadMore && posts.length" class="py-4 flex justify-center">
      <button @click="loadMore" :disabled="loading"
        class="px-4 py-2 rounded-2xl border border-slate-800 hover:bg-slate-800 disabled:opacity-60">
        {{ loading ? '加载中…' : '加载更多' }}
      </button>
    </div>
  </section>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import PostInfoCard from '@/modules/post/components/PostInfoCard.vue';
import { type PostListItem } from '@/modules/post/types';
import { getPostList } from '@/modules/post/api';

const props = withDefaults(defineProps<{
  num?: number; // 每次拉取数量
  tailPostId?: number | null; // 首次拉取时可指定从某个 id 之后
  autoLoad?: boolean; // 是否在挂载时自动加载
  showLoadMore?: boolean; // 显示“加载更多”按钮
  PostMode?: number;
  tagName?: string | null; 
}>(), { num: 10, tailPostId: null, autoLoad: true, showLoadMore: true, PostMode: 0, tagName: null  });

const emit = defineEmits<{ (e: 'loaded', list: PostListItem[]): void; (e: 'error', err: unknown): void }>();

const posts = ref<PostListItem[]>([]);
const loading = ref(false);

async function fetchOnce() {
  loading.value = true;
  try {
    const lastId = posts.value.length ? posts.value[posts.value.length - 1].postId : (props.tailPostId ?? undefined);
    const list = await getPostList(props.num, lastId, props.PostMode, props.tagName);
    if (list?.length) posts.value.push(...list);
    emit('loaded', list || []);
  } catch (e) { emit('error', e); }
  finally { loading.value = false; }
}

function loadMore() { if (!loading.value) fetchOnce(); }

onMounted(() => { if (props.autoLoad) fetchOnce() });

// 对外暴露刷新能力
function refresh() { posts.value = []; fetchOnce(); }
// 供父组件通过 ref 调用：<PostList ref="listRef"/>, listRef.value.refresh()
// ts-expect-error 暴露实例方法
defineExpose({ refresh })

const showLoadMore = ref(props.showLoadMore);
watch(() => props.showLoadMore, v => showLoadMore.value = v);
</script>
