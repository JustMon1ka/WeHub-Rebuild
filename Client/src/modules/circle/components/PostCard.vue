<template>
  <div
    class="post-card bg-white rounded-lg shadow-md p-6 mb-4 cursor-pointer hover:shadow-lg transition-shadow"
    @click="goToPostDetail"
  >
    <h3 class="text-xl font-semibold text-gray-800 mb-3">{{ post.title }}</h3>
    <p class="text-gray-600 mb-4 line-clamp-3">{{ post.content }}</p>

    <div class="flex items-center justify-between text-sm text-gray-500">
      <div class="flex items-center space-x-4">
        <span class="flex items-center">
          <svg class="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
            <path d="M10 12a2 2 0 100-4 2 2 0 000 4z" />
            <path
              fill-rule="evenodd"
              d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z"
              clip-rule="evenodd"
            />
          </svg>
          {{ post.views }}
        </span>
        <span class="flex items-center">
          <svg class="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
            <path
              fill-rule="evenodd"
              d="M3.172 5.172a4 4 0 015.656 0L10 6.343l1.172-1.171a4 4 0 115.656 5.656L10 17.657l-6.828-6.829a4 4 0 010-5.656z"
              clip-rule="evenodd"
            />
          </svg>
          {{ post.likes }}
        </span>
      </div>
      <span>{{ formatDate(post.createdAt) }}</span>
    </div>

    <div v-if="post.tags.length > 0" class="mt-3">
      <span
        v-for="tag in post.tags"
        :key="tag"
        class="inline-block bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded-full mr-2"
      >
        {{ tag }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { Post } from '../types'

interface Props {
  post: Post
}

const props = defineProps<Props>()
const router = useRouter()

const goToPostDetail = () => {
  router.push(`/post/${props.post.postId}`)
}

const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}
</script>

<style scoped>
.line-clamp-3 {
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.post-card:hover {
  transform: translateY(-2px);
}
</style>
