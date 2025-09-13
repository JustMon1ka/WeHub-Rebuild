import { ref, computed } from 'vue';

export interface User {
  id: number;
  name: string;
  username: string;
  avatar: string;
  email: string;
}

// 简单的全局状态
const currentUser = ref<User | null>(null);
const authToken = ref<string | null>(localStorage.getItem('auth_token'));

// 初始化时从localStorage加载用户信息
const initializeUser = () => {
  try {
    const userData = localStorage.getItem('user_data');
    if (userData) {
      currentUser.value = JSON.parse(userData);
    }
  } catch (error) {
    return;
  }
};

// 初始化
initializeUser();

export function useAuthState() {
  const setUser = (user: User) => {
    currentUser.value = user;
    // 保存到localStorage
    localStorage.setItem('user_data', JSON.stringify(user));
  };

  const setToken = (token: string) => {
    authToken.value = token;
    localStorage.setItem('auth_token', token);
  };

  const clearAuth = () => {
    currentUser.value = null;
    authToken.value = null;
    localStorage.removeItem('auth_token');
    localStorage.removeItem('user_data');
  };

  const isAuthenticated = computed(() => !!authToken.value && !!currentUser.value);

  // 添加获取用户ID的便捷方法
  const getUserId = computed(() => currentUser.value?.id || null);

  // 添加检查用户状态的调试方法
  const debugAuthState = () => {
  };

  return {
    currentUser,
    authToken,
    isAuthenticated,
    getUserId, // 新增
    setUser,
    setToken,
    clearAuth,
    debugAuthState // 新增调试方法
  };
}
