// src/utils/userHelper.ts
import { User } from '@/modules/auth/scripts/User'

export class UserHelper {
  // 检查是否已登录
  static isLoggedIn(): boolean {
    return !!User.getInstance()
  }

  // 获取当前用户token
  static getToken(): string | null {
    const userInstance = User.getInstance()
    return userInstance?.userAuth?.token || null
  }

  // 获取当前用户ID
  static getUserId(): string | null {
    const userInstance = User.getInstance()
    return userInstance?.userAuth?.userId || null
  }

  // 获取当前用户名
  static getUsername(): string | null {
    const userInstance = User.getInstance()
    return userInstance?.userInfo?.username || null
  }

  // 获取当前用户邮箱
  static getEmail(): string | null {
    const userInstance = User.getInstance()
    return userInstance?.userInfo?.email || null
  }

  // 获取完整用户信息
  static getUserInfo() {
    const userInstance = User.getInstance()
    if (!userInstance) {
      return {
        isLoggedIn: false,
        userId: null,
        username: null,
        email: null,
        hasToken: false,
      }
    }

    return {
      isLoggedIn: true,
      userId: userInstance.userAuth?.userId || null,
      username: userInstance.userInfo?.username || null,
      email: userInstance.userInfo?.email || null,
      hasToken: !!userInstance.userAuth?.token,
    }
  }

  // 强制刷新用户信息
  static async refreshUserInfo(): Promise<void> {
    const userInstance = User.getInstance()
    if (userInstance && userInstance.reloadUserInfo) {
      await userInstance.reloadUserInfo()
    }
  }
}
