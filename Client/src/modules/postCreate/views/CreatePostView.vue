<template>
  <transition
    enter-active-class="transition-transform duration-300 ease-out"
    leave-active-class="transition-transform duration-200 ease-in"
    enter-from-class="-translate-x-full"
    enter-to-class="translate-x-0"
    leave-from-class="translate-x-0"
    leave-to-class="-translate-x-full"
  >
    <div
      v-if="visible"
      class="z-50 h-full bg-slate-900 border-x border-slate-700 shadow-2xl flex flex-col"
      :style="{ left }"
    >
      <!-- 标题栏 -->
      <header class="sticky top-0 z-10 bg-slate-900/75 backdrop-blur-md flex items-center justify-between px-6 py-4 border-b border-slate-700">
        <h2 class="text-xl font-bold text-white">创建新帖子</h2>
        <button @click="close" class="p-2 rounded-full hover:bg-slate-800 transition-colors">
          <img src="@/assets/close.svg" alt="close" class="w-5 h-5" />
        </button>
      </header>

      <!-- 表单区 -->
      <main class="flex-1 p-6 space-y-8">
        <!-- 社区选择 -->
        <div>
          <label class="block text-sm font-medium text-slate-300 mb-2">选择社区</label>
          <CommunitySelect
            v-model="circleId"
            class="w-full sm:w-80 bg-slate-800 border border-slate-700 rounded-md shadow-inner py-2 px-3 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-sky-500 text-slate-200"
          />
        </div>

        <!-- 帖子标题 -->
        <div>
          <input
            v-model="title"
            type="text"
            placeholder="帖子标题"
            class="w-full bg-transparent border-b-2 border-slate-700 focus:border-sky-500 text-3xl font-extrabold py-2 placeholder-slate-400 text-white focus:outline-none"
          />
        </div>

        <!-- 富文本编辑器 -->
        <div>
          <label class="block text-sm font-medium text-slate-300 mb-2">正文内容</label>
          <div class="bg-slate-800 border border-slate-700 rounded-md">
            <!-- 编辑器本体 -->
            <RichTextEditor v-model="content" class="rounded-b-md min-h-[300px] p-3 text-slate-200 bg-slate-800" />
          </div>
        </div>

        <!-- 标签输入 -->
        <div>
          <label class="block text-sm font-medium text-slate-300 mb-2">标签 (可选)</label>
          <TagInput
            v-model="tags"
            class="w-full bg-slate-800 border border-slate-700 rounded-md py-2 px-3 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-sky-500 text-slate-200 placeholder-slate-400"
            placeholder="例如：游戏攻略, 赛博朋克"
          />
        </div>
      </main>

      <!-- 发布按钮 -->
      <footer class="sticky bottom-0 bg-slate-900 px-6 py-4 border-t border-slate-700">
        <button
          @click="publish"
          :disabled="isLoading"
          class="w-full bg-gradient-to-r from-sky-500 to-sky-600 hover:from-sky-600 hover:to-sky-700 text-white font-semibold py-3 rounded-full shadow-lg transition-all disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <span v-if="isLoading">发布中...</span>
          <span v-else>发布</span>
        </button>
      </footer>

      <transition
        enter-active-class="transition-opacity duration-200 ease-out"
        leave-active-class="transition-opacity duration-200 ease-in"
        enter-from-class="opacity-0"
        enter-to-class="opacity-100"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
      >
        <div v-if="isLoading" class="absolute inset-0 bg-slate-900/80 backdrop-blur-sm flex items-center justify-center z-20">
          <div class="w-12 h-12 border-4 border-slate-500 border-t-sky-500 rounded-full animate-spin"></div>
        </div>
      </transition>
    </div>
  </transition>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import CommunitySelect from '../components/CommunitySelect.vue'
import RichTextEditor from '../components/RichTextEditor.vue'
import TagInput from '../components/TagInput.vue'
import {addTags, publishPost} from '../public'
import { sharePost } from "../../post/api";

const router = useRouter()
const route = useRoute()

// 控制面板显示／隐藏
const visible = ref(false)
// 面板左偏移：默认侧边栏宽度 4rem
const left = ref('4rem')

// 表单数据
const circleId = ref<number | null>(null)
const title = ref('')
const content = ref('')
const tags = ref<string[]>([])

const isLoading = ref(false)

onMounted(() => {
  visible.value = true
  const x = Number(route.query.x)
  if (!isNaN(x)) left.value = `${x}px`

  // 监听关闭事件
  window.addEventListener('close-post-create', close)
})

onBeforeUnmount(() => {
  window.removeEventListener('close-post-create', close)
})

async function publish() {
  // 0. 判断是否是分享模式
  const shareFrom = route.query.shareFrom as string | undefined
  isLoading.value = true // <--- 新增：开始加载，显示蒙版
  try {
    // 1. 先处理标签（仅普通发帖需要）
    // 假设 TagInput 的 v-model 绑定的就是标签名数组（string[]）
    // 如果 TagInput 输出的是 id，需要先改成 names
    let finalTagIds: number[] = []
    if (!shareFrom) {
      const tagNames = tags.value // 如果不是字符串数组，要先转换

      if (tagNames.length > 0) {
        // 调用添加标签 API（返回值里应该有所有标签的 id）
        const tagRes = await addTags(tagNames)
        // 确保 finalTagIds 里都是 number
        finalTagIds = tagRes.data.map((t: any) => t.tagId)
      }
    }

    // 2. 根据模式调用 API
    if (shareFrom) {
      // 分享模式 → 调用分享接口
      const res = await sharePost(Number(shareFrom), content.value)
      alert('分享成功，ID: ' + res.data?.postId) // 注意根据实际返回字段调整
    } else {
      // 普通发帖 → 调用发布帖子 API
      const payload = {
        circleId: circleId.value,
        title: title.value,
        content: content.value,
        tags: finalTagIds
      }
      const res = await publishPost(payload)
      alert('发布成功，ID: ' + res.data?.postId) // 注意根据实际返回字段调整
    }

    // 3. 关闭弹窗 / 返回
    close()
  } catch (err) {
    alert(shareFrom ? '分享失败，请重试' : '发布失败，请重试')
  } finally {
    isLoading.value = false // <--- 新增：结束加载，隐藏蒙版
  }
}

function close() {
  visible.value = false
  setTimeout(() => router.back(), 200)
}

</script>

<style scoped>
@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
.animate-spin {
  animation: spin 1s linear infinite;
}
</style>
