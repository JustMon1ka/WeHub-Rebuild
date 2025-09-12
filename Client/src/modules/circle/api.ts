// src/services/api.ts
import request from './utils/request'
import { User } from '@/modules/auth/scripts/User'

// 直接在 api.ts 中定义帖子相关的接口
interface Post {
  postId: number
  userId: number
  circleId: number
  title: string
  content: string
  tags: string[]
  createdAt: string
  views: number
  likes: number
}

interface PostsResponse {
  code: number
  msg: string
  data: Post[]
}

interface CirclePostsResponse {
  code: number
  msg: string
  data: {
    circleId: number
    postIds: number[]
    totalCount: number
  }
}

// 使用相对路径，通过 Vite 代理转发到后端
const API_BASE_URL = ''

// 然后添加这些函数（替换原来的）
export const getCirclePosts = async (circleId: number): Promise<CirclePostsResponse> => {
  const response = await request.get(`/api/Circles/${circleId}/posts`)
  return response.data
}

export const getPostsByIds = async (postIds: number[]): Promise<PostsResponse> => {
  const idsParam = postIds.join(',')
  const response = await request.get(`/api/posts?ids=${idsParam}`)
  return response.data
}

export const getPostById = async (postId: number): Promise<PostsResponse> => {
  const response = await request.get(`/api/posts?ids=${postId}`)
  return response.data
}

// 导出类型供其他文件使用
export type { Post }

interface ApiResponse<T> {
  success: boolean
  data: T
  message?: string
}

export const getSpuPage = async (params: string) => {
  const url = `${API_BASE_URL}/api/files/proxy?u=${params}`
  const response = await fetch(url, {
    method: 'GET',
    headers: {
      'Content-Type': 'image/png, image/jpeg, image/webp, image/gif, image/svg+xml',
      'Cache-Control': 'public,max-age=86400',
    },
    credentials: 'include',
  })
  const blob = await response.blob()
  return URL.createObjectURL(blob)
}

// 在 api.ts 中添加这个函数
export const getProxiedImageUrl = async (originalUrl: string): Promise<string> => {
  if (!originalUrl) return ''

  try {
    // 使用你提供的代理接口
    const encodedUrl = encodeURIComponent(originalUrl)
    const proxyUrl = `${API_BASE_URL}/api/files/proxy?u=${encodedUrl}`

    const response = await fetch(proxyUrl, {
      method: 'GET',
      credentials: 'include',
    })

    if (!response.ok) {
      console.error('代理请求失败:', response.status)
      return originalUrl // 如果代理失败，返回原始URL
    }

    const blob = await response.blob()
    return URL.createObjectURL(blob)
  } catch (error) {
    console.error('获取代理图片失败:', error)
    return originalUrl
  }
}

