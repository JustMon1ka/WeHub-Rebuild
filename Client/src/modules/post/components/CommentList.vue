<template>
  <div class="border-t border-slate-800 divide-y divide-slate-800">
    <CommentForm
      :post-id="postId"
      :reply-to="currentReply"
      @submitted="loadComments"
      @cancel-reply="currentReply = undefined"
    />
    
    <div v-if="loading" class="p-4 text-center text-slate-400">
      加载中...
    </div>
    
    <div v-else-if="comments.length === 0" class="p-4 text-center text-slate-400">
      暂无评论
    </div>
    
    <template v-else>
      <CommentItem
        v-for="comment in comments"
        :key="comment.comment_id || comment.reply_id"
        :comment="comment"
        @reply="handleReply"
        @delete="handleDelete"
        @update:comment="handleCommentUpdate"
      />
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import type { Comment } from '../types';
import { postService } from '../api';
import CommentItem from './CommentItem.vue';
import CommentForm from './CommentForm.vue';

const props = defineProps<{
  postId: number;
}>();

const comments = ref<Comment[]>([]);
const loading = ref(false);
const currentReply = ref<Comment>();

const loadComments = async () => {
  loading.value = true;
  try {
    comments.value = await postService.getComments(props.postId);
  } catch (error) {
    console.error('加载评论失败:', error);
  } finally {
    loading.value = false;
  }
};

const handleReply = (comment: Comment) => {
  currentReply.value = comment;
};

const handleDelete = (comment: Comment) => {
  const index = comments.value.findIndex(c => 
    (c.comment_id && c.comment_id === comment.comment_id) ||
    (c.reply_id && c.reply_id === comment.reply_id)
  );
  
  if (index !== -1) {
    comments.value.splice(index, 1);
  }
};

const handleCommentUpdate = (updatedComment: Comment) => {
  const index = comments.value.findIndex(c => 
    (c.comment_id && c.comment_id === updatedComment.comment_id) ||
    (c.reply_id && c.reply_id === updatedComment.reply_id)
  );
  
  if (index !== -1) {
    comments.value[index] = updatedComment;
  }
};

onMounted(() => {
  loadComments();
});
</script>