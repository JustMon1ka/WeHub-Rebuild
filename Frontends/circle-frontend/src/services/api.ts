// src/services/api.ts
const API_BASE_URL = 'http://localhost:5080'

interface ApiResponse<T> {
  success: boolean
  data: T
  message?: string
}

interface CreateCircleResponse {
  success?: boolean
  circleId?: number
  data?: any
  message?: string
}

export class CircleAPI {
  // 获取所有圈子
  static async getCircles(name?: string) {
    try {
      const url = name
        ? `${API_BASE_URL}/api/circles?name=${encodeURIComponent(name)}`
        : `${API_BASE_URL}/api/circles`

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

  // 创建圈子
  static async createCircle(data: {
    name: string
    description: string
    category: string
    isPrivate: boolean
    maxMembers?: number
  }) {
    try {
      console.log('API调用 - 发送数据:', data)

      const response = await fetch(`${API_BASE_URL}/api/circles`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      })

      console.log('HTTP状态:', response.status)
      console.log('Content-Type:', response.headers.get('content-type'))

      // 检查响应是否为JSON
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

      // 处理成功响应
      if (isJson) {
        const result = await response.json()
        console.log('JSON响应:', result)
        return result
      } else {
        // 如果不是JSON但状态码是成功的，可能是纯文本响应
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

      // 解析响应
      let result
      try {
        result = responseText ? JSON.parse(responseText) : { success: true }
      } catch {
        result = { success: true, message: responseText }
      }

      if (!response.ok) {
        // 特殊处理：如果是已经加入的错误，返回特殊标识
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

      // 使用DELETE方法和正确的路径
      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}/membership`, {
        method: 'DELETE', // 改为DELETE
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

  // 添加成员状态检查方法
  static async checkMembership(circleId: number): Promise<boolean> {
    try {
      // 通过获取圈子成员列表来检查
      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}/members`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      })

      if (response.ok) {
        const result = await response.json()
        console.log('成员列表响应:', result)

        // 假设当前用户ID是2（从Controller代码看到的硬编码值）
        const currentUserId = 2

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
      const response = await fetch(`${API_BASE_URL}/api/circles/${circleId}`, {
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
      console.error('获取圈子详情失败:', error)
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