export class CircleAPI {
  // 获取所有圈子
  static async getCircles(name?: string, joinedBy?: number) {
    try {
      let url = `${API_BASE_URL}/api/circles`
      const params = new URLSearchParams()

      if (name) {
        params.append('name', name)
      }
      if (joinedBy) {
        params.append('joinedBy', joinedBy.toString())
      }

      if (params.toString()) {
        url += `?${params.toString()}`
      }

      console.log('发送请求到:', url)

      const response = await fetch(url, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        // 如果需要发送 cookies 可以添加这个
        credentials: 'include',
      })

      console.log('响应状态:', response.status)

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const result = await response.json()
      console.log('响应数据:', result)
      return result
    } catch (error) {
      console.error('获取圈子列表失败:', error)
      throw error
    }
  }

  // 获取用户已加入的圈子
  static async getUserJoinedCircles(userId: number = 2) {
    try {
      return await this.getCircles(undefined, userId)
    } catch (error) {
      console.error('获取用户已加入圈子失败:', error)
      throw error
    }
  }

  // 创建圈子
  static async createCircle(data: {
    name: string
    description: string
    categories?: string // 保持与后端一致
    isPrivate?: boolean
    maxMembers?: number
  }) {
    try {
      console.log('=== 创建社区 ===')
      console.log('API调用 - 发送数据:', data)

      // 🔧 获取认证token
      const userInstance = User.getInstance()
      const token = userInstance?.userAuth?.token

      if (!token) {
        throw new Error('用户未认证，请先登录')
      }

      // 发送后端支持的字段
      const backendData = {
        name: data.name,
        role: 1,
        status: 1,
        description: data.description,
        categories: data.categories || '通用', // 确保有默认值
      }

      console.log('🔐 使用认证Token:', token.substring(0, 20) + '...')
      console.log('👤 当前用户ID:', userInstance?.userAuth?.userId)

      const response = await fetch(`${API_BASE_URL}/api/circles`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`, // 🔧 添加这一行
        },
        body: JSON.stringify(backendData),
        credentials: 'include',
      })

      console.log('创建社区响应状态:', response.status)

      // 🔧 检查401错误
      if (response.status === 401) {
        console.error('🔐 认证失败，清除登录状态')
        if (userInstance) {
          userInstance.logout()
        }
        throw new Error('登录已过期，请重新登录')
      }

      const contentType = response.headers.get('content-type')
      const isJson = contentType && contentType.includes('application/json')

      if (!response.ok) {
        let errorMessage = `HTTP error! status: ${response.status}`

        try {
          if (isJson) {
            const errorData = await response.json()
            errorMessage += `, message: ${errorData.message || JSON.stringify(errorData)}`
          } else {
            const errorText = await response.text()
            errorMessage += `, text: ${errorText}`
          }
        } catch (e) {
          console.log('无法读取错误响应体')
        }

        throw new Error(errorMessage)
      }

      if (isJson) {
        const result = await response.json()
        console.log('JSON响应:', result)
        return result
      } else {
        const textResult = await response.text()
        console.log('文本响应:', textResult)
        return { success: true, message: textResult, rawResponse: textResult }
      }
    } catch (error) {
      console.error('❌ API调用失败:', error)
      throw error
    }
  }

  // 获取所有分类列表
  static async getCategories(): Promise<string[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/api/circles/categories`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const result = await response.json()
      const apiCategories = result.data || []

      // 常用分类列表
      const commonCategories = [
        '技术',
        '生活',
        '娱乐',
        '教育',
        '商业',
        '体育',
        '健康',
        '旅游',
        '美食',
        '音乐',
        '电影',
        '读书',
        '游戏',
        '其他',
      ]

      // 合并并去重，保持API返回的分类在前面
      const allCategories = [...new Set([...apiCategories, ...commonCategories])]

      console.log('合并后的分类列表:', allCategories)
      return allCategories
    } catch (error) {
      console.error('获取分类列表失败，使用默认分类:', error)
      // 如果接口调用失败，返回默认分类
      return [
        '技术',
        '生活',
        '娱乐',
        '教育',
        '商业',
        '体育',
        '健康',
        '旅游',
        '美食',
        '音乐',
        '电影',
        '读书',
        '游戏',
        '其他',
      ]
    }
  }

  // 加入圈子
  static async joinCircle(circleId: number): Promise<any> {
    try {
      console.log('加入社区 ID:', circleId)

      // 🔧 获取认证token
      const userInstance = User.getInstance()
      const token = userInstance?.userAuth?.token

      if (!token) {
        throw new Error('用户未认证，请先登录')
      }

      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}/join`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`, // 🔧 添加认证头
        },
        credentials: 'include',
      })

      console.log('加入社区响应状态:', response.status)

      const responseText = await response.text()
      console.log('加入社区响应:', responseText)

      let result
      try {
        result = responseText ? JSON.parse(responseText) : { success: true }
      } catch {
        result = { success: true, message: responseText }
      }

      if (!response.ok) {
        if (response.status === 400 && result.msg && result.msg.includes('已是该圈子成员')) {
          return {
            success: true,
            alreadyMember: true,
            message: '您已经是该社区的成员了',
          }
        }

        if (response.status === 400 && result.msg && result.msg.includes('已经申请过')) {
          return {
            success: true,
            alreadyMember: true,
            message: '您已经申请过了',
          }
        }

        throw new Error(`加入失败: ${response.status} - ${responseText}`)
      }

      return result
    } catch (error) {
      console.error('加入圈子失败:', error)

      throw error
    }
  }

  // 退出圈子
  static async leaveCircle(circleId: number): Promise<any> {
    try {
      console.log('退出社区 ID:', circleId)

      // 🔧 获取认证token
      const userInstance = User.getInstance()
      const token = userInstance?.userAuth?.token

      if (!token) {
        throw new Error('用户未认证，请先登录')
      }

      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}/membership`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`, // 🔧 添加认证头
        },
        credentials: 'include',
      })

      console.log('退出社区响应状态:', response.status)
      const responseText = await response.text()
      console.log('退出社区响应:', responseText)

      let result
      try {
        result = responseText ? JSON.parse(responseText) : { success: true }
      } catch {
        result = { success: true, message: responseText }
      }

      if (!response.ok) {
        if (response.status === 400 && result.msg && result.msg.includes('不是该圈子成员')) {
          return {
            success: true,
            notMember: true,
            message: '您不是该社区的成员',
          }
        }
        throw new Error(`退出失败: ${response.status} - ${responseText}`)
      }

      return result
    } catch (error) {
      console.error('退出圈子失败:', error)
      throw error
    }
  }

  // 检查成员状态
  // 检查成员状态
  static async checkMembership(circleId: number): Promise<boolean> {
    try {
      // 🔧 获取认证token
      const userInstance = User.getInstance()
      const token = userInstance?.userAuth?.token

      if (!token) {
        console.warn('用户未认证，无法检查成员状态')
        return false
      }

      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}/members`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`, // 🔧 添加认证头
        },
        credentials: 'include',
      })

      if (response.ok) {
        const result = await response.json()
        console.log('成员列表响应:', result)

        // 🔧 使用真实的用户ID，不要硬编码
        const currentUserId = userInstance?.userAuth?.userId

        if (result.code === 200 && result.data && Array.isArray(result.data)) {
          const isMember = result.data.some((member: any) => member.userId === currentUserId)
          console.log(`用户${currentUserId}是否为社区${circleId}成员:`, isMember)
          return isMember
        }
      }

      return false
    } catch (error) {
      console.error('检查成员状态失败:', error)
      return false
    }
  }

  static async getCircleDetails(circleId: number) {
    try {
      console.log('=== API调用开始 ===')
      const url = `${API_BASE_URL}/api/circles/${circleId}`
      console.log('请求URL:', url)

      const response = await fetch(url, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      })

      console.log('HTTP状态码:', response.status)

      const responseText = await response.text()
      console.log('原始响应内容:', responseText)

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${responseText}`)
      }

      let result
      try {
        result = responseText ? JSON.parse(responseText) : null
      } catch (parseError) {
        console.error('JSON解析失败:', parseError)
        throw new Error(`服务器响应格式错误: ${responseText}`)
      }

      console.log('解析后的JSON:', result)

      if (!result) {
        throw new Error('服务器返回空响应')
      }

      if (result.code === 200) {
        console.log('=== API调用成功 ===')
        return {
          success: true,
          data: result.data,
          message: result.msg,
        }
      } else {
        throw new Error(result.msg || `服务器返回错误代码: ${result.code}`)
      }
    } catch (error) {
      console.error('=== API调用失败 ===')
      console.error('错误详情:', error)
      throw error
    }
  }

  static async getCircleMembers(circleId: number) {
    try {
      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}/members`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      return await response.json()
    } catch (error) {
      console.error('获取圈子成员失败:', error)
      throw error
    }
  }

  static async getCircleActivities(circleId: number) {
    try {
      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}/activities`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      return await response.json()
    } catch (error) {
      console.error('获取圈子活动失败:', error)
      throw error
    }
  }

  // 上传圈子头像
  static async uploadCircleAvatar(circleId: number, file: File): Promise<any> {
    try {
      console.log('=== 开始上传头像 ===')

      // 🔧 获取认证token
      const userInstance = User.getInstance()
      const token = userInstance?.userAuth?.token

      if (!token) {
        throw new Error('用户未认证，请先登录')
      }

      console.log('圈子ID:', circleId)
      console.log('文件信息:', {
        name: file.name,
        size: file.size,
        type: file.type,
      })

      const formData = new FormData()
      formData.append('file', file)

      const url = `${API_BASE_URL}/api/circles/${circleId}/avatar`
      console.log('请求URL:', url)

      const response = await fetch(url, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`, // 🔧 添加这一行
        },
        body: formData,
        credentials: 'include',
      })

      console.log('响应状态:', response.status)
      console.log('响应头:', response.headers)

      const responseText = await response.text()
      console.log('原始响应:', responseText)

      if (!response.ok) {
        throw new Error(`上传失败: HTTP ${response.status} - ${responseText}`)
      }

      let result
      try {
        result = responseText ? JSON.parse(responseText) : { success: true }
      } catch (parseError) {
        console.error('JSON解析失败:', parseError)
        throw new Error(`响应格式错误: ${responseText}`)
      }

      console.log('解析后的结果:', result)
      return result
    } catch (error) {
      console.error('=== 头像上传失败 ===')
      console.error('错误详情:', error)
      throw error
    }
  }

  // 上传圈子横幅
  // 上传圈子横幅
  static async uploadCircleBanner(circleId: number, file: File): Promise<any> {
    try {
      console.log('=== 开始上传横幅 ===')
      // 🔧 获取认证token
      const userInstance = User.getInstance()
      const token = userInstance?.userAuth?.token

      if (!token) {
        throw new Error('用户未认证，请先登录')
      }

      console.log('圈子ID:', circleId)
      console.log('文件信息:', {
        name: file.name,
        size: file.size,
        type: file.type,
      })

      const formData = new FormData()
      formData.append('file', file)

      const url = `${API_BASE_URL}/api/circles/${circleId}/banner`
      console.log('请求URL:', url)

      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}/banner`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${token}`, // 🔧 添加这一行
        },
        body: formData,
        credentials: 'include',
      })

      console.log('响应状态:', response.status)

      const responseText = await response.text()
      console.log('原始响应:', responseText)

      if (!response.ok) {
        throw new Error(`上传失败: HTTP ${response.status} - ${responseText}`)
      }

      let result
      try {
        result = responseText ? JSON.parse(responseText) : { success: true }
      } catch (parseError) {
        console.error('JSON解析失败:', parseError)
        throw new Error(`响应格式错误: ${responseText}`)
      }

      console.log('解析后的结果:', result)
      return result
    } catch (error) {
      console.error('=== 横幅上传失败 ===')
      console.error('错误详情:', error)
      throw error
    }
  }
  // 获取图片代理URL
  static getImageProxyUrl(imageUrl: string): string {
    if (!imageUrl) return ''

    // 如果已经是完整URL，直接返回
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      return imageUrl
    }

    // 根据你同学的文档，直接访问文件服务器
    if (imageUrl.startsWith('/files/')) {
      return `http://120.26.118.70:5001${imageUrl}`
    }

    // 其他情况，使用API资源路径
    return `http://120.26.118.70:5001/files/uploads/${imageUrl}`
  }
}

