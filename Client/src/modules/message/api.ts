import axios from 'axios'
import type { conversationList2 } from './types'

// 获取会话列表
export async function getConversationList(): Promise<conversationList2> {
    const { data } = await axios.get<conversationList2>('http://127.0.0.1:4523/m1/7050705-6770801-default/api/messages')
    return data
}

