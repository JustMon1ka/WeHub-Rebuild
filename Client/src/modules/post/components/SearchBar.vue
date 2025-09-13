<template>
  <div
    class="search-container"
    :class="{ 'is-focused': isFocused }"
    @click="focusInput"
  >
    <input
      ref="inputRef"
      v-model="query"
      type="text"
      class="search-input"
      :class="{ 'is-active-text': isInputActive }"
      :placeholder="placeholderText"
      @focus="handleFocus"
      @blur="handleBlur"
      @input="handleInput"
      @compositionstart="handleCompositionStart"
      @compositionend="handleCompositionEnd"
      @keydown.enter="handleSearch"
    />

    <button class="search-button" @click="handleSearch">
      <svg
        class="magnifying-glass"
        xmlns="http://www.w3.org/2000/svg"
        viewBox="0 0 512 512"
      >
        <path
          d="M416 208c0 45.9-14.9 88.3-40 122.7L502.6 457.4c12.5 12.5 12.5 32.8 0 45.3s-32.8 12.5-45.3 0L330.7 376c-34.4 25.2-76.8 40-122.7 40C93.1 416 0 322.9 0 208S93.1 0 208 0S416 93.1 416 208zM208 352a144 144 0 1 0 0-288 144 144 0 1 0 0 288z"
        />
      </svg>
    </button>

    <div v-if="showSuggestions" class="suggestions-container">
      <div
        v-for="(suggestion, index) in searchSuggestions"
        :key="index"
        class="suggestion-item"
        :class="{ 'is-hovered': hoveredIndex === index }"
        @mouseenter="hoveredIndex = index"
        @mouseleave="hoveredIndex = -1"
        @click="selectSuggestion(suggestion.keyword)"
      >
        <span v-html="highlightKeyword(suggestion.keyword)"></span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">

import { useRouter, useRoute } from "vue-router"
const router = useRouter()
import {computed, onMounted, ref, watch} from 'vue';
// 这里假设你已经提供了这两个方法
import {getPosts, getSearch, getSearchSuggestion} from '../api.ts';
import {type SearchResponse, type SearchSuggestions} from "../types.ts"

const inputRef = ref<HTMLInputElement | null>(null);
const query = ref('');
// const placeholderText = ref('');
const placeholderText = ref('搜索帖子...'); // ✅ 固定占位符
const isFocused = ref(false);
const showSuggestions = ref(false);
const searchSuggestions = ref<SearchSuggestions['data']>([]);
const hoveredIndex = ref(-1);

let timer: number | null = null;
const isComposing = ref(false);

const isInputActive = computed(() => isFocused.value || query.value.length > 0);

// 获取搜索建议
const fetchSuggestions = async (keyword?: string, limits?: number) => {
  try {
    const data: SearchSuggestions = await getSearchSuggestion(keyword, limits);
    searchSuggestions.value = data.data;
  } catch (error) {
    searchSuggestions.value = [];
  }
};

// ✅ 回到页面时自动清空搜索框
onMounted(() => {
  query.value = ""
})
const route = useRoute()
// ✅ 如果你想更保险，每次路由切换也清空
watch(() => route.fullPath, () => {
  query.value = ""
})
// 获取初始建议
// onMounted(() => {
//   fetchSuggestions('', 1).then(() => {
//     if (searchSuggestions.value.length > 0) {
//       placeholderText.value = searchSuggestions.value[0].keyword;
//     }
//   });
// });

// 处理输入焦点
const handleFocus = () => {
  isFocused.value = true;
  showSuggestions.value = true;
  // 清除初始占位符文字
  if (query.value === placeholderText.value) {
    query.value = '';
  }
  // 重新获取完整的搜索建议列表
  fetchSuggestions(query.value, 10);
};

// 处理输入失焦
const handleBlur = () => {
  // 延迟关闭建议栏，以便点击建议项时能够触发
  setTimeout(() => {
    isFocused.value = false;
    showSuggestions.value = false;
    // 如果输入框为空，恢复初始占位符
    // if (query.value.trim() === '') {
    //   fetchSuggestions('', 1).then(() => {
    //     if (searchSuggestions.value.length > 0) {
    //       placeholderText.value = searchSuggestions.value[0].keyword;
    //     }
    //   });
    // }
  }, 150);
};

// 监听输入
const handleInput = () => {
  // 如果正在进行中文输入法组合，则不发送请求
  if (isComposing.value) {
    return;
  }
  // 使用防抖处理，避免频繁调用接口
  if (timer) {
    clearTimeout(timer);
  }
  timer = setTimeout(() => {
    fetchSuggestions(query.value, 10);
  }, 300); // 300ms防抖
};