// FileBrowser API 类
interface FileBrowserAuthResponse {
  token: string
}

interface Activity {
  id: number
  title: string
  description: string
  startTime: string
  endTime: string
  location?: string
  maxParticipants?: number
  currentParticipants: number
  status: 'upcoming' | 'ongoing' | 'completed' | 'cancelled'
  createdBy: number
  circleId: number
  createdAt: string
  updatedAt: string
}

interface CreateActivityRequest {
  title: string
  description: string
  startTime: string
  endTime: string
  location?: string
  maxParticipants?: number
}

interface UpdateActivityRequest {
  title?: string
  description?: string
  startTime?: string
  endTime?: string
  location?: string
  maxParticipants?: number
}

export class FileBrowserAPI {
  private static baseURL = 'http://120.26.118.70:5001'
  private static sessionEstablished = false

  // 建立FileBrowser会话
  static async establishSession(): Promise<boolean> {
    if (this.sessionEstablished) {
      return true
    }

    try {
      console.log('=== 建立 FileBrowser 会话 ===')

      // 访问 /files 目录建立会话
      const response = await fetch(`${this.baseURL}/files/`, {
        method: 'GET',
        credentials: 'include', // 重要：包含cookies
      })

      console.log('会话建立响应状态:', response.status)

      if (response.ok || response.status === 401) {
        // 即使是401，会话也可能已经建立
        this.sessionEstablished = true
        console.log('✅ FileBrowser 会话已建立')
        return true
      }

      return false
    } catch (error) {
      console.error('建立会话失败:', error)
      return false
    }
  }

