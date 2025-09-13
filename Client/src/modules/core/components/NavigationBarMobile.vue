<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { getUnreadMessageCount } from '@/modules/message/api'
import { User } from '@/modules/auth/public'
import { eventBus, EVENTS } from '@/modules/core/utils/eventBus'

const route = useRoute()
const unreadCount = ref(0)
let refreshInterval: number | null = null

// 计算当前活跃的路由
const isActive = (path: string) => {
  return computed(() => {
    if (path === '/notice') {
      return route.path.startsWith('/notice')
    }
    if (path === '/message') {
      return route.path.startsWith('/message')
    }
    return route.path === path
  })
}

// 获取未读消息数量
const fetchUnreadCount = async () => {
  try {
    // 检查用户是否已登录
    const user = User.getInstance()
    if (!user?.userAuth?.token) {
      unreadCount.value = 0
      return
    }

    const count = await getUnreadMessageCount()
    unreadCount.value = count
  } catch (error) {
    console.error('获取未读消息数量失败:', error)
    unreadCount.value = 0
  }
}

// 启动定时刷新
const startRefresh = () => {
  if (refreshInterval) return

  // 立即获取一次
  fetchUnreadCount()

  // 每30秒刷新一次
  refreshInterval = setInterval(fetchUnreadCount, 30000)
}

// 停止定时刷新
const stopRefresh = () => {
  if (refreshInterval) {
    clearInterval(refreshInterval)
    refreshInterval = null
  }
}

// 监听消息已读事件
const handleMessageMarkedAsRead = () => {
  // 立即刷新未读数量
  fetchUnreadCount()
}

onMounted(() => {
  startRefresh()
  // 监听消息已读事件
  eventBus.on(EVENTS.MESSAGE_MARKED_AS_READ, handleMessageMarkedAsRead)
})

onUnmounted(() => {
  stopRefresh()
  // 移除事件监听
  eventBus.off(EVENTS.MESSAGE_MARKED_AS_READ, handleMessageMarkedAsRead)
})
</script>

<template>
  <nav class="flex flex-row w-full justify-around">
    <router-link to="/" :class="['router-link', { 'router-link-active': isActive('/').value }]">
      <img src="@/modules/core/assets/home.svg" alt="Home" class="svg" />
    </router-link>
    <router-link
      to="/founding"
      :class="['router-link', { 'router-link-active': isActive('/founding').value }]"
    >
      <img src="@/modules/core/assets/discover.svg" alt="Discover" class="svg" />
    </router-link>
    <router-link
      to="/community"
      :class="['router-link', { 'router-link-active': isActive('/community').value }]"
    >
      <img src="@/modules/core/assets/community.svg" alt="Community" class="svg" />
    </router-link>
    <router-link
      to="/notice"
      :class="['router-link', { 'router-link-active': isActive('/notice').value }]"
    >
      <img src="@/modules/core/assets/notifications.svg" alt="Notifications" class="svg" />
    </router-link>
    <router-link
      to="/message"
      :class="['router-link', { 'router-link-active': isActive('/message').value }]"
    >
      <div class="relative">
        <img src="@/modules/core/assets/messages.svg" alt="Messages" class="svg" />
        <span
          v-if="unreadCount > 0"
          class="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full h-4 w-4 flex items-center justify-center min-w-[16px] text-[10px]"
        >
          {{ unreadCount > 99 ? '99+' : unreadCount }}
        </span>
      </div>
    </router-link>
    <router-link
      to="/user_page/Me"
      :class="['router-link', { 'router-link-active': isActive('/user_page/Me').value }]"
    >
      <img src="@/modules/core/assets/profile.svg" alt="Profile" class="svg" />
    </router-link>
  </nav>
</template>

<style scoped>
.svg {
  width: 1.5rem;
  height: 1.5rem;
  margin-right: 1rem;
  fill: currentColor;
}

.router-link {
  padding: 0.25rem;
  border-radius: 0.5rem;
  transition-property: background-color;
  transition-duration: 200ms;
}
.router-link:hover {
  background-color: #1e293b !important; /* 对应 bg-slate-800 */
}
.router-link-active {
  background-color: #334155 !important; /* 对应 bg-slate-700，比悬停状态更深 */
  color: #f1f5f9 !important; /* 对应 text-slate-100，更亮的文字颜色 */
}
.router-link-active:hover {
  background-color: #334155 !important; /* 保持选中状态的背景色 */
}
</style>

