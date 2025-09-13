// src/utils/request.ts
import axios from 'axios'
import { User } from '@/modules/auth/scripts/User'

const service = axios.create({
  baseURL: '',
  timeout: 5000,
  withCredentials: true,
})

service.interceptors.request.use(
  (config) => {
    config.headers['Content-Type'] = 'application/json'

    // 自动添加认证头
    try {
      const userInstance = User.getInstance()
      const token = userInstance?.userAuth?.token

      if (token) {
        config.headers['Authorization'] = `Bearer ${token}`
      }
    } catch (error) {
    }
    return config
  },
  (error) => Promise.reject(error),
)

service.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    // 处理认证错误
    if (error.response?.status === 401) {

      try {
        // 清除登录状态
        const userInstance = User.getInstance()
        if (userInstance) {
          userInstance.logout()
        }

        // 触发登录弹窗（如果可以访问到全局方法）
        if (window.$app && window.$app.toggleLoginHover) {
          window.$app.toggleLoginHover(true)
        }
      } catch (clearError) {
        return;
      }
    }

    return Promise.reject(error)
  },
)

export default service
