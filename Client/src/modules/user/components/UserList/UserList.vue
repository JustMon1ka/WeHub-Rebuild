<script setup lang="ts">
import { onMounted, onUnmounted, type Ref, ref } from 'vue'
import UserCardList from '@/modules/user/components/UserList/UserCardList.vue'
import styles from '@/modules/user/scripts/Styles.ts'
import {nanoid} from 'nanoid'

const { generator }= defineProps<{
  // 生成器函数，用于模拟加载用户数据，yield 返回用户ID列表，每次调用传入一个数字表示请求的数量，
  // return返回一个字符串，非空时表示加载失败，值表示错误信息，
  // 初始时会调用一次next()用于初始化，此时返回空数组即可，后续调用next()再传入数字表示请求的用户数量
  // 样例参见 FollowList.vue 第 37 行 的generator的定义
  generator: AsyncGenerator<Array<string>, string, number>;
  followBtn?: boolean;
}>();

const PAGE_SIZE = 10; // 每次加载的用户数量
const state = ref({
  end: false, // 是否已加载完所有用户
  loading: false, // 是否正在加载数据
  error: false, // 是否发生错误
  errorMsg: '', // 错误信息
})

const users : Ref<Array<string>> = ref([]);
generator.next(); // 初始化生成器，第一次调用next()以便开始生成用户数据

// 加载数据
const loadMore = async () => {
  if (state.value.loading) return;

  state.value.loading = true;
  try {
    const newItems = await generator.next(PAGE_SIZE);
    if (newItems.done) {
      state.value.end = true;
      if (newItems.value) {
        state.value.error = true;
        state.value.errorMsg = newItems.value;
      }
      return;
    }
    users.value.push(...(newItems.value));
  } catch (error) {
    state.value.end = true;
    state.value.error = true;
    state.value.errorMsg = `加载失败: ${error instanceof Error ? error.message : '未知错误'}`;
  } finally {
    state.value.loading = false;
  }
};

let observer: IntersectionObserver | null = null;
const uid = nanoid(8); // 生成唯一的哨兵元素ID

onMounted(() => {
  const sentinelElement = document.querySelector(`[data-uid="${uid}"]`) as HTMLElement | null;
  if (!sentinelElement) return;

  observer = new IntersectionObserver(
    (entries) => {
      const [entry] = entries;
      if (entry.isIntersecting) loadMore();
    },
    { threshold: 0.1 } // 当哨兵元素 10% 进入视口时触发
  );

  observer.observe(sentinelElement);
});

// 组件卸载时取消观察

onUnmounted(() => {
  if (observer) observer.disconnect();
});
</script>

<template>
  <div class="w-full y-auto relative">
    <!-- 用户列表 -->
    <div v-for="(user, index) in users" :key="user"
         class="flex items-start space-x-4 hover:bg-slate-800/50 transition-colors duration-200">
      <UserCardList :user-id="user" :follow-btn="followBtn"/>
    </div>

    <!-- 加载组件 -->
    <div v-if="!state.end" :data-uid="uid" class="flex items-center justify-center p-4">
      <div class="w-full p-4">
        <div class="flex animate-pulse space-x-4">
          <div class="p-4 size-12 rounded-full bg-gray-200"></div>
          <div class="flex-1 space-y-6 py-1">
            <div class="h-2 rounded bg-gray-200"></div>
            <div class="space-y-3">
              <div class="grid grid-cols-3 gap-4">
                <div class="col-span-2 h-2 rounded bg-gray-200"></div>
                <div class="col-span-1 h-2 rounded bg-gray-200"></div>
              </div>
              <div class="h-2 rounded bg-gray-200"></div>
              <div class="grid grid-cols-3 gap-4">
                <div class="col-span-1 h-2 rounded bg-gray-200"></div>
                <div class="col-span-2 h-2 rounded bg-gray-200"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 结束标志 -->
    <div v-else-if="state.end && !state.error" class="p-4 text-sm text-center text-slate-500">
      <p>· 到底了 ·</p>
    </div>

    <!-- 错误信息 -->
    <div v-else class="p-4 text-center items-center justify-center">
      <label :class="styles.error">{{ state.errorMsg }}</label>
    </div>
  </div>
</template>
