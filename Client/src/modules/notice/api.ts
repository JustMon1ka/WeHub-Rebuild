import axios from 'axios'
import type { unreadNoticeCount } from './types'

// 获取未读通知数量
export async function getUnreadNoticeCount(): Promise<unreadNoticeCount> {
    const { data } = await axios.get<unreadNoticeCount>(
        'http://127.0.0.1:4523/m1/7050705-6770801-default/api/notifications'
    )
    return data
}

// 标记通知已读
export async function markNotificationsRead(type: 'reply' | 'like' | 'repost' | 'mention') {
    await axios.post(
        'http://127.0.0.1:4523/m1/7050705-6770801-default/api/notifications/read',
        { type }
    )
    return true
}