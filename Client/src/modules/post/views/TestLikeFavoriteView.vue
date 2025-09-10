<template>
  <div class="container mx-auto max-w-screen-xl">
    <div class="grid grid-cols-1 md:grid-cols-10 lg:grid-cols-12 gap-x-4">
      
      <!-- 左侧导航栏 (保持不变) -->
      <aside class="hidden md:block md:col-span-2 lg:col-span-2 p-4 sticky top-0 h-screen">
        <!-- ... 左侧导航栏内容保持不变 ... -->
      </aside>

      <!-- 中间主内容区 -->
      <main class="col-span-1 md:col-span-8 lg:col-span-7 border-x border-slate-800 min-h-screen">
        <!-- 页面头部 -->
        <div class="sticky top-0 z-10 bg-slate-900/80 backdrop-blur-md flex items-center p-4 border-b border-slate-800">
          <button class="p-2 rounded-full hover:bg-slate-800 mr-4" @click="goBack">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"></path></svg>
          </button>
          <h1 class="text-xl font-bold">组件测试页面</h1>
        </div>

        <!-- 帖子内容 -->
        <div class="p-4">
          <!-- 作者信息 -->
          <div class="flex items-center space-x-3">
            <img class="w-12 h-12 rounded-full" src="https://placehold.co/100x100/7dd3fc/0f172a?text=头像" alt="User Avatar">
            <div>
              <p class="font-bold">测试用户</p>
              <p class="text-sm text-slate-500">@testuser</p>
            </div>
          </div>
          
          <!-- 帖子内容 -->
          <h2 class="text-2xl font-bold mt-4">测试帖子标题 (ID: 100072)</h2>
          <p class="mt-4 text-slate-300 leading-relaxed">
            这是一个用于测试点赞、收藏和分享功能的帖子。帖子ID为100072，确保API调用正确。
          </p>
          <div class="mt-4 rounded-2xl border border-slate-800 overflow-hidden">
            <img src="https://placehold.co/600x350/1e40af/dbeafe?text=测试图片" alt="Test Image" class="w-full h-auto object-cover">
          </div>
          <p class="text-sm text-slate-500 mt-4">发布于 刚刚</p>
          
          <!-- 互动数据 -->
          <div class="flex items-center space-x-6 py-4 border-y border-slate-800 mt-4">
            <p><span class="font-bold text-white">{{ testPost.shares || 0 }}</span> <span class="text-slate-500">转发</span></p>
            <p><span class="font-bold text-white">{{ testPost.likes }}</span> <span class="text-slate-500">赞</span></p>
            <p><span class="font-bold text-white">{{ testPost.favorites || 0 }}</span> <span class="text-slate-500">收藏</span></p>
          </div>
          
          <!-- 测试组件区域 -->
          <div class="bg-slate-800 p-4 rounded-lg mt-6">
            <h3 class="text-lg font-bold mb-4 text-sky-400">组件测试区域 - 帖子ID: 100072</h3>
            
            <!-- 状态显示 -->
            <div class="bg-slate-900 p-4 rounded-lg mb-4">
              <h4 class="font-semibold mb-2 text-green-400">当前状态</h4>
              <div class="grid grid-cols-2 gap-4 text-sm">
                <div>点赞状态: <span :class="testPost.isLiked ? 'text-red-400' : 'text-slate-400'">{{ testPost.isLiked ? '已点赞' : '未点赞' }}</span></div>
                <div>点赞数: <span class="text-white">{{ testPost.likes }}</span></div>
                <div>收藏状态: <span :class="testPost.isFavorited ? 'text-yellow-400' : 'text-slate-400'">{{ testPost.isFavorited ? '已收藏' : '未收藏' }}</span></div>
                <div>收藏数: <span class="text-white">{{ testPost.favorites || 0 }}</span></div>
              </div>
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
              <!-- 点赞按钮测试 -->
              <div class="bg-slate-900 p-4 rounded-lg">
                <h4 class="font-semibold mb-2">点赞按钮测试</h4>
                <LikeButton
                  :post-id="100072"
                  :is-liked="testPost.isLiked"
                  :like-count="testPost.likes"
                  @update:isLiked="handleLikeUpdate"
                  @update:likeCount="handleLikeCountUpdate"
                  @error="handleError"
                />
                <div class="mt-2 text-xs text-slate-400">
                  当前: {{ testPost.isLiked ? '已点赞' : '未点赞' }} ({{ testPost.likes }})
                </div>
              </div>
              
              <!-- 收藏按钮测试 -->
              <div class="bg-slate-900 p-4 rounded-lg">
                <h4 class="font-semibold mb-2">收藏按钮测试</h4>
                <FavoriteButton
                  :post-id="100072"
                  :is-favorited="testPost.isFavorited"
                  :favorite-count="testPost.favorites"
                  :show-count="true"
                  @update:isFavorited="handleFavoriteUpdate"
                  @update:favoriteCount="handleFavoriteCountUpdate"
                  @error="handleError"
                />
                <div class="mt-2 text-xs text-slate-400">
                  当前: {{ testPost.isFavorited ? '已收藏' : '未收藏' }} ({{ testPost.favorites || 0 }})
                </div>
              </div>
              
              <!-- 分享按钮测试 -->
              <div class="bg-slate-900 p-4 rounded-lg">
                <h4 class="font-semibold mb-2">分享按钮测试</h4>
                <ShareButton
                  :post-id="100072"
                />
                <div class="mt-2 text-xs text-slate-400">
                  点击测试分享功能
                </div>
              </div>
            </div>
            
            <!-- 互动按钮行 -->
            <div class="flex justify-around text-slate-500 mt-2">
              <button class="flex-1 flex items-center justify-center space-x-2 py-3 rounded-md hover:bg-slate-800 hover:text-sky-400">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"></path></svg>
                <span>回复</span>
              </button>
              <button class="flex-1 flex items-center justify-center space-x-2 py-3 rounded-md hover:bg-slate-800 hover:text-green-400">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h5M20 20v-5h-5M4 20h5v-5M20 4h-5v5"></path></svg>
                <span>转发</span>
              </button>
              
              <!-- 集成到互动行的点赞按钮 -->
              <LikeButton
                :post-id="100072"
                :is-liked="testPost.isLiked"
                :like-count="testPost.likes"
                @update:isLiked="handleLikeUpdate"
                @update:likeCount="handleLikeCountUpdate"
                @error="handleError"
                class="flex-1 flex items-center justify-center space-x-2 py-3 rounded-md"
              />
              
              <!-- 集成到互动行的分享按钮 -->
              <ShareButton
                :post-id="100072"
                class="flex-1 flex items-center justify-center space-x-2 py-3 rounded-md"
              />
              
              <button class="flex-1 flex items-center justify-center space-x-2 py-3 rounded-md hover:bg-slate-800 hover:text-orange-400">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1v12z"></path></svg>
                <span>举报</span>
              </button>
            </div>
          </div>
        </div>

        <!-- 控制台信息 -->
        <div class="bg-slate-800 p-4 rounded-lg mt-6 mx-4">
          <h4 class="font-semibold mb-2 text-purple-400">控制台输出</h4>
          <div class="bg-slate-900 p-3 rounded text-xs font-mono text-slate-300 h-32 overflow-y-auto">
            <div v-for="(log, index) in consoleLogs" :key="index" class="mb-1">
              {{ log }}
            </div>
          </div>
        </div>
      </main>

      <!-- 右侧边栏 (保持不变) -->
      <aside class="hidden lg:block lg:col-span-3 p-4 sticky top-0 h-screen">
        <!-- ... 右侧边栏内容保持不变 ... -->
      </aside>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
