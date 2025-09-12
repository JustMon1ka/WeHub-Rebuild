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
        console.log('[API] 开始获取会话列表...')
        const response = await apiClient.get<conversationListResponse>('/api/Messages')
        console.log('[API] 完整响应对象:', response)
        console.log('[API] 响应状态:', response.status)
        console.log('[API] 响应头:', response.headers)
        console.log('[API] 原始响应数据:', response.data)
        console.log('[API] 数据类型:', typeof response.data)
        console.log('[API] 数据是否为数组:', Array.isArray(response.data))

        const result = unwrap(response.data)
        console.log('[API] unwrap后的数据:', result)
        console.log('[API] unwrap后数据类型:', typeof result)
        console.log('[API] unwrap后是否为数组:', Array.isArray(result))

        // 将对象转换为数组
        let conversations: conversationList = []
        if (Array.isArray(result)) {
            conversations = result
        } else if (typeof result === 'object' && result !== null) {
            // 如果是对象，转换为数组
            conversations = Object.values(result)
        }

        console.log('[API] 转换后的会话列表:', conversations)
        console.log('[API] 会话列表长度:', conversations.length)

        // 详细输出每个会话
        conversations.forEach((conv, index) => {
            console.log(`[API] 会话 ${index + 1} 详情:`, {
                otherUserId: conv.otherUserId,
                unreadCount: conv.unreadCount,
                lastMessage: conv.lastMessage,
                lastMessageId: conv.lastMessage?.messageId,
                lastMessageContent: conv.lastMessage?.content,
                lastMessageTime: conv.lastMessage?.sentAt,
                lastMessageSender: conv.lastMessage?.senderId,
                lastMessageReceiver: conv.lastMessage?.receiverId,
                lastMessageIsRead: conv.lastMessage?.isRead
            })
        })

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

// 测试函数：在控制台输出会话列表
export async function testConversationList() {
    console.log('=== 开始测试会话列表获取 ===')
    try {
        const conversations = await getConversationList()
        console.log('=== 测试结果 ===')
        console.log('获取到的会话数量:', conversations.length)

        if (conversations.length > 0) {
            console.log('=== 会话详情 ===')
            conversations.forEach((conv, index) => {
                console.log(`\n--- 会话 ${index + 1} ---`)
                console.log('对方用户ID:', conv.otherUserId)
                console.log('未读消息数:', conv.unreadCount)
                console.log('最新消息:', conv.lastMessage)

                if (conv.lastMessage) {
                    console.log('  消息ID:', conv.lastMessage.messageId)
                    console.log('  发送者ID:', conv.lastMessage.senderId)
                    console.log('  接收者ID:', conv.lastMessage.receiverId)
                    console.log('  消息内容:', conv.lastMessage.content)
                    console.log('  发送时间:', conv.lastMessage.sentAt)
                    console.log('  是否已读:', conv.lastMessage.isRead)
                }
            })
        } else {
            console.log('没有找到任何会话')
        }

        console.log('=== 测试完成 ===')
        return conversations
    } catch (error) {
        console.error('测试失败:', error)
        return []
    }
}

// 将测试函数暴露到全局，方便在控制台调用
if (typeof window !== 'undefined') {
    (window as any).testConversationList = testConversationList
    console.log('测试函数已暴露到全局: window.testConversationList()')
}

