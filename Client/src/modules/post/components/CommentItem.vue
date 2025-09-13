<template>
  <article
    class="p-4"
    :data-comment-id="comment.comment_id || comment.reply_id"
    :data-type="comment.type"
  >
    <div class="flex space-x-4">
      <img
        v-if="!!comment.user?.avatarUrl"
        :src="comment.user?.avatarUrl"
        class="w-16 h-16 rounded-full"
        alt="avatar"
      />
      <PlaceHolder
        v-else
        width="80"
        height="80"
        :text="comment.user?.nickName || comment.user?.username"
        class="w-16 h-16 rounded-full"
      />
      <div class="flex-1">
        <div class="flex items-baseline space-x-2">
          <!-- 主要修改这里：username → nickname -->
          <p class="font-bold">
            {{ comment.user?.nickName || comment.user?.username || `用户${comment.user_id}` }}
          </p>
          <p class="text-slate-400 text-sm">{{ formatTime(comment.created_at) }}</p>
        </div>
        <p class="mt-2 text-slate-300">{{ comment.content }}</p>
        <div class="flex space-x-4 text-slate-500 mt-2 text-sm">
          <button class="hover:text-sky-400" @click="handleReply">回复</button>
          <button
            class="hover:text-red-400"
            :class="{ 'text-red-400': isLiked }"
            @click="handleLike"
          >
            赞 ({{ comment.likes || 0 }})
          </button>
          <button v-if="isCurrentUser" class="hover:text-orange-400" @click="handleDelete">
            删除
          </button>
          <ReportCommentButton
            v-if="!isCurrentUser && (comment.comment_id || comment.reply_id)"
            :comment-id="comment.comment_id || comment.reply_id || 0"
          />
        </div>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import type { Comment } from '../types'
import { postService } from '../api'
import { useAuthState } from '../utils/useAuthState'
import PlaceHolder from '@/modules/user/components/PlaceHolder.vue'
import ReportCommentButton from './ReportCommentButton.vue'
const props = defineProps<{
  comment: Comment
}>()

const emit = defineEmits<{
  (e: 'reply', comment: Comment): void
  (e: 'delete', comment: Comment): void
  (e: 'update:comment', comment: Comment): void
}>()

const { currentUser } = useAuthState()
const isLiked = ref(false)

const isCurrentUser = computed(() => {
  return currentUser.value?.id === props.comment.user_id
})

const getDefaultAvatar = (userId: number) => {
  return `https://placehold.co/100x100/7dd3fc/0f172a?text=用户${userId}`
}

const formatTime = (timestamp: string) => {
  const date = new Date(timestamp)
  const now = new Date()
  const diff = now.getTime() - date.getTime()

  if (diff < 60000) {
    return '刚刚'
  } else if (diff < 3600000) {
    return `${Math.floor(diff / 60000)}分钟前`
  } else if (diff < 86400000) {
    return `${Math.floor(diff / 3600000)}小时前`
  } else {
    return date.toLocaleDateString()
  }
}

const handleReply = () => {
  emit('reply', props.comment)
}

const handleLike = async () => {
  try {
    const targetId = props.comment.comment_id || props.comment.reply_id

    if (!targetId) {
      return
    }

    // 使用小驼峰命名规范
    const result = await postService.toggleLike({
      type: props.comment.type === 'comment' ? 'comment' : 'reply',
      targetId: targetId, // 小驼峰
      like: !isLiked.value,
    })

    if (result.code === 200) {
      isLiked.value = !isLiked.value
      const updatedComment = {
        ...props.comment,
        likes: isLiked.value
          ? (props.comment.likes || 0) + 1
          : Math.max(0, (props.comment.likes || 0) - 1),
        isLiked: isLiked.value, // 也改为小驼峰
      }
      emit('update:comment', updatedComment)
    }
  } catch (error) {
    return
  }
}

const handleDelete = async () => {
  if (!confirm('确定要删除吗？')) return

  try {
    const targetId = props.comment.comment_id || props.comment.reply_id
    if (!targetId) return

    // 修正删除API调用
    const success = await postService.deleteComment(props.comment.type, targetId)
    if (success) {
      emit('delete', props.comment)
    }
  } catch (error) {
    return
  }
}
</script>
