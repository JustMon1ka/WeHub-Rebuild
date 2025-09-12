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
        console.log('🔐 已添加认证头:', token.substring(0, 20) + '...')
      }
    } catch (error) {
      console.warn('⚠️ 无法获取认证token:', error)
    }

    console.log('发送请求:', config.method?.toUpperCase(), config.url)
    console.log('请求数据:', config.data)
    return config
  },
  (error) => Promise.reject(error),
)

service.interceptors.response.use(
  (response) => {
    console.log('✅ 响应成功:', response.status, response.config.url)
    return response
  },
  (error) => {
    console.error('❌ 响应错误:', error.response?.status, error.config?.url)
    console.error('📋 错误详情:', error.response?.data)

    // 处理认证错误
    if (error.response?.status === 401) {
      console.error('🔐 认证失败，可能需要重新登录')

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
        console.error('❌ 清除认证状态失败:', clearError)
      }
    }

    return Promise.reject(error)
  },
)

export default service
