import axios from 'axios'
import type {
    conversationListResponse,
    chatHistoryResponse,
    sendMessageResponse,
    conversationList,
    message,
    BaseResp
} from './types'
import { unwrap } from './types'
import { User } from '@/modules/auth/public.ts'

// 创建独立的axios实例，避免全局配置冲突
const apiClient = axios.create({
    baseURL: 'http://localhost:5000'
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
        // console.log('发送请求，token:', user.userAuth.token)
        //console.log('请求头:', config.headers.Authorization)
    } else {
        console.log('用户未登录或token不存在')
    }
    return config
})

// 获取会话列表
export async function getConversationList(): Promise<conversationList> {
    try {
        const { data } = await apiClient.get<conversationListResponse>('/api/Messages')
        return unwrap(data)
    } catch (error) {
        console.warn('MessageService未运行，返回空会话列表')
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

