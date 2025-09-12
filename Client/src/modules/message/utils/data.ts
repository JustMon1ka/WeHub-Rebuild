/**
 * 数据处理相关工具函数
 */

import type { conversation, messageDisplay } from '../types'

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
        messageId: msg.messageId,
        senderId: msg.senderId,
        receiverId: msg.receiverId,
        content: msg.content,
        sentAt: msg.sentAt,
        isRead: msg.isRead,
        sendTime: msg.sentAt,
        sender: userCache.get(msg.senderId)!,
        receiver: userCache.get(msg.receiverId)!,
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
        const timeA = new Date(a.lastMessage?.sentAt || a.time || 0).getTime()
        const timeB = new Date(b.lastMessage?.sentAt || b.time || 0).getTime()
        return timeB - timeA // 降序排列，新消息在前
    })
}
