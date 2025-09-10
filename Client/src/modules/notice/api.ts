import axios from 'axios'
import type {
    unreadNoticeCount,
    likeNoticeResponse,
    commentNoticeResponse,
    replyNoticeResponse,
    repostNoticeResponse,
    atNoticeResponse,
    likeNoticeListById,
    BaseResp,
    markReadResponse,
    postDetailResponse,
    commentDetailResponse,
    getLikersByTargetParams,
    getLikersByTargetResponse
} from './types'
import { unwrap } from './types'
import { getUserDetail } from '../message/api'

// 设置baseURL
axios.defaults.baseURL = 'http://127.0.0.1:4523/m1/7050705-6770801-default/api'


// 获取未读通知数量
export async function getUnreadNoticeCount(): Promise<unreadNoticeCount> {
    const { data } = await axios.get<unreadNoticeCount>('/notifications')
    return data
}

// 标记通知已读
export async function markNotificationsRead(type: 'comment' | 'reply' | 'like' | 'repost' | 'at'): Promise<markReadResponse> {
    const { data } = await axios.post<markReadResponse>('/notifications/read', { type })
    return data
}

export async function getLikeNotices(params?: { page?: number; pageSize?: number }): Promise<likeNoticeResponse> {
    const { page = 1, pageSize = 20 } = params ?? {}
    const { data } = await axios.get<likeNoticeResponse>('/notifications/likes', {
        params: { page, pageSize },
    })
    return data
}

export async function getCommentNotices(params?: { page?: number; pageSize?: number; unreadOnly?: boolean }): Promise<commentNoticeResponse> {
    const { page = 1, pageSize = 20, unreadOnly = false } = params ?? {}
    const { data } = await axios.get<commentNoticeResponse>('/notifications/comments', {
        params: { page, pageSize, unreadOnly },
    })
    return data
}
export async function getReplyNotices(params?: { page?: number; pageSize?: number; unreadOnly?: boolean }): Promise<replyNoticeResponse> {
    const { page = 1, pageSize = 20, unreadOnly = false } = params ?? {}
    const { data } = await axios.get<replyNoticeResponse>('/notifications/replies', {
        params: { page, pageSize, unreadOnly },
    })
    return data
}

export async function getRepostNotices(params?: { page?: number; pageSize?: number; unreadOnly?: boolean }): Promise<repostNoticeResponse> {
    const { page = 1, pageSize = 20, unreadOnly = false } = params ?? {}
    const { data } = await axios.get<repostNoticeResponse>('/notifications/reposts', {
        params: { page, pageSize, unreadOnly },
    })
    return data
}


export async function getAtNotices(params?: { page?: number; pageSize?: number; unreadOnly?: boolean }): Promise<atNoticeResponse> {
    const { page = 1, pageSize = 20, unreadOnly = false } = params ?? {}
    const { data } = await axios.get<atNoticeResponse>('/notifications/mentions', {
        params: { page, pageSize, unreadOnly },
    })
    return data
}

// 根据帖子ID获取帖子详细信息
export async function getPostDetail(postId: number): Promise<postDetailResponse> {
    const { data } = await axios.get<postDetailResponse>(`/posts/${postId}`)
    return data
}

// 根据评论ID获取评论详细信息
export async function getCommentDetail(commentId: number): Promise<commentDetailResponse> {
    const { data } = await axios.get<commentDetailResponse>(`/comments/${commentId}`)
    return data
}

// 获取指定目标的点赞者用户ID列表
export async function getLikersByTarget(params: getLikersByTargetParams): Promise<getLikersByTargetResponse> {
    const { targetType, targetId, page = 1, pageSize = 20 } = params
    const { data } = await axios.get<getLikersByTargetResponse>('/notifications/likes/target', {
        params: { targetType, targetId, page, pageSize }
    })
    return data
}


