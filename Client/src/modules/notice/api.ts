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
import { User } from '@/modules/auth/public.ts'
import { GATEWAY } from '@/modules/core/public.ts'

// 创建独立的axios实例，避免全局配置冲突
const apiClient = axios.create({
    baseURL: GATEWAY,
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


// 获取未读通知数量
export async function getUnreadNoticeCount(): Promise<unreadNoticeCount> {
    const { data } = await apiClient.get<unreadNoticeCount>('/api/notifications')
    return data
}

// 标记通知已读
export async function markNotificationsRead(type: 'comment' | 'reply' | 'like' | 'repost' | 'at'): Promise<markReadResponse> {
    const { data } = await apiClient.post<markReadResponse>('/api/notifications/read', { type })
    return data
}

export async function getLikeNotices(params?: { page?: number; pageSize?: number }): Promise<likeNoticeResponse> {
    const { page = 1, pageSize = 20 } = params ?? {}
    const { data } = await apiClient.get<likeNoticeResponse>('/api/notifications/likes', {
        params: { page, pageSize },
    })
    return data
}

export async function getCommentNotices(params?: { page?: number; pageSize?: number; unreadOnly?: boolean }): Promise<commentNoticeResponse> {
    const { page = 1, pageSize = 20, unreadOnly = false } = params ?? {}
    const { data } = await apiClient.get<commentNoticeResponse>('/api/notifications/comments', {
        params: { page, pageSize, unreadOnly },
    })
    return data
}
export async function getReplyNotices(params?: { page?: number; pageSize?: number; unreadOnly?: boolean }): Promise<replyNoticeResponse> {
    const { page = 1, pageSize = 20, unreadOnly = false } = params ?? {}
    const { data } = await apiClient.get<replyNoticeResponse>('/api/notifications/replies', {
        params: { page, pageSize, unreadOnly },
    })
    return data
}

export async function getRepostNotices(params?: { page?: number; pageSize?: number; unreadOnly?: boolean }): Promise<repostNoticeResponse> {
    const { page = 1, pageSize = 20, unreadOnly = false } = params ?? {}
    const { data } = await apiClient.get<repostNoticeResponse>('/api/notifications/reposts', {
        params: { page, pageSize, unreadOnly },
    })
    return data
}


export async function getAtNotices(params?: { page?: number; pageSize?: number; unreadOnly?: boolean }): Promise<atNoticeResponse> {
    const { page = 1, pageSize = 20, unreadOnly = false } = params ?? {}
    const { data } = await apiClient.get<atNoticeResponse>('/api/notifications/mentions', {
        params: { page, pageSize, unreadOnly },
    })
    return data
}

// 根据帖子ID获取帖子详细信息
export async function getPostDetail(postId: number): Promise<postDetailResponse> {
    const { data } = await apiClient.get<postDetailResponse>(`/posts/${postId}`)
    return data
}

// 根据评论ID获取评论详细信息
export async function getCommentDetail(commentId: number): Promise<commentDetailResponse> {
    const { data } = await apiClient.get<commentDetailResponse>(`/comments/${commentId}`)
    return data
}

// 获取指定目标的点赞者用户ID列表
export async function getLikersByTarget(params: getLikersByTargetParams): Promise<getLikersByTargetResponse> {
    const { targetType, targetId, page = 1, pageSize = 20 } = params
    const { data } = await apiClient.get<getLikersByTargetResponse>('/api/notifications/likes/target', {
        params: { targetType, targetId, page, pageSize }
    })
    return data
}


