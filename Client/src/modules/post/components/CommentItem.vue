<template>
  <article class="p-4" :data-comment-id="comment.comment_id || comment.reply_id" :data-type="comment.type">
    <div class="flex space-x-4">
      <img class="w-10 h-10 rounded-full flex-shrink-0" :src="comment.user?.avatar || getDefaultAvatar(comment.user_id)"
           :alt="comment.user?.name || `用户${comment.user_id}`">
      <div class="flex-1">
        <div class="flex items-baseline space-x-2">
          <p class="font-bold">{{ comment.user?.name || `用户${comment.user_id}` }}</p>
          <p class="text-slate-400 text-sm">{{ formatTime(comment.created_at) }}</p>
        </div>
        <p class="mt-2 text-slate-300">{{ comment.content }}</p>
        <div class="flex space-x-4 text-slate-500 mt-2 text-sm">
          <button class="hover:text-sky-400" @click="handleReply">回复</button>
          <button class="hover:text-red-400" :class="{ 'text-red-400': isLiked }" @click="handleLike">
            赞 ({{ comment.likes || 0 }})
          </button>
          <button v-if="isCurrentUser" class="hover:text-orange-400" @click="handleDelete">删除</button>
        </div>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import type { Comment } from '../types';
import { postService } from '../api';
import { useAuthState } from '../utils/useAuthState';
const props = defineProps<{
  comment: Comment;
}>();

const emit = defineEmits<{
  (e: 'reply', comment: Comment): void;
  (e: 'delete', comment: Comment): void;
  (e: 'update:comment', comment: Comment): void;
}>();

const { currentUser } = useAuthState();
const isLiked = ref(false);

const isCurrentUser = computed(() => {
  return currentUser.value?.id === props.comment.user_id;
});

const getDefaultAvatar = (userId: number) => {
  return `https://placehold.co/100x100/7dd3fc/0f172a?text=用户${userId}`;
};

const formatTime = (timestamp: string) => {
  const date = new Date(timestamp);
  const now = new Date();
  const diff = now.getTime() - date.getTime();

  if (diff < 60000) {
    return '刚刚';
  } else if (diff < 3600000) {
    return `${Math.floor(diff / 60000)}分钟前`;
  } else if (diff < 86400000) {
    return `${Math.floor(diff / 3600000)}小时前`;
  } else {
    return date.toLocaleDateString();
  }
};

const handleReply = () => {
  emit('reply', props.comment);
};

const handleLike = async () => {
  try {
    const targetId = props.comment.comment_id || props.comment.reply_id;
    if (!targetId) return;

    // 修正点赞API调用
    const result = await postService.toggleLike({
      type: props.comment.type === 'comment' ? 'comment' : 'reply',
      targetId: targetId,
      like: !isLiked.value
    });

    if (result.code === 200) {
      isLiked.value = !isLiked.value;
      const updatedComment = {
        ...props.comment,
        likes: isLiked.value ? (props.comment.likes || 0) + 1 : Math.max(0, (props.comment.likes || 0) - 1)
      };
      emit('update:comment', updatedComment);
    }
  } catch (error) {
    console.error('点赞失败:', error);
  }
};

const handleDelete = async () => {
  if (!confirm('确定要删除吗？')) return;

  try {
    const targetId = props.comment.comment_id || props.comment.reply_id;
    if (!targetId) return;

    // 修正删除API调用
    const success = await postService.deleteComment(props.comment.type, targetId);
    if (success) {
      emit('delete', props.comment);
    }
  } catch (error) {
    console.error('删除失败:', error);
  }
};
</script>
