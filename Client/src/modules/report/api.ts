import axios from 'axios'
import { User } from '@/modules/auth/public.ts'

// 创建axios实例
const apiClient = axios.create({
    baseURL: 'http://localhost:5000'
})

// 添加认证拦截器
apiClient.interceptors.request.use((config) => {
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
            console.error('[ReportAPI] 认证失败，可能需要重新登录')
        }
        return Promise.reject(error)
    }
)

// 帖子信息接口
export interface PostInfo {
    postId: number
    userId: number
    title: string
    content: string
    tags: string[]
    createdAt: string
    views: number
    likes: number
    circleId: number
}

// API响应接口
export interface PostApiResponse {
    code: number
    msg: string
    data: PostInfo
}

// 获取帖子详细信息
export async function getPostInfo(postId: number): Promise<PostInfo> {
    try {
        console.log('[ReportAPI] 获取帖子信息，帖子ID:', postId)
        const { data } = await apiClient.get<PostApiResponse>(`/api/posts/${postId}`)
        console.log('[ReportAPI] 帖子信息响应:', data)

        if (data.code === 200 && data.data) {
            return data.data
        } else {
            throw new Error(data.msg || '获取帖子信息失败')
        }
    } catch (error: any) {
        console.error('[ReportAPI] 获取帖子信息失败:', error)

        // 返回默认数据
        return {
            postId: postId,
            userId: 0,
            title: `帖子${postId}`,
            content: '内容不可用',
            tags: [],
            createdAt: new Date().toISOString(),
            views: 0,
            likes: 0,
            circleId: 0
        }
    }
}

// 用户信息接口（用于获取作者信息）
export interface UserInfo {
    userId: number
    username: string
    nickname?: string
    avatarUrl?: string
}

// 用户信息API响应
export interface UserApiResponse {
    code: number
    msg: string
    data: UserInfo
}

// 获取用户信息（使用UserDataService）
const userDataApiClient = axios.create({
    baseURL: 'http://localhost:5002'
})

// 为UserDataService添加认证拦截器
userDataApiClient.interceptors.request.use((config) => {
    const user = User.getInstance()
    if (user?.userAuth?.token) {
        config.headers.Authorization = `Bearer ${user.userAuth.token}`
    }
    return config
})

// 获取用户详细信息
export async function getUserInfo(userId: number): Promise<UserInfo> {
    try {
        console.log('[ReportAPI] 获取用户信息，用户ID:', userId)
        const { data } = await userDataApiClient.get<UserApiResponse>(`/api/user_data/${userId}`)
        console.log('[ReportAPI] 用户信息响应:', data)

        if (data.code === 200 && data.data) {
            return data.data
        } else {
            throw new Error(data.msg || '获取用户信息失败')
        }
    } catch (error: any) {
        console.error('[ReportAPI] 获取用户信息失败:', error)

        // 返回默认数据
        return {
            userId: userId,
            username: `用户${userId}`,
            nickname: `用户${userId}`,
            avatarUrl: 'https://placehold.co/100x100/facc15/78350f?text=U'
        }
    }
}

