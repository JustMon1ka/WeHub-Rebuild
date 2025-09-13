import axios from 'axios'
import type {
    unreadNoticeCount,
    likeNoticeResponse,
    commentNoticeResponse,
    replyNoticeResponse,
    repostNoticeResponse,
    atNoticeResponse,
    markReadResponse,
    postDetailResponse,
    commentDetailResponse,
    getLikersByTargetParams,
    getLikersByTargetResponse
} from './types'
import { unwrap, type BaseResp } from './types'
import { User } from '@/modules/auth/public.ts'
import { GATEWAY } from '@/modules/core/public.ts'
import { handleApiError } from './utils/errorHandler'

// 创建独立的axios实例，避免全局配置冲突
const apiClient = axios.create({
    baseURL: 'http://localhost:5000'
})

// 创建UserDataService的axios实例
const userDataApiClient = axios.create({
    baseURL: 'http://localhost:5002'
})

// 获取当前用户ID
function getCurrentUserId(): string | null {
    const user = User.getInstance()
    return user?.userAuth?.userId || null
}

// 添加认证拦截器
apiClient.interceptors.request.use((config) => {
    const user = User.getInstance()
    if (user?.userAuth?.token) {
        config.headers.Authorization = `Bearer ${user.userAuth.token}`
    }
    return config
})

// 为UserDataService添加认证拦截器
userDataApiClient.interceptors.request.use((config) => {
    const user = User.getInstance()
    if (user?.userAuth?.token) {
        config.headers.Authorization = `Bearer ${user.userAuth.token}`
    }
    return config
})

// 添加响应拦截器处理错误
apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            console.error('[NoticeAPI] 认证失败，可能需要重新登录')
        }
        return Promise.reject(error)
    }
)

// 为UserDataService添加响应拦截器
userDataApiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            console.error('[UserDataAPI] 认证失败，可能需要重新登录')
        }
        return Promise.reject(error)
    }
)


// 获取未读通知数量

export async function getUnreadNoticeCount(): Promise<unreadNoticeCount> {
    try {
        const { data } = await apiClient.get<unreadNoticeCount>('/api/notifications')
        console.log('[通知数据] 未读通知数量:', data)
        return data
    } catch (error) {
        console.error('[NoticeAPI] 获取未读通知数量失败:', error)
        throw error
    }
}

// 标记通知已读
export async function markNotificationsRead(type: 'comment' | 'reply' | 'like' | 'repost' | 'at'): Promise<markReadResponse> {
    const { data } = await apiClient.post<markReadResponse>('/api/notifications/read', { type })
    return data
}

export async function getLikeNotices(params?: { page?: number; pageSize?: number }): Promise<likeNoticeResponse> {
    try {
        const { page = 1, pageSize = 20 } = params ?? {}
        const { data } = await apiClient.get<likeNoticeResponse>('/api/notifications/likes', {
            params: { page, pageSize },
        })
        console.log('[通知数据] 点赞通知:', data)
        return data
    } catch (error) {
        console.error('[NoticeAPI] 获取点赞通知失败:', error)
        throw error
    }
}

export async function getCommentNotices(params?: { page?: number; pageSize?: number; unreadOnly?: boolean }): Promise<commentNoticeResponse> {
    try {
        const { page = 1, pageSize = 20, unreadOnly = false } = params ?? {}
        const { data } = await apiClient.get<commentNoticeResponse>('/api/notifications/comments', {
            params: { page, pageSize, unreadOnly },
        })
        console.log('[通知数据] 评论通知:', data)
        return data
    } catch (error) {
        console.error('[NoticeAPI] 获取评论通知失败:', error)
        throw error
    }
}
export async function getReplyNotices(params?: { page?: number; pageSize?: number; unreadOnly?: boolean }): Promise<replyNoticeResponse> {
    try {
        const { page = 1, pageSize = 20, unreadOnly = false } = params ?? {}
        const { data } = await apiClient.get<replyNoticeResponse>('/api/notifications/replies', {
            params: { page, pageSize, unreadOnly },
        })
        console.log('[通知数据] 回复通知:', data)
        return data
    } catch (error) {
        console.error('[NoticeAPI] 获取回复通知失败:', error)
        throw error
    }
}

