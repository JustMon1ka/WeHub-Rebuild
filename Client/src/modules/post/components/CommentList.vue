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

    <!-- 在评论列表中添加嵌套回复显示 -->
    <div v-for="comment in comments" :key="comment.comment_id">
      <CommentItem
        :comment="comment"
        @reply="handleReply"
        @delete="handleDelete"
        @update:comment="handleCommentUpdate"
      />
      
      <!-- 显示嵌套回复 -->
      <div v-if="comment.replies && comment.replies.length > 0" 
           class="ml-12 pl-4 border-l-2 border-slate-800 space-y-4">
        <CommentItem
          v-for="reply in comment.replies"
          :key="reply.reply_id"
          :comment="reply"
          @reply="handleNestedReply"
          @delete="handleDelete"
          @update:comment="handleCommentUpdate"
        />
      </div>
    </div>
  </div>
</template>



<script setup lang="ts">
import { ref, onMounted } from 'vue';
import type { Comment } from '../types';
import { postService } from '../api';
import CommentItem from './CommentItem.vue';
import CommentForm from './CommentForm.vue';

const error = ref<string>('');

const props = defineProps<{
  postId: number;
}>();

const comments = ref<Comment[]>([]);
const loading = ref(false);
const currentReply = ref<Comment>();

const loadComments = async () => {
  loading.value = true;
  error.value = '';
  try {
    comments.value = await postService.getComments(props.postId);
  } catch (err) {
    console.error('加载评论失败:', err);
    error.value = '加载评论失败，请重试';
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

const handleNestedReply = (reply: Comment) => {
  // 对于嵌套回复，需要找到父评论
  const parentComment = comments.value.find(c => 
    c.comment_id === reply.parent_id || c.replies?.some(r => r.reply_id === reply.parent_id)
  );
  currentReply.value = reply;
};

const handleCommentSubmitted = () => {
  loadComments();
  currentReply.value = undefined;
};

onMounted(() => {
  loadComments();
});
</script>