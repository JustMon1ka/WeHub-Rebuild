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

// 设置baseURL
axios.defaults.baseURL = 'http://localhost:5000'

// 获取会话列表
export async function getConversationList(): Promise<conversationList> {
    const { data } = await axios.get<conversationListResponse>('/messages')
    return unwrap(data)
}

// 获取聊天记录
export async function getChatHistory(userId: number): Promise<message[]> {
    const { data } = await axios.get<BaseResp<message[]>>(`/messages/${userId}`)
    return unwrap(data)
}

// 发送消息
export async function sendMessage(params: {
    receiverId: number;
    content: string;
    type: 'text' | 'image';
}): Promise<{ messageId: number; success: boolean }> {
    const { data } = await axios.post<sendMessageResponse>('/messages', params)
    return unwrap(data)
}

// 标记消息已读
export async function markMessagesRead(otherUserId: number): Promise<{ success: boolean }> {
    const { data } = await axios.post<sendMessageResponse>(`/messages/${otherUserId}/read`)
    return unwrap(data)
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
    const { data } = await axios.get<UserDetailApiResp>(`/users/${userId}`)
    return unwrap(data)
}

