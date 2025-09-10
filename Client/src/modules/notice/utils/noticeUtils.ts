import type { notice, unreadNoticeCount } from '../types'

// 取出未读统计的纯数据类型（去除 BaseResp 外层）
export type UnreadSummaryData = unreadNoticeCount['data']

/**
 * 索引到通知类型的映射转换
 * @param idx 索引值
 * @returns 对应的通知类型或null
 */
export function indexToType(idx: number): 'at' | 'comment' | 'reply' | 'like' | 'repost' | null {
    if (idx === 0) return 'like'
    if (idx === 1) return 'comment'
    if (idx === 2) return 'reply'
    if (idx === 3) return 'at'
    if (idx === 4) return 'repost'
    return null
}

/**
 * 乐观更新未读通知状态
 * @param type 通知类型
 * @param readOnce 已读标记集合
 * @param unreadSummary 未读通知摘要
 */
export function optimisticMarkRead(
    type: 'at' | 'comment' | 'reply' | 'like' | 'repost',
    readOnce: Set<string>,
    unreadSummary: UnreadSummaryData | null
) {
    if (readOnce.has(type)) return
    readOnce.add(type)
    if (unreadSummary) {
        const delta = unreadSummary.unreadByType[type] || 0
        unreadSummary.unreadByType[type] = 0
        unreadSummary.totalUnread = Math.max(0, (unreadSummary.totalUnread || 0) - delta)
    }
}

/**
 * 根据帖子ID筛选点赞通知
 * @param postId 帖子ID
 * @param noticeList 通知列表
 * @returns 该帖子的点赞通知列表
 */
export function getLikeNoticesForPost(postId: number, noticeList: notice[]) {
    return noticeList.filter(
        (notice) => notice.type === 'like' && notice.targetPostId === postId
    )
}

/**
 * 格式化未读通知数量显示
 * @param n 数量
 * @returns 格式化后的显示文本
 */
export function displayUnreadNoticeCount(n: number): string {
    return n > 99 ? '99+' : n.toString()
}

/**
 * 根据类型获取未读通知数量
 * @param index 类型索引
 * @param unreadSummary 未读通知摘要
 * @param readOnce 已读标记集合
 * @returns 该类型的未读数量
 */
export function getUnreadCountByType(
    index: number,
    unreadSummary: UnreadSummaryData | null,
    readOnce: Set<string>
): number {
    const type = indexToType(index)
    if (type && readOnce.has(type)) return 0
    if (!unreadSummary) return 0
    const u = unreadSummary.unreadByType
    if (index === 0) return u.like || 0
    if (index === 1) return u.comment || 0
    if (index === 2) return u.reply || 0
    if (index === 3) return u.at || 0
    if (index === 4) return u.repost || 0
    return 0
}