// 引入你的测试组件
import { LikeButton, FavoriteButton, ShareButton } from "../public";
// 引入API函数
import { getPostDetail } from "../../post/api";

const router = useRouter();
const consoleLogs = ref<string[]>([]);

// 测试用的帖子状态
const testPost = ref({
  id: 100072,
  title: '测试帖子标题',
  content: '这是测试帖子的内容...',
  likes: 5,
  favorites: 3,
  shares: 2,
  isLiked: false,
  isFavorited: false
});

const goBack = () => {
  router.back();
};

// 添加日志到控制台
const addLog = (message: string) => {
  consoleLogs.value.unshift(`[${new Date().toLocaleTimeString()}] ${message}`);
  if (consoleLogs.value.length > 10) {
    consoleLogs.value.pop();
  }
};

// 事件处理函数
const handleLikeUpdate = (isLiked: boolean) => {
  testPost.value.isLiked = isLiked;
  addLog(`点赞状态更新: ${isLiked ? '已点赞' : '取消点赞'}`);
};

const handleLikeCountUpdate = (count: number) => {
  testPost.value.likes = count;
  addLog(`点赞数更新: ${count}`);
};

const handleFavoriteUpdate = (isFavorited: boolean) => {
  testPost.value.isFavorited = isFavorited;
  addLog(`收藏状态更新: ${isFavorited ? '已收藏' : '取消收藏'}`);
};

const handleFavoriteCountUpdate = (count: number) => {
  testPost.value.favorites = count;
  addLog(`收藏数更新: ${count}`);
};

const handleError = (error: unknown) => {
  addLog(`操作错误: ${error instanceof Error ? error.message : String(error)}`);
  console.error('组件操作错误:', error);
};

// 初始化时获取帖子详情
onMounted(async () => {
  try {
    addLog('正在获取帖子详情...');
    const postDetail = await getPostDetail(100072);
    addLog('帖子详情获取成功');
    
    // 更新测试数据
    testPost.value = {
      ...testPost.value,
      likes: postDetail.likes || 5,
      isLiked: (postDetail as any).isLiked ?? (postDetail as any).liked ?? false,
      isFavorited: (postDetail as any).isFavorited ?? (postDetail as any).favorited ?? false
    };
    
    addLog(`初始化完成: 点赞数=${testPost.value.likes}, 收藏数=${testPost.value.favorites}`);
  } catch (error) {
    addLog(`获取帖子详情失败: ${error instanceof Error ? error.message : String(error)}`);
    console.error('获取帖子详情失败:', error);
  }
});
</script>

<style scoped>
/* 保持原有的样式 */
body {
  font-family: 'Inter', sans-serif;
}
::-webkit-scrollbar {
  width: 8px;
}
::-webkit-scrollbar-track {
  background: #1e293b;
}
::-webkit-scrollbar-thumb {
  background: #475569;
  border-radius: 4px;
}
::-webkit-scrollbar-thumb:hover {
  background: #64748b;
}
html, body {
  overflow-x: hidden;
}
</style>