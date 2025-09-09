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

export function useAuthState() {
  const setUser = (user: User) => {
    currentUser.value = user;
  };

  const setToken = (token: string) => {
    authToken.value = token;
    localStorage.setItem('auth_token', token);
  };

  const clearAuth = () => {
    currentUser.value = null;
    authToken.value = null;
    localStorage.removeItem('auth_token');
  };

  const isAuthenticated = computed(() => !!authToken.value);

  return {
    currentUser,
    authToken,
    isAuthenticated,
    setUser,
    setToken,
    clearAuth
  };
}