import axios from 'axios'
import type {
    conversationListResponse,
    sendMessageResponse,
    conversationList,
    message,
    BaseResp
} from './types'
import { unwrap } from './types'
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

// 添加响应拦截器处理错误
apiClient.interceptors.response.use(
    (response) => {
        console.log('[MessageAPI] 响应成功:', response.status, response.config.url)
        return response
    },
    (error) => {
        console.error('[MessageAPI] 请求失败:', error.response?.status, error.response?.data, error.config?.url)
        if (error.response?.status === 401) {
            console.error('[MessageAPI] 认证失败，可能需要重新登录')
        }
        return Promise.reject(error)
    }
)

// 获取会话列表
export async function getConversationList(): Promise<conversationList> {
    try {
        const response = await apiClient.get<conversationListResponse>('/api/Messages')
        const result = unwrap(response.data)
        // 将对象转换为数组
        let conversations: conversationList = []
        if (Array.isArray(result)) {
            conversations = result
        } else if (typeof result === 'object' && result !== null) {
            // 如果是对象，转换为数组
            conversations = Object.values(result)
        }

        return conversations
    } catch (error) {
        console.error('[API] 获取会话列表失败:', error)
        return []
    }
}

// 获取聊天记录
export async function getChatHistory(userId: number): Promise<message[]> {
    try {
        const { data } = await apiClient.get<BaseResp<message[]>>(`/api/Messages/${userId}`)
        return unwrap(data)
    } catch (error) {
        console.warn('MessageService未运行，返回空聊天记录')
        return []
    }
}

// 发送消息
export async function sendMessage(params: {
    receiverId: number;
    content: string;
    type: 'text' | 'image';
}): Promise<{ messageId: number; success: boolean }> {
    try {
        const { data } = await apiClient.post<sendMessageResponse>(`/api/Messages/${params.receiverId}`, { content: params.content })
        return unwrap(data)
    } catch (error) {
        console.warn('MessageService未运行，无法发送消息')
        return { messageId: 0, success: false }
    }
}

// 标记消息已读
export async function markMessagesRead(otherUserId: number): Promise<{ success: boolean }> {
    try {
        const { data } = await apiClient.put<sendMessageResponse>(`/api/Messages/${otherUserId}/read`)
        return unwrap(data)
    } catch (error) {
        console.warn('MessageService未运行，无法标记消息已读')
        return { success: false }
    }
}

// 获取用户详情
export interface UserDetailApiResp extends BaseResp<{
    userId: number
    username: string
    email: string
    nickname: string
    avatar: string
    bio: string
    createdAt: string
    lastLoginAt: string
}> { }

export type UserDetail = {
    userId: number
    username: string
    email: string
    nickname: string
    avatar: string
    bio: string
    createdAt: string
    lastLoginAt: string
}

export async function getUserDetail(userId: number): Promise<UserDetail> {
    const { data } = await apiClient.get<UserDetailApiResp>(`/api/user_data/${userId}`)
    return unwrap(data)
}
