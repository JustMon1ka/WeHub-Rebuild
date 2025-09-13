// 基础响应类型
export type BaseResp<T = unknown> = {
    code: number;
    msg: string | null;
    data: T;
};

// 用户
export interface user {
    id: number; // id
    nickname: string; // 昵称
    avatar: string; // 头像
    url: string; // 主页url
}

// 消息
export interface message {
    messageId: number; // id (注意：后端返回的是messageId，不是MessageId)
    senderId: number; // 发送者id (注意：后端返回的是senderId，不是SenderId)
    receiverId: number; // 接收者id (注意：后端返回的是receiverId，不是ReceiverId)
    content: string; // 内容 (注意：后端返回的是content，不是Content)
    sentAt: string; // 发送时间 (注意：后端返回的是sentAt，不是SentAt)
    isRead: boolean; // 是否已读 (注意：后端返回的是isRead，不是IsRead)
}

// 扩展的消息类型（用于前端显示）
export interface messageDisplay extends message {
    messageId: number; // 前端使用的id
    senderId: number; // 发送者ID
    receiverId: number; // 接收者ID
    content: string; // 前端使用的内容
    sentAt: string; // 发送时间
    sendTime: string; // 前端使用的时间格式
    sender: user; // 发送者信息
    receiver: user; // 接收者信息
    isRead: boolean; // 是否已读
    type: 'text' | 'image'; // 消息类型
}

// 会话
export interface conversation {
    otherUserId: number; // 其他用户id (注意：后端返回的是otherUserId，不是OtherUserId)
    lastMessage: message; // 最新消息 (注意：后端返回的是lastMessage，不是LastMessage)
    unreadCount: number; // 未读消息数 (注意：后端返回的是unreadCount，不是UnreadCount)
    // 添加前端显示需要的字段
    contactUser?: user; // 联系人用户信息
    newestMessage?: string; // 最新消息内容（用于显示）
    time?: string; // 时间（用于显示）
}

// 聊天历史记录
export interface chatHistory {
    contactUser: user; // 联系人用户信息
    messageList: messageDisplay[]; // 消息列表
}

// 会话列表
export type conversationList = conversation[];

// 聊天记录列表
export type chatHistoryList = chatHistory[];

// 空状态类型
export interface EmptyState {
    type: 'no-conversations' | 'no-messages' | 'loading';
    message: string;
    icon?: string;
}

// API响应类型
export type conversationListResponse = BaseResp<conversationList>;

// 获取聊天记录响应
export type chatHistoryResponse = BaseResp<{
    messages: message[];
    total: number;
}>;

// 发送消息响应
export type sendMessageResponse = BaseResp<{
    messageId: number;
    success: boolean;
}>;

// unwrap函数，用于解包BaseResp
export function unwrap<T>(payload: BaseResp<T>): T {
    // 兼容BaseResp包装结构
    // 如果没有data字段，就当作T直接返回
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error
    return payload?.data ?? payload;
}