  // 获取图片URL（带会话）
  static async getSessionImageUrl(originalUrl: string): Promise<string> {
    // 先建立会话
    await this.establishSession()

    // 直接返回原始URL，现在应该可以访问了
    console.log('使用会话访问原始URL:', originalUrl)
    return originalUrl
  }
}

// 修改 CircleAPI 中的图片处理方法
export class CircleImageAPI {
  // 智能图片URL处理
  static async getOptimalImageUrl(imageUrl: string): Promise<string> {
    if (!imageUrl) {
      console.log('图片URL为空')
      return ''
    }

    console.log('=== 智能图片URL处理 ===')
    console.log('原始URL:', imageUrl)

    // 如果是 FileBrowser 预览API路径
    if (imageUrl.includes('/api/preview/')) {
      console.log('检测到 FileBrowser 预览API')

      // 优先尝试公开文件路径
      const publicUrl = FileBrowserAPI.convertToPublicUrl(imageUrl)

      try {
        // 测试公开路径是否可用
        const response = await fetch(publicUrl, { method: 'HEAD' })
        if (response.ok) {
          console.log('✅ 公开路径可用:', publicUrl)
          return publicUrl
        }
      } catch (error) {
        console.log('公开路径不可用，尝试认证路径')
      }

      // 公开路径不可用，尝试认证
      try {
        const authenticatedUrl = await FileBrowserAPI.getAuthenticatedImageUrl(imageUrl)
        console.log('✅ 使用认证URL:', authenticatedUrl)
        return authenticatedUrl
      } catch (error) {
        console.error('认证失败，返回原始URL')
        return imageUrl
      }
    }

    // 其他URL类型的处理
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      return imageUrl
    }

