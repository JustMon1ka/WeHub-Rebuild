// 用户
export interface user {
    id: number; // id
    nickname: string; // 昵称
    avatar: string; // 头像
    url: string; // 主页url
}

// 消息
export interface message {
    messageId: number; // id
    content: string // 内容
    sendTime: string; // 发送时间
    sender: user // 发送者
    // isRead: boolean // 是否已读
    type: 'text' | 'image' // 消息类型
}

// 最新消息
export interface lastMessage {
    messageId: number; // id
    senderId: number; // 发送者id
    receiverId: number; // 接收者id
    content: string; // 内容
    sendTime: string; // 发送时间
    type: 'text' | 'image'; // 消息类型
    isRead: boolean; // 是否已读
}

export interface conversation2 {
    contactUserId: number; // 其他用户id
    lastMessage: lastMessage; // 最新消息
    unreadMessageCount: number; // 未读消息数
}

// 会话
export interface conversation {
    contactUser: user // 联系人 
    newestMessage: string; // 最新的消息
    time: string // 最近一次会话的时间
    unreadMessageCount: number; // 未读消息数
}

// 聊天记录
export interface chatHistory {
    contactUser: user;
    messageList: message[];
}

// 会话列表
export type conversationList = conversation[];

export type conversationList2 = conversation2[];
