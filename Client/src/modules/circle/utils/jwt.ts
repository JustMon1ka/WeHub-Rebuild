// src/utils/jwt.ts

// JWT Token解析工具
export class JWTHelper {
  // 解析JWT Token获取payload
  static parseToken(token: string): any {
    try {
      const base64Url = token.split('.')[1]
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join(''),
      )
      return JSON.parse(jsonPayload)
    } catch (error) {
      console.error('❌ JWT Token解析失败:', error)
      return null
    }
  }

  // 获取用户ID (从sub字段)
  static getUserIdFromToken(token: string): string | null {
    const payload = this.parseToken(token)
    return payload?.sub || null
  }

  // 获取用户名
  static getUsernameFromToken(token: string): string | null {
    const payload = this.parseToken(token)
    return payload?.unique_name || null
  }

  // 获取邮箱
  static getEmailFromToken(token: string): string | null {
    const payload = this.parseToken(token)
    return payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || null
  }

  // 获取用户状态
  static getUserStatusFromToken(token: string): string | null {
    const payload = this.parseToken(token)
    return payload?.status || null
  }

  // 检查token是否过期
  static isTokenExpired(token: string): boolean {
    const payload = this.parseToken(token)
    if (!payload?.exp) return true

    const currentTime = Math.floor(Date.now() / 1000)
    return payload.exp < currentTime
  }

  // 获取token过期时间
  static getTokenExpiry(token: string): Date | null {
    const payload = this.parseToken(token)
    if (!payload?.exp) return null

    return new Date(payload.exp * 1000)
  }
}