    if (imageUrl.startsWith('/api/resources/')) {
      const convertedPath = imageUrl.replace('/api/resources/', '/files/uploads/')
      return `http://120.26.118.70:5001${convertedPath}`
    }

    if (imageUrl.startsWith('/files/')) {
      return `http://120.26.118.70:5001${imageUrl}`
    }

    // 默认情况
    return `http://120.26.118.70:5001/files/uploads/${imageUrl}`
  }
}

// 活动相关API接口
export const activityApi = {
  // 获取圈子的所有活动
  getActivitiesByCircleId: async (circleId: number): Promise<ApiResponse<Activity[]>> => {
    const response = await request.get(`/api/Circles/${circleId}/activities`)
    return response.data
  },

  // 创建活动
  createActivity: async (
    circleId: number,
    data: CreateActivityRequest,
  ): Promise<ApiResponse<Activity>> => {
    const response = await request.post(`/api/Circles/${circleId}/activities`, data)
    return response.data
  },

  // 更新活动
  updateActivity: async (
    circleId: number,
    activityId: number,
    data: UpdateActivityRequest,
  ): Promise<ApiResponse<Activity>> => {
    const response = await request.put(`/api/Circles/${circleId}/activities/${activityId}`, data)
    return response.data
  },

  // 删除活动
  deleteActivity: async (circleId: number, activityId: number): Promise<ApiResponse<null>> => {
    const response = await request.delete(`/api/Circles/${circleId}/activities/${activityId}`)
    return response.data
  },

  // 报名参加活动
  joinActivity: async (circleId: number, activityId: number): Promise<ApiResponse<null>> => {
    try {
      console.log('=== 报名活动 ===')
      console.log('报名活动请求:', { circleId, activityId })

      // 🔧 获取认证信息
      const userInstance = User.getInstance()
      const token = userInstance?.userAuth?.token

      if (!token) {
        throw new Error('用户未认证，请先登录')
      }

      console.log('🔐 使用认证Token:', token.substring(0, 20) + '...')
      console.log('👤 当前用户ID:', userInstance?.userAuth?.userId)

      const response = await fetch(
        `${API_BASE_URL}/api/activities/${activityId}/participants/join`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`, // 🔧 添加认证头
          },
          body: JSON.stringify({}),
          credentials: 'include',
        },
      )

      console.log('报名响应状态:', response.status)

      // 🔧 处理401认证错误
      if (response.status === 401) {
        console.error('🔐 认证失败，清除登录状态')
        if (userInstance) {
          userInstance.logout()
        }
        throw new Error('登录已过期，请重新登录')
      }

      const responseText = await response.text()
      console.log('报名响应内容:', responseText)

      let result
      try {
        result = responseText ? JSON.parse(responseText) : { success: true }
      } catch {
        result = { success: true, message: responseText }
      }

      if (!response.ok) {
        // 处理已报名活动的情况
        if (response.status === 400 && responseText.includes('已参加')) {
          return {
            success: true,
            data: null,
            alreadyJoined: true,
            message: '您已经报名了该活动',
          }
        }
        throw new Error(`报名失败: ${responseText}`)
      }

      return result
    } catch (error: any) {
      console.error('报名活动API调用失败:', {
        circleId,
        activityId,
        error: error.message,
      })

      // 处理已报名活动的情况
      if (error.message.includes('已参加')) {
        return {
          success: true,
          data: null,
          alreadyJoined: true,
          message: '您已经报名了该活动',
        }
      }

      throw error
    }
  },

  // 提交参与心得（完成活动）
  submitParticipationNote: async (
    circleId: number,
    activityId: number,
    data: {
      content: string
      contact?: string
    },
  ): Promise<any> => {
    try {
      console.log('=== 开始一键完成活动流程 ===')

      // 🔧 获取认证信息
      const userInstance = User.getInstance()
      const token = userInstance?.userAuth?.token

      if (!token) {
        throw new Error('用户未认证，请先登录')
      }

      console.log('🔐 使用认证Token:', token.substring(0, 20) + '...')
      console.log('👤 当前用户ID:', userInstance?.userAuth?.userId)

      // 🔧 统一的认证headers
      const authHeaders = {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      }

      // 第一步：报名参加活动
      console.log('第一步：报名参加活动')
      try {
        const joinUrl = `${API_BASE_URL}/api/activities/${activityId}/participants/join`
        const joinResponse = await fetch(joinUrl, {
          method: 'POST',
          headers: authHeaders,
          body: JSON.stringify({}),
          credentials: 'include',
        })

        console.log('报名响应状态:', joinResponse.status)

        // 🔧 处理401认证错误
        if (joinResponse.status === 401) {
          console.error('🔐 认证失败，清除登录状态')
          if (userInstance) {
            userInstance.logout()
          }
          throw new Error('登录已过期，请重新登录')
        }

        const joinResponseText = await joinResponse.text()

        console.log('报名响应内容:', joinResponseText)

        if (
          !joinResponse.ok &&
          !joinResponseText.includes('已参加') &&
          !joinResponseText.includes('已经申请')
        ) {
          throw new Error(`报名失败: ${joinResponseText}`)
        }
        console.log('✅ 报名成功')
      } catch (joinError: any) {
        if (
          !joinError.message.includes('已参加') &&
          !joinError.message.includes('已经申请') &&
          !joinError.message.includes('登录已过期')
        ) {
          throw new Error(`报名活动失败: ${joinError.message}`)
        }
        if (joinError.message.includes('登录已过期')) {
          throw joinError // 直接抛出登录过期错误
        }
        console.log('✅ 用户已报名，继续下一步')
      }

      // 第二步：提交心得完成活动
      console.log('第二步：提交心得完成活动')
      const completeUrl = `${API_BASE_URL}/api/activities/${activityId}/participants/complete`
      const completeResponse = await fetch(completeUrl, {
        method: 'PUT',
        headers: authHeaders, // 🔧 使用认证headers
        body: JSON.stringify(data),
        credentials: 'include',
      })

      console.log('完成活动响应状态:', completeResponse.status)
      // 🔧 处理401认证错误
      if (completeResponse.status === 401) {
        console.error('🔐 认证失败，清除登录状态')
        if (userInstance) {
          userInstance.logout()
        }
        throw new Error('登录已过期，请重新登录')
      }
      const completeResponseText = await completeResponse.text()
      console.log('完成活动响应内容:', completeResponseText)

      if (!completeResponse.ok) {
        throw new Error(`完成活动失败: ${completeResponseText}`)
      }

      console.log('✅ 活动完成成功')

      // 第三步：自动领取奖励
      console.log('第三步：自动领取奖励')
      try {
        const rewardUrl = `${API_BASE_URL}/api/activities/${activityId}/participants/claim-reward`
        const rewardResponse = await fetch(rewardUrl, {
          method: 'POST',
          headers: authHeaders, // 🔧 使用认证headers
          body: JSON.stringify({}),
          credentials: 'include',
        })

        console.log('领取奖励响应状态:', rewardResponse.status)
        if (rewardResponse.status === 401) {
          console.error('🔐 奖励领取认证失败')
          return {
            success: true,
            message: '心得提交成功，活动已完成！（请手动领取奖励）',
            completed: true,
            rewardClaimed: false,
            rewardError: '认证失败',
          }
        }
        const rewardResponseText = await rewardResponse.text()
        console.log('领取奖励响应内容:', rewardResponseText)

        if (rewardResponse.ok) {
          console.log('✅ 奖励领取成功')
          return {
            success: true,
            message: '心得提交成功，活动完成，奖励已到账！',
            completed: true,
            rewardClaimed: true,
            data: rewardResponseText ? JSON.parse(rewardResponseText) : null,
          }
        } else {
          // 奖励领取失败，但活动已完成
          console.log('⚠️ 奖励领取失败，但活动已完成')
          return {
            success: true,
            message: '心得提交成功，活动已完成！',
            completed: true,
            rewardClaimed: false,
            rewardError: rewardResponseText,
          }
        }
      } catch (rewardError: any) {
        console.log('⚠️ 奖励领取出错:', rewardError)
        // 即使奖励领取失败，活动也算完成了
        return {
          success: true,
          message: '心得提交成功，活动已完成！',
          completed: true,
          rewardClaimed: false,
          rewardError: rewardError.message,
        }
      }
    } catch (error: any) {
      console.error('=== 活动流程失败 ===', error)
      throw error
    }
  },

  // 获取用户参与状态
  getUserParticipationStatus: async (
    circleId: number,
    activityId: number,
  ): Promise<ApiResponse<ActivityParticipant | null>> => {
    try {
      // 🔧 获取当前登录用户的真实ID
      const userInstance = User.getInstance()
      const currentUserId = userInstance?.userAuth?.userId // 这应该是100247

      console.log('=== 获取用户参与状态 ===')
      console.log('当前用户ID:', currentUserId)
      console.log('活动ID:', activityId)

      if (!currentUserId) {
        throw new Error('用户未登录')
      }

      const response = await request.get(
        `/api/activity-participants/activity/${activityId}/user/${currentUserId}`,
      )
      return response.data
    } catch (error: any) {
      if (error.response?.status === 404) {
        // 404表示用户未参与该活动
        return { success: true, data: null }
      }
      if (error.response?.status === 403) {
        // 403表示权限不足，可能是在查看其他用户的状态
        console.log('权限不足，可能在查看其他用户的活动参与状态')
        return { success: true, data: null }
      }
      console.error('获取参与状态失败:', error)
      throw error
    }
  },

  // 上传活动图片
  uploadActivityImage: async (circleId: number, activityId: number, file: File): Promise<any> => {
    try {
      console.log('=== 开始上传活动图片 ===')
      console.log('圈子ID:', circleId)
      console.log('活动ID:', activityId)
      console.log('文件信息:', {
        name: file.name,
        size: file.size,
        type: file.type,
      })

      const formData = new FormData()
      formData.append('file', file)

      const url = `${API_BASE_URL}/api/activities/${activityId}/image`
      console.log('请求URL:', url)

      const response = await fetch(url, {
        method: 'POST',
        body: formData,
        credentials: 'include',
      })

      console.log('响应状态:', response.status)

      const responseText = await response.text()
      console.log('原始响应:', responseText)

      if (!response.ok) {
        throw new Error(`上传失败: HTTP ${response.status} - ${responseText}`)
      }

      let result
      try {
        result = responseText ? JSON.parse(responseText) : { success: true }
      } catch (parseError) {
        console.error('JSON解析失败:', parseError)
        throw new Error(`响应格式错误: ${responseText}`)
      }

      console.log('解析后的结果:', result)
      return result
    } catch (error: any) {
      console.error('=== 活动图片上传失败 ===')
      console.error('错误详情:', error)
      throw error
    }
  },
}

// 在 api.ts 中添加发帖相关的API
export class PostAPI {
  private static baseURL = ''

  // 获取当前用户token
  private static getAuthToken(): string | null {
    try {
      const userInstance = User.getInstance()
      return userInstance?.userAuth?.token || null
    } catch (error) {
      console.warn('⚠️ 无法获取认证token:', error)
      return null
    }
  }

  // 获取当前用户ID
  private static getCurrentUserId(): string | null {
    try {
      const userInstance = User.getInstance()
      return userInstance?.userAuth?.userId || null
    } catch (error) {
      console.warn('⚠️ 无法获取用户ID:', error)
      return null
    }
  }

  // 发布帖子
  static async publishPost(data: {
    circleId?: number
    title: string
    content: string
    tags?: number[]
  }): Promise<any> {
    try {
      console.log('=== 发布帖子 ===')
      console.log('请求数据:', data)

      const requestData = {
        circleId: data.circleId || 0,
        title: data.title,
        content: data.content,
        tags: data.tags || [],
      }

      // 获取认证token
      const token = this.getAuthToken()
      if (!token) {
        throw new Error('用户未认证，请先登录')
      }

      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      }

      console.log('发送到后端的数据:', requestData)
      console.log('🔐 使用认证Token:', token.substring(0, 20) + '...')
      console.log('👤 当前用户ID:', this.getCurrentUserId())

      const response = await fetch(`${this.baseURL}/api/posts/publish`, {
        method: 'POST',
        headers,
        body: JSON.stringify(requestData),
        credentials: 'include',
      })

      console.log('发帖响应状态:', response.status)

      const responseText = await response.text()
      console.log('发帖响应内容:', responseText)

      if (!response.ok) {
        if (response.status === 401) {
          // 认证失败，清除本地token并提示登录
          const userInstance = User.getInstance()
          if (userInstance) {
            userInstance.logout()
          }
          throw new Error('登录已过期，请重新登录')
        }

        throw new Error(`发帖失败: HTTP ${response.status} - ${responseText}`)
      }

      let result
      try {
        result = responseText ? JSON.parse(responseText) : { success: true }
      } catch (parseError) {
        console.error('JSON解析失败:', parseError)
        throw new Error(`响应格式错误: ${responseText}`)
      }

      // 🔧 检查后端返回的业务状态码
      if (result.code && result.code !== 200) {
        console.error('❌ 后端业务错误:', result)

        // 🔧 处理数据库错误
        if (result.msg && result.msg.includes('ORA-')) {
          console.error('💾 数据库错误:', result.msg)

          if (result.msg.includes('ORA-12570')) {
            throw new Error('数据库连接失败，请稍后重试或联系管理员')
          } else if (result.msg.includes('ORA-00942')) {
            throw new Error('系统维护中，请稍后再试')
          } else {
            throw new Error('数据库操作失败，请稍后重试')
          }
        }

        // 🔧 其他业务错误
        throw new Error(result.msg || '发布失败，请重试')
      }

      console.log('发帖成功:', result)
      return result
    } catch (error) {
      console.error('=== 发帖失败 ===')
      console.error('错误详情:', error)
      throw error
    }
  }

  // 获取帖子列表（按社区ID筛选）
  static async getPostsByCircle(
    circleId: number,
    params?: {
      lastId?: number
      num?: number
    },
  ): Promise<any> {
    try {
      const queryParams = new URLSearchParams()
      if (params?.lastId) queryParams.append('lastId', params.lastId.toString())
      if (params?.num) queryParams.append('num', params.num.toString())

      const queryString = queryParams.toString()
      const url = `${this.baseURL}/api/posts/list${queryString ? `?${queryString}` : ''}`

      const response = await fetch(url, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      })

      if (!response.ok) {
        throw new Error(`获取帖子失败: HTTP ${response.status}`)
      }

      const result = await response.json()

      // 筛选指定社区的帖子
      if (result.success && result.data) {
        result.data = result.data.filter((post: any) => post.circleId === circleId)
      }

      return result
    } catch (error) {
      console.error('获取帖子失败:', error)
      throw error
    }
  }

  // 获取单个帖子详情
  static async getPostDetail(postId: number): Promise<any> {
    try {
      const response = await fetch(`${this.baseURL}/api/posts/${postId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      })

      if (!response.ok) {
        throw new Error(`获取帖子详情失败: HTTP ${response.status}`)
      }

      return await response.json()
    } catch (error) {
      console.error('获取帖子详情失败:', error)
      throw error
    }
  }

  // 删除帖子
  static async deletePost(postId: number): Promise<any> {
    try {
      const response = await fetch(`${this.baseURL}/api/posts?post_id=${postId}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      })

      if (!response.ok) {
        throw new Error(`删除帖子失败: HTTP ${response.status}`)
      }

      const responseText = await response.text()
      return responseText ? JSON.parse(responseText) : { success: true }
    } catch (error) {
      console.error('删除帖子失败:', error)
      throw error
    }
  }
}
