// src/services/api.ts
const API_BASE_URL = 'http://localhost:5080'

interface ApiResponse<T> {
  success: boolean
  data: T
  message?: string
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

      const response = await fetch(url, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      return await response.json()
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
    category?: string // 前端保留分类字段，但后端可能不存储
    isPrivate?: boolean
    maxMembers?: number
  }) {
    try {
      console.log('API调用 - 发送数据:', data)

      // 只发送后端支持的字段
      const backendData = {
        name: data.name,
        description: data.description,
        // 暂时不发送 category, isPrivate, maxMembers 因为后端模型不支持
      }

      const response = await fetch(`${API_BASE_URL}/api/circles`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(backendData),
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

  // 加入圈子
  static async joinCircle(circleId: number): Promise<any> {
    try {
      console.log('加入社区 ID:', circleId)

      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}/join`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
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
          return { success: true, alreadyMember: true, message: '您已经是该社区的成员了' }
        }

        if (response.status === 400 && result.msg && result.msg.includes('已经申请过')) {
          return { success: true, alreadyMember: true, message: '您已经申请过了' }
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
          return { success: true, notMember: true, message: '您不是该社区的成员' }
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
      console.log('请求URL:', `${API_BASE_URL}/api/circles/${circleId}`)

      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
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
}
