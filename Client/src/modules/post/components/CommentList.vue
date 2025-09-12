<template>
  <div class="comment-section">
    <CommentForm
      :post-id="postId"
      :reply-to="currentReply"
      @submitted="handleCommentSubmitted"
      @cancel-reply="currentReply = undefined"
    />

    <div v-if="loading" class="p-4 text-center text-slate-400">
      加载中...
    </div>

    <div v-else-if="comments.length === 0" class="p-4 text-center text-slate-400">
      暂无评论
    </div>

    <template v-else>
      <div v-for="comment in comments" :key="comment.comment_id || comment.reply_id">
        <!-- 主评论 -->
        <CommentItem
          :comment="comment"
          @reply="handleReply"
          @delete="handleDelete"
          @update:comment="handleCommentUpdate"
        />

        <!-- 嵌套回复 -->
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
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import axios from 'axios';
import type { Comment } from '../types';
import { postService } from '../api';
import CommentItem from './CommentItem.vue';
import CommentForm from './CommentForm.vue';
import { convertCommentResponseToFrontend } from '../types';

const props = defineProps<{
  postId: number;
}>();

const emit = defineEmits<{
  (e: 'comment-added', totalCount: number): void;
  (e: 'comment-deleted', totalCount: number): void;
  (e: 'comment-updated', comment: Comment): void;
  (e: 'comment-count-change', newCount: number): void; // 新增事件
}>();

const comments = ref<Comment[]>([]);
const loading = ref(false);
const currentReply = ref<Comment>();

// 调试信息
const debugInfo = ref({
  apiUrl: '',
  responseData: null as any,
  error: null as any,
  apiMode: '' // 记录使用的API模式
});

// 计算评论总数
const totalCommentCount = computed(() => {
  let count = 0;
  comments.value.forEach(comment => {
    count += 1;
    if (comment.replies && Array.isArray(comment.replies)) {
      count += comment.replies.length;
    }
  });
  return count;
});


const loadComments = async () => {
  loading.value = true;
  try {
    const response = await axios.get('/posts/comments', {
      params: { postId: props.postId }
    });

    console.log('📦 API原始响应:', response.data);

    if (response.data && response.data.code === 200 && Array.isArray(response.data.data)) {

      const rawData = response.data.data;
      const processedComments: Comment[] = [];
      let lastComment: Comment | null = null; // 用于追踪最近的一个主评论

      rawData.forEach(item => {
        // 先使用您已有的转换函数将后端数据转换为前端格式
        const formattedItem = convertCommentResponseToFrontend(item);

        if (item.type === 0) {
          // 这是一个主评论
          // 为其初始化一个空的 replies 数组，以备后续添加回复
          formattedItem.replies = [];
          processedComments.push(formattedItem);
          // 更新 lastComment，使其指向当前这个主评论
          lastComment = formattedItem;
        } else if (item.type === 1 && lastComment) {
          // 这是一个回复，并且我们已经有了一个可以归属的主评论
          // 根据规则，将其添加到最近一个主评论的 replies 数组中
          lastComment.replies.push(formattedItem);
        }
      });

      comments.value = processedComments;
      console.log('✅ 转换并嵌套后的评论:', comments.value);

    } else {
      // 处理空数据或错误码的情况
      comments.value = [];
    }

  } catch (error) {
    console.error('❌ 加载评论失败:', error);
  } finally {
    loading.value = false;
  }
};

const handleReply = (comment: Comment) => {
  currentReply.value = comment;
};

const handleNestedReply = (reply: Comment) => {
  currentReply.value = reply;
};

const handleDelete = (comment: Comment) => {
  const index = comments.value.findIndex(c =>
    (c.comment_id && c.comment_id === comment.comment_id) ||
    (c.reply_id && c.reply_id === comment.reply_id)
  );

  if (index !== -1) {
    comments.value.splice(index, 1);
    emit('comment-deleted', totalCommentCount.value);
    // 不需要手动发射 comment-count-change，watch 会自动处理
  }
};

const handleCommentUpdate = (updatedComment: Comment) => {
  const index = comments.value.findIndex(c =>
    (c.comment_id && c.comment_id === updatedComment.comment_id) ||
    (c.reply_id && c.reply_id === updatedComment.reply_id)
  );

  if (index !== -1) {
    comments.value[index] = updatedComment;
    emit('comment-updated', updatedComment);
  }
};

const handleCommentSubmitted = async () => {
  await loadComments();
  currentReply.value = undefined;
  emit('comment-added', totalCommentCount.value);
  // 不需要手动发射 comment-count-change，watch 会自动处理
};

// 监听评论数变化并发射事件
watch(totalCommentCount, (newCount, oldCount) => {
  if (newCount !== oldCount) {
    console.log('📊 评论数变化:', oldCount, '→', newCount);
    emit('comment-count-change', newCount);
  }
}, { immediate: true }); // immediate: true 表示组件挂载时立即触发

onMounted(() => {
  console.log('🚀 CommentList 组件挂载，帖子ID:', props.postId);
  loadComments();
});
</script>
