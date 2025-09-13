/**
 * 通知工具类
 * 用于在用户操作时创建相应的通知
 */

import { createLikeNotification, type CreateLikeNotificationRequest } from '@/modules/notice/api'
import { User } from '@/modules/auth/public'

/**
 * 创建点赞通知
 * @param targetUserId 被点赞内容的作者ID (用户B)
 * @param targetId 被点赞的内容ID (帖子ID或评论ID)
 * @param targetType 内容类型 ("POST" 或 "COMMENT")
 * @param likeType 点赞类型，默认为1
 */
export async function notifyLike(
    targetUserId: number,
    targetId: number,
    targetType: 'POST' | 'COMMENT',
    likeType: number = 1
): Promise<void> {
    try {
        const user = User.getInstance()
        if (!user?.userAuth?.userId) {
            console.warn('[NotificationHelper] 用户未登录，无法创建点赞通知')
            return
        }

        const likerId = parseInt(user.userAuth.userId)

        // 如果点赞者就是内容作者，不需要创建通知
        if (likerId === targetUserId) {
            console.log('[NotificationHelper] 用户给自己的内容点赞，跳过通知')
            return
        }

        const request: CreateLikeNotificationRequest = {
            likerId,
            targetUserId,
            targetId,
            targetType,
            likeType
        }

        await createLikeNotification(request)
        console.log('[NotificationHelper] 点赞通知创建成功')
    } catch (error) {
        console.error('[NotificationHelper] 创建点赞通知失败:', error)
        // 不抛出异常，避免影响主要功能
    }
}

/**
 * 批量创建点赞通知（用于批量操作）
 * @param notifications 通知列表
 */
export async function notifyLikesBatch(notifications: Array<{
    targetUserId: number
    targetId: number
    targetType: 'POST' | 'COMMENT'
    likeType?: number
}>): Promise<void> {
    try {
        const user = User.getInstance()
        if (!user?.userAuth?.userId) {
            console.warn('[NotificationHelper] 用户未登录，无法创建批量点赞通知')
            return
        }

        const likerId = parseInt(user.userAuth.userId)

        // 过滤掉自己给自己点赞的通知
        const validNotifications = notifications.filter(n => n.targetUserId !== likerId)

        if (validNotifications.length === 0) {
            console.log('[NotificationHelper] 没有有效的点赞通知需要创建')
            return
        }

        // 并行创建所有通知
        const promises = validNotifications.map(notification =>
            createLikeNotification({
                likerId,
                targetUserId: notification.targetUserId,
                targetId: notification.targetId,
                targetType: notification.targetType,
                likeType: notification.likeType || 1
            })
        )

        await Promise.allSettled(promises)
        console.log('[NotificationHelper] 批量点赞通知创建完成')
    } catch (error) {
        console.error('[NotificationHelper] 创建批量点赞通知失败:', error)
    }
}
