/**
 * 用户相关工具函数
 */

import type { user } from '../types'
import { getUserDetail } from '../api'
import { mockUsers } from './data'

/**
 * 获取默认头像URL
 * @returns 默认头像URL
 */
export function getDefaultAvatar(): string {
    return 'https://placehold.co/100x100/facc15/78350f?text=U'
}

/**
 * 用户信息缓存
 */
class UserCache {
    private cache = new Map<number, user>()

    /**
     * 获取缓存的用户信息
     * @param userId 用户ID
     * @returns 用户信息或undefined
     */
    get(userId: number): user | undefined {
        return this.cache.get(userId)
    }

    /**
     * 设置用户信息到缓存
     * @param userId 用户ID
     * @param userInfo 用户信息
     */
    set(userId: number, userInfo: user): void {
        this.cache.set(userId, userInfo)
    }

    /**
     * 清除缓存
     */
    clear(): void {
        this.cache.clear()
    }

    /**
     * 获取缓存大小
     * @returns 缓存中的用户数量
     */
    size(): number {
        return this.cache.size
    }
}

// 导出单例实例
export const userCache = new UserCache()

/**
 * 确保用户信息存在，优先从缓存获取，否则从API获取
 * @param userId 用户ID
 * @returns Promise<user> 用户信息
 */
export async function ensureUser(userId: number): Promise<user> {
    const cached = userCache.get(userId)
    if (cached) return cached

    // 首先检查是否是模拟用户
    const mockUser = mockUsers.find(u => u.id === userId)
    if (mockUser) {
        const userInfo: user = {
            id: mockUser.id,
            nickname: mockUser.nickname,
            avatar: mockUser.avatar,
            url: `/user/${mockUser.id}`,
        }
        userCache.set(userId, userInfo)
        return userInfo
    }

    try {
        const detail = await getUserDetail(userId)
        const u: user = {
            id: detail.userId,
            nickname: detail.nickname || detail.username,
            avatar: detail.avatar || getDefaultAvatar(),
            url: `/user/${detail.userId}`,
        }
        userCache.set(userId, u)
        return u
    } catch (error) {
        console.warn(`获取用户${userId}信息失败，使用默认信息:`, error)
        // 返回默认用户信息
        const defaultUser: user = {
            id: userId,
            nickname: '用户' + userId,
            avatar: getDefaultAvatar(),
            url: `/user/${userId}`,
        }
        userCache.set(userId, defaultUser)
        return defaultUser
    }
}

/**
 * 批量确保用户信息存在
 * @param userIds 用户ID数组
 * @returns Promise<user[]> 用户信息数组
 */
export async function ensureUsers(userIds: number[]): Promise<user[]> {
    const uniqueIds = [...new Set(userIds)]
    return Promise.all(uniqueIds.map(id => ensureUser(id)))
}

/**
 * 创建用户信息对象
 * @param userId 用户ID
 * @param nickname 昵称
 * @param avatar 头像URL
 * @returns user对象
 */
export function createUserInfo(
    userId: number,
    nickname: string,
    avatar?: string
): user {
    return {
        id: userId,
        nickname,
        avatar: avatar || getDefaultAvatar(),
        url: `/user/${userId}`,
    }
}

/**
 * 获取模拟用户信息（用于测试）
 * @param userId 用户ID
 * @returns 模拟用户信息或null
 */
export function getMockUserInfo(userId: number): user | null {
    const mockUser = mockUsers.find(u => u.id === userId)
    if (!mockUser) return null

    return {
        id: mockUser.id,
        nickname: mockUser.nickname,
        avatar: mockUser.avatar,
        url: `/user/${mockUser.id}`,
    }
}
