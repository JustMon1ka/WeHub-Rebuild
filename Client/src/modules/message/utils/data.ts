/**
 * 数据处理相关工具函数
 */

import type { conversation, messageDisplay } from '../types'

/**
 * 模拟用户数据
 */
export const mockUsers = [
    {
        id: 1001,
        nickname: '张三',
        username: 'zhangsan',
        avatar: 'https://placehold.co/100x100/4ade80/166534?text=张',
    },
    {
        id: 1002,
        nickname: '李四',
        username: 'lisi',
        avatar: 'https://placehold.co/100x100/f59e0b/92400e?text=李',
    },
    {
        id: 1003,
        nickname: '王五',
        username: 'wangwu',
        avatar: 'https://placehold.co/100x100/ef4444/dc2626?text=王',
    },
    {
        id: 1004,
        nickname: '赵六',
        username: 'zhaoliu',
        avatar: 'https://placehold.co/100x100/8b5cf6/7c3aed?text=赵',
    },
    {
        id: 1005,
        nickname: '钱七',
        username: 'qianqi',
        avatar: 'https://placehold.co/100x100/06b6d4/0891b2?text=钱',
    },
    {
        id: 1006,
        nickname: '孙八',
        username: 'sunba',
        avatar: 'https://placehold.co/100x100/84cc16/65a30d?text=孙',
    },
    {
        id: 1007,
        nickname: '周九',
        username: 'zhoujiu',
        avatar: 'https://placehold.co/100x100/f97316/ea580c?text=周',
    },
    {
        id: 1008,
        nickname: '吴十',
        username: 'wushi',
        avatar: 'https://placehold.co/100x100/ec4899/db2777?text=吴',
    },
]

/**
 * 模拟聊天记录数据
 */
export const mockChatHistory: Record<number, Array<{ content: string; time: string; sender: string }>> = {
    1001: [
        { content: '你好，最近怎么样？', time: '2024-01-15 10:30', sender: '张三' },
        { content: '我很好，谢谢关心！', time: '2024-01-15 10:32', sender: '我' },
        { content: '明天有空一起吃饭吗？', time: '2024-01-15 10:35', sender: '张三' },
        { content: '当然可以，几点？', time: '2024-01-15 10:36', sender: '我' },
    ],
    1002: [
        { content: '项目进展如何？', time: '2024-01-14 15:20', sender: '李四' },
        { content: '基本完成了，还需要测试', time: '2024-01-14 15:22', sender: '我' },
        { content: '好的，辛苦了', time: '2024-01-14 15:25', sender: '李四' },
    ],
    1003: [
        { content: '周末有什么计划？', time: '2024-01-13 20:15', sender: '王五' },
        { content: '想去爬山，你要一起吗？', time: '2024-01-13 20:18', sender: '我' },
        { content: '好主意！', time: '2024-01-13 20:20', sender: '王五' },
    ],
}

/**
 * 创建模拟会话数据
 * @param myUserId 当前用户ID
 * @returns 模拟会话数组
 */
export function createMockConversations(myUserId: number): conversation[] {
    return mockUsers.map((user, index) => ({
        OtherUserId: user.id,
        lastMessage: {
            MessageId: index + 1,
            SenderId: user.id,
            ReceiverId: myUserId,
            Content: `这是来自${user.nickname}的消息`,
            SendAt: new Date(Date.now() - index * 60000).toISOString(), // 每条消息间隔1分钟
            IsRead: index % 3 === 0, // 部分消息未读
        },
        UnreadCount: index % 3 === 0 ? Math.floor(Math.random() * 5) + 1 : 0,
        contactUser: {
            id: user.id,
            nickname: user.nickname,
            avatar: user.avatar,
            url: `/user/${user.id}`,
        },
        newestMessage: `这是来自${user.nickname}的消息`,
        time: new Date(Date.now() - index * 60000).toISOString(),
    }))
}

/**
 * 转换API消息数据为前端显示格式
 * @param messages API消息数组
 * @param userCache 用户缓存
 * @returns 前端显示格式的消息数组
 */
export function convertMessagesToDisplay(
    messages: any[],
    userCache: any
): messageDisplay[] {
    return messages.map((msg) => ({
        MessageId: msg.MessageId,
        SenderId: msg.SenderId,
        ReceiverId: msg.ReceiverId,
        Content: msg.Content,
        SendAt: msg.SendAt,
        IsRead: msg.IsRead,
        messageId: msg.MessageId,
        content: msg.Content,
        sendTime: msg.SendAt,
        sender: userCache.get(msg.SenderId)!,
        receiver: userCache.get(msg.ReceiverId)!,
        isRead: msg.IsRead,
        type: 'text' as const,
    }))
}

/**
 * 按时间排序会话列表
 * @param conversations 会话数组
 * @returns 按时间排序的会话数组
 */
export function sortConversationsByTime(conversations: conversation[]): conversation[] {
    return [...conversations].sort((a, b) => {
        const timeA = new Date(a.lastMessage?.SendAt || a.time || 0).getTime()
        const timeB = new Date(b.lastMessage?.SendAt || b.time || 0).getTime()
        return timeB - timeA // 降序排列，新消息在前
    })
}
