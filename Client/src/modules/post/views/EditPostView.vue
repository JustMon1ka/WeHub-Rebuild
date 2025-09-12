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
  <h2 class="text-xl font-bold text-white">编辑帖子</h2>
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

      <!-- 底部按钮 -->
<footer class="sticky bottom-0 bg-slate-900 px-6 py-4 border-t border-slate-700">
  <button
    @click="save"
    :disabled="isLoading"
    class="w-full bg-gradient-to-r from-sky-500 to-sky-600 hover:from-sky-600 hover:to-sky-700 text-white font-semibold py-3 rounded-full shadow-lg transition-all disabled:opacity-50 disabled:cursor-not-allowed"
  >
    <span v-if="isLoading">保存中...</span>
    <span v-else>保存</span>
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
import CommunitySelect from '@/modules/postCreate/components/CommunitySelect.vue'
import RichTextEditor from '@/modules/postCreate/components/RichTextEditor.vue'
import TagInput from '@/modules/postCreate/components/TagInput.vue'
import { addTags } from '@/modules/postCreate/public'
import { getPostDetail, updatePost } from '@/modules/post/api'

const router = useRouter()
const route = useRoute()

// 控制面板显示／隐藏
const visible = ref(false)
// 面板左偏移：默认侧边栏宽度 4rem
const left = ref('4rem')

// 路由中的帖子 id（优先 params，其次 query）
const postId = ref<number | null>(null)

// 表单数据
const circleId = ref<number | null>(null)
const title = ref('')
const content = ref('')
const tags = ref<string[]>([])   // TagInput 用字符串名字数组

const isLoading = ref(false)

onMounted(async () => {
  visible.value = true
  const x = Number(route.query.x)
  if (!isNaN(x)) left.value = `${x}px`

  // 读取路由 id
  const pId = route.params.id ?? route.query.id
  const idNum = Number(pId)
  if (!idNum || Number.isNaN(idNum)) {
    alert('缺少帖子ID，无法编辑')
    return close()
  }
  postId.value = idNum

  // 监听关闭事件
  window.addEventListener('close-post-edit', close)

  // 加载现有帖子详情并回填
  isLoading.value = true
  try {
    const detail = await getPostDetail(idNum)
    // 回填
    circleId.value = (detail as any).circleId ?? null
    title.value = detail.title ?? ''
    content.value = detail.content ?? ''
    // tags 可能是字符串数组或对象数组，统一转成 string[]
    const rawTags = (detail as any).tags ?? []
    tags.value = Array.isArray(rawTags)
      ? rawTags.map((t:any)=> typeof t === 'string' ? t : (t.tagName ?? t.name ?? String(t)))
      : []
  } catch (err) {
    console.error(err)
    alert('加载帖子详情失败，请稍后重试')
    return close()
  } finally {
    isLoading.value = false
  }
})

onBeforeUnmount(() => {
  window.removeEventListener('close-post-edit', close)
})

async function save() {
  if (!postId.value) {
    alert('缺少帖子ID，无法保存')
    return
  }
  if (!title.value.trim()) {
    alert('请填写标题')
    return
  }
  isLoading.value = true
  try {
    // 将标签名 -> 标签ID
    let finalTagIds: number[] = []
    const tagNames = tags.value.filter(Boolean)
    if (tagNames.length > 0) {
      const tagRes = await addTags(tagNames)
      finalTagIds = (tagRes?.data || []).map((t: any) => Number(t.tagId)).filter(Boolean)
    }

    // 组织 payload（根据你的后端实际字段名调整）
    const payload = {
      postId: postId.value,
      circleId: circleId.value,           // 如果后端不允许修改圈子，可删除这一行
      title: title.value,
      content: content.value,
      tags: finalTagIds                   // 若后端要求传名字而非ID，则改为 tagNames
    }

    // 调用后端更新
    const res = await updatePost(payload)
    // 这里不假设 unwrap 后结构，按你项目习惯自行判断
    alert('保存成功')
    close()
  } catch (err) {
    console.error(err)
    alert('保存失败，请稍后重试')
  } finally {
    isLoading.value = false
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
