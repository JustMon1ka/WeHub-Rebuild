// src/services/api.ts
// 使用相对路径，通过 Vite 代理转发到后端
const API_BASE_URL = ''

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
      console.log('API调用 - 发送数据:', data)

      // 发送后端支持的字段
      const backendData = {
        name: data.name,
        description: data.description,
        categories: data.categories || '通用', // 确保有默认值
      }

      const response = await fetch(`${API_BASE_URL}/api/circles`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(backendData),
        credentials: 'include',
      })

      console.log('HTTP状态:', response.status)

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
      console.error('API调用失败:', error)
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

      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}/join`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
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

      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}/membership`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
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
  static async checkMembership(circleId: number): Promise<boolean> {
    try {
      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}/members`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      })

      if (response.ok) {
        const result = await response.json()
        console.log('成员列表响应:', result)

        const currentUserId = 2 // 硬编码的用户ID，与后端保持一致

        if (result.code === 200 && result.data && Array.isArray(result.data)) {
          return result.data.some((member: any) => member.userId === currentUserId)
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