// 监听中文输入法状态
const handleCompositionStart = () => {
  isComposing.value = true;
};

const handleCompositionEnd = () => {
  isComposing.value = false;
  // 组合结束时再触发一次搜索
  handleInput();
};

// 高亮关键词
const highlightKeyword = (suggestion: string) => {
  if (!query.value) {
    return suggestion;
  }
  const regex = new RegExp(`(${query.value})`, 'gi');
  return suggestion.replace(
    regex,
    '<span class="highlighted-text">$1</span>',
  );
};

// 搜索操作
// const performSearch = async (searchQuery: string) => {
//   try {
//     const searchResult: SearchResponse = await getSearch(searchQuery);
//     if (!searchResult.data || searchResult.data.length === 0) {
//       return [];
//     }
//     const ids: string = searchResult.data.map(item => item.postId).join(',');
//     return await getPosts(ids);
//   } catch (error) {
//     alert('搜索失败，请稍后重试。');
//   }
// };

// const handleSearch = () => {
//   let finalQuery = query.value.trim();
//   // 如果输入框为空，则使用占位符内容进行搜索
//   if (!finalQuery && placeholderText.value) {
//     finalQuery = placeholderText.value;
//   }
//   if (finalQuery) {
//     const searchResults = performSearch(finalQuery);
//     // 跳转到xxxView

//   }
//   // 搜索后隐藏建议栏
//   showSuggestions.value = false;
//   inputRef.value?.blur();
// };



const handleSearch = () => {
  let finalQuery = query.value.trim();
  if (!finalQuery && placeholderText.value) {
    finalQuery = placeholderText.value;
  }
  if (finalQuery) {
    router.push({ path: "/search", query: { q: finalQuery } });
  }
  showSuggestions.value = false;
  inputRef.value?.blur();
};



// 选择搜索建议
const selectSuggestion = (suggestion: string) => {
  query.value = suggestion;
  handleSearch();
};

// 点击搜索条外部区域时，将焦点设置到输入框
const focusInput = () => {
  inputRef.value?.focus();
};
</script>

<style scoped>
.search-container {
  position: relative;
  display: flex;
  align-items: center;
  width: 300px; /* 根据需要调整 */
  height: 40px;
  background-color: #2c2c2c; /* 灰黑底 */
  border-radius: 20px 20px 20px 20px; /* 长边为矩形，短边为半圆 */
  transition: all 0.3s ease;
}

/* 搜索输入框 */
.search-input {
  flex-grow: 1;
  height: 100%;
  padding: 0 20px;
  background-color: transparent;
  border: none;
  outline: none;
  color: #c0c0c0; /* 灰白字 */
  font-size: 16px;
  caret-color: #fff; /* 光标颜色 */
}

/* 输入时文字变为亮白色 */
.search-input.is-active-text {
  color: #ffffff;
}

/* 占位符文字样式 */
.search-input::placeholder {
  color: #c0c0c0;
}

/* 搜索按钮 */
.search-button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 50px;
  height: 100%;
  background-color: transparent;
  border: none;
  cursor: pointer;
  padding-right: 10px; /* 留出空隙 */
}

.search-button .magnifying-glass {
  width: 20px;
  height: 20px;
  fill: #c0c0c0; /* 放大镜图标颜色 */
  transition: fill 0.2s ease;
}

.search-button:hover .magnifying-glass {
  fill: #fff; /* 鼠标悬停时变亮 */
}

/* 搜索建议栏 */
.suggestions-container {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  z-index: 10;
  margin-top: 5px;
  background-color: #2c2c2c;
  border-radius: 10px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.2);
  max-height: 200px;
  overflow-y: auto;
}

.suggestion-item {
  padding: 10px 20px;
  cursor: pointer;
  color: #ffffff;
  transition: background-color 0.2s ease;
}

/* 鼠标悬停时背景色变化 */
.suggestion-item.is-hovered {
  background-color: #1a1a2e; /* 夜深蓝色 */
}

/* 关键词高亮 */
.highlighted-text {
  color: #ff69b4; /* 粉色 */
  font-weight: bold;
}

/* 使用 :deep() 穿透作用域，应用到 v-html 生成的元素上 */
.suggestions-container :deep(.highlighted-text) {
  color: #ff69b4;
  font-weight: bold;
}
</style>
