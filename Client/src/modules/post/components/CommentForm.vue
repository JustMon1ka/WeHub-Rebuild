<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import type { Comment, CommentRequest } from '../types';
import { postService, CommentType } from '../api';
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
    console.log('🔍 开始提交，回复对象:', props.replyTo);
    
    // 详细检查回复对象
    if (props.replyTo) {
      console.log('📋 回复对象详情:', {
        comment_id: props.replyTo.comment_id,
        reply_id: props.replyTo.reply_id,
        type: props.replyTo.type,
        user_id: props.replyTo.user_id
      });
    }

    // 修复targetId计算逻辑
    let targetId = props.postId; // 默认是帖子ID
    
    if (props.replyTo) {
      // 对于回复，targetId应该是被回复的评论ID
      targetId = props.replyTo.comment_id || props.replyTo.reply_id;
      
      if (!targetId) {
        throw new Error('无法获取被回复评论的ID');
      }
      
      console.log('🎯 计算出的targetId:', targetId);
    }

    const type = props.replyTo ? CommentType.Reply : CommentType.Comment;
    
    const commentData: CommentRequest = {
      type: type,
      targetId: targetId, // 使用计算出的正确targetId
      content: content.value.trim()
    };

    console.log('📤 最终提交的数据:', commentData);
    
    const result = await postService.submitComment(commentData);
    console.log('✅ 提交成功，响应:', result);
    
    content.value = '';
    emit('submitted');
    
    if (props.replyTo) {
      emit('cancel-reply');
    }
  } catch (error) {
    console.error('❌ 提交失败:', error);
    alert(error.message || '提交评论失败，请重试');
  } finally {
    isSubmitting.value = false;
  }
};


const cancelReply = () => {
  content.value = '';
  emit('cancel-reply');
};
</script>

<template>
  <div class="p-4 border-t border-slate-800" :class="{ 'bg-slate-800/50': isReply }">
    <!-- 回复提示 -->
    <div v-if="isReply" class="flex items-center text-sm text-slate-400 mb-2">
      <span>回复 @{{ replyTo?.user?.name || `用户${replyTo?.user_id}` }}</span>
      <button @click="cancelReply" class="ml-2 text-sky-400 hover:text-sky-300 text-xs">
        [取消回复]
      </button>
    </div>
    
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
            :disabled="isSubmitting || !content.trim()"
          >
            {{ isSubmitting ? '提交中...' : isReply ? '回复' : '评论' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>