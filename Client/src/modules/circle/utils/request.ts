// utils/request.ts
import axios from 'axios'

const service = axios.create({
  baseURL: '',
  timeout: 5000,
  withCredentials: true,
})

service.interceptors.request.use(
  (config) => {
    config.headers['Content-Type'] = 'application/json'
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
    return Promise.reject(error)
  },
)

export default service
