<template>
  <div class="p-4 border-t border-slate-800">
    <div class="flex space-x-3">
      <img class="w-10 h-10 rounded-full flex-shrink-0" :src="userAvatar" alt="你的头像">
      <div class="flex-1">
        <textarea
          v-model="content"
          class="w-full bg-slate-800 border border-slate-700 rounded-lg p-3 focus:outline-none focus:ring-2 focus:ring-sky-500"
          :placeholder="placeholder"
          rows="3"
        ></textarea>
        <div class="flex justify-end mt-2 space-x-2">
          <button
            v-if="isReply"
            class="bg-slate-700 hover:bg-slate-600 text-sm py-2 px-4 rounded-full"
            @click="cancelReply"
          >
            取消
          </button>
          <button
            class="bg-sky-500 hover:bg-sky-400 text-sm py-2 px-4 rounded-full"
            @click="submitComment"
            :disabled="isSubmitting"
          >
            {{ isSubmitting ? '提交中...' : '评论' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import type { Comment, CommentRequest } from '../types';
import { postService } from '../api';
import { useAuthState } from '../utils/useAuthState';

const props = defineProps<{
  postId: number;
  replyTo?: Comment;
}>();

const emit = defineEmits<{
  (e: 'submitted'): void;
  (e: 'cancel-reply'): void;
}>();

const authStore = useAuthState();
const content = ref('');
const isSubmitting = ref(false);

const isReply = computed(() => !!props.replyTo);
const userAvatar = computed(() => authStore.currentUser.value?.avatar || 'https://placehold.co/100x100/7dd3fc/0f172a?text=头像');

const placeholder = computed(() => {
  if (props.replyTo) {
    const username = props.replyTo.user?.name || `用户${props.replyTo.user_id}`;
    return `回复 @${username}...`;
  }
  return '写下你的评论...';
});

watch(() => props.replyTo, (newReplyTo) => {
  if (newReplyTo) {
    const username = newReplyTo.user?.name || `用户${newReplyTo.user_id}`;
    content.value = `@${username} `;
  } else {
    content.value = '';
  }
});

const submitComment = async () => {
  if (!content.value.trim()) {
    alert('评论内容不能为空');
    return;
  }

  isSubmitting.value = true;

  try {
    // 修正请求参数格式
    const commentData: CommentRequest = {
      post_id: props.postId,
      content: content.value.trim(),
      parent_id: props.replyTo ? (props.replyTo.comment_id || props.replyTo.reply_id || 0) : 0,
      reply_to_user_id: props.replyTo ? props.replyTo.user_id : undefined
    };

    await postService.submitComment(commentData);
    content.value = '';
    emit('submitted');
    
    // 如果是回复，取消回复状态
    if (props.replyTo) {
      emit('cancel-reply');
    }
  } catch (error) {
    console.error('提交评论失败:', error);
    alert('提交评论失败，请重试');
  } finally {
    isSubmitting.value = false;
  }
};

const cancelReply = () => {
  content.value = '';
  emit('cancel-reply');
};
</script>