export async function getRepostNotices(params?: { page?: number; pageSize?: number; unreadOnly?: boolean }): Promise<repostNoticeResponse> {
    try {
        const { page = 1, pageSize = 20, unreadOnly = false } = params ?? {}
        const { data } = await apiClient.get<repostNoticeResponse>('/api/notifications/reposts', {
            params: { page, pageSize, unreadOnly },
        })
        console.log('[通知数据] 转发通知:', data)
        return data
    } catch (error) {
        console.error('[NoticeAPI] 获取转发通知失败:', error)
        throw error
    }
}


export async function getAtNotices(params?: { page?: number; pageSize?: number; unreadOnly?: boolean }): Promise<atNoticeResponse> {
    try {
        const { page = 1, pageSize = 20, unreadOnly = false } = params ?? {}
        const { data } = await apiClient.get<atNoticeResponse>('/api/notifications/mentions', {
            params: { page, pageSize, unreadOnly },
        })
        console.log('[通知数据] @通知:', data)
        return data
    } catch (error) {
        console.error('[NoticeAPI] 获取@通知失败:', error)
        throw error
    }
}

// 根据帖子ID获取帖子详细信息
export async function getPostDetail(postId: number): Promise<postDetailResponse> {
    try {
        const { data } = await apiClient.get<postDetailResponse>(`/api/posts/${postId}`)
        console.log('[帖子数据] 帖子详情:', data)
        return data
    } catch (error: any) {
        console.error('[NoticeAPI] 获取帖子详情失败:', error)

        // 使用统一的错误处理
        const fallbackData = handleApiError(error, 'post', postId, 'getPostDetail')
        if (fallbackData) {
            const defaultPostDetail: postDetailResponse = {
                code: 200,
                msg: '使用降级数据',
                data: fallbackData
            }
            return defaultPostDetail
        }

        throw error
    }
}

// 根据评论ID获取评论详细信息
export async function getCommentDetail(commentId: number): Promise<commentDetailResponse> {
    try {
        const { data } = await apiClient.get<commentDetailResponse>(`/api/posts/comment?ids=${commentId}&type=Comment`)
        console.log('[帖子数据] 评论详情:', data)
        return data
    } catch (error: any) {
        console.error('[NoticeAPI] 获取评论详情失败:', error)

        // 使用统一的错误处理
        const fallbackData = handleApiError(error, 'comment', commentId, 'getCommentDetail')
        if (fallbackData) {
            const defaultCommentDetail: commentDetailResponse = {
                code: 200,
                msg: '使用降级数据',
                data: fallbackData
            }
            return defaultCommentDetail
        }

        throw error
    }
}

// 获取指定目标的点赞者用户ID列表
export async function getLikersByTarget(params: getLikersByTargetParams): Promise<getLikersByTargetResponse> {
    try {
        const { targetType, targetId, page = 1, pageSize = 20 } = params
        const { data } = await apiClient.get<getLikersByTargetResponse>('/api/notifications/likes/target', {
            params: { targetType, targetId, page, pageSize }
        })
        console.log('[点赞详情数据] 点赞者列表:', data)
        return data
    } catch (error) {
        console.error('[NoticeAPI] 获取点赞者列表失败:', error)
        throw error
    }
}

// ==================== UserDataService接口 ====================

// 用户详细信息接口类型定义
export interface UserInfoResponse {
    userId: number
    username: string
    email: string
    phone?: string
    createdAt: string
    status: number
    avatarUrl?: string      // 头像URL
    bio?: string
    gender?: string
    birthday?: string
    location?: string
    experience: number
    level: number
    nickname?: string       // 昵称
    profileUrl?: string     // 主页URL
}

export interface UserInfoApiResponse extends BaseResp<UserInfoResponse> { }

// 获取用户详细信息（使用UserDataService）
export async function getUserInfo(userId: number): Promise<UserInfoResponse> {
    try {
        const { data } = await userDataApiClient.get<UserInfoApiResponse>(`/api/user_data/${userId}`)
        console.log('[用户数据] 用户信息:', data)
        return unwrap(data)
    } catch (error: any) {
        console.error('[UserDataAPI] 获取用户信息失败:', error)

        // 使用统一的错误处理
        const fallbackData = handleApiError(error, 'user', userId, 'getUserInfo')
        if (fallbackData) {
            const defaultUserInfo: UserInfoResponse = {
                userId: userId,
                username: `用户${userId}`,
                email: '',
                status: 0,
                experience: 0,
                level: 1,
                nickname: `用户${userId}`,
                avatarUrl: 'https://placehold.co/100x100/facc15/78350f?text=U',
                profileUrl: `#/user/${userId}`,
                ...fallbackData
            }
            return defaultUserInfo
        }

        throw error
    }
}


