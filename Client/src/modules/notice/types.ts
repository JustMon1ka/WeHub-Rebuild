import type { user } from "../message/types";

// 基础响应类型
export type BaseResp<T = unknown> = {
    code: number;
    msg: string | null;
    data: T;
};

// 通知类型
export const noticeType = {
    like: 0, // 点赞
    comment: 1, // 评论
    at: 2, // @
    repost: 3, // 转发
} as const;
export type noticeType = typeof noticeType[keyof typeof noticeType];

// 未读通知数量 
export type unreadNoticeCount = BaseResp<{
    totalUnread: number // 总未读数
    unreadByType: {
        reply: number  // 回复
        like: number  // 点赞
        repost: number  // 转发
        at: number  // @
    }
}>

// 点赞通知响应
export interface likeNoticeItem {
    targetId: number // 目标id(帖子/评论)
    targetType: 'post' | 'comment' // 目标类型
    lastLikedAt: string // 最后点赞时间
    likeCount: number // 点赞数
    likerIds: number[] // 点赞用户id列表
}


export type likeNoticeResponse = BaseResp<{
    unread: likeNoticeItem[]
    read: {
        total: number
        items: likeNoticeItem[]
    }
}>


// 回复通知响应
export interface replyNoticeItem {
    replyId: number // 回复id
    replyPoster: number // 回复者用户id
    contentPreview: string // 回复内容预览
    createdAt: string // 创建时间
}


export type replyNoticeResponse = BaseResp<{
    total: number // 回复总数
    items: replyNoticeItem[]
}>


// 转发通知响应
export interface repostNoticeItem {
    repostId: number // 转发id
    userId: number // 转发者用户id
    postId: number // 帖子id
    commentPreview: string | null // 转发评论摘要(可为空)
    createdAt: string // 创建时间
}

export type repostNoticeResponse = BaseResp<{
    total: number // 转发总数
    items: repostNoticeItem[]
}>


// @通知响应
export interface atNoticeItem {
    targetId: number // 目标id(帖子/评论)
    targetType: 'post' | 'comment' // 目标类型
    userId: number // 提及者用户id
    createdAt: string // 创建时间
}

export type atNoticeResponse = BaseResp<{
    total: number // 提及总数
    items: atNoticeItem[]
}>


// 标记已读响应
export type markReadResponse = BaseResp<{
    success: boolean
}>

// 点赞者列表
export type likeNoticeListById = BaseResp<{
    total: number // 总数
    items: number[] // 点赞者用户id列表
}>

export function unwrap<T>(payload: BaseResp<T>): T {
    // 兼容BaseResp包装结构
    // 如果没有data字段，就当作T直接返回
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error
    return payload?.data ?? payload;
}

// 通知
export interface baseNoticeInfo {
    noticeId: number; // 通知id
    sender: user; // 发送者
    time: string; // 发送时间
    isRead: boolean; // 是否已读
    objectType: 'post' | 'comment' | 'user'; // 目标对象类型  
};

export interface targetPostInfo {
    targetPostId: number; // 目标帖子id
    targetPostTitle: string; // 目标帖子简介 
    targetPostTitleImage: string; // 目标帖子简介图片
}

export interface targetCommentInfo {
    targetCommentId: number; // 目标评论id
    targetCommentContent: string; // 目标评论内容
    targetCommentAuthor: string; // 目标评论作者
}

// 点赞通知
export interface likeNoticeInfo extends baseNoticeInfo, targetPostInfo {
    type: 'like';
    // 当 objectType 为 'comment' 时，以下字段为必需
    targetCommentId?: number;
    targetCommentContent?: string;
    targetCommentAuthor?: string;
}

// 评论通知
export interface commentNoticeInfo extends baseNoticeInfo, targetPostInfo {
    type: 'comment';
    newCommentContent: string; // 新评论/回复内容
    // 当 objectType 为 'comment' 时，以下字段为必需
    targetCommentId?: number;
    targetCommentContent?: string;
    targetCommentAuthor?: string;
}

// @通知
export interface atNoticeInfo extends baseNoticeInfo, targetPostInfo {
    type: 'at';
    atContent: string; // @内容
    // 当 objectType 为 'comment' 时，以下字段为必需
    targetCommentId?: number;
    targetCommentContent?: string;
    targetCommentAuthor?: string;
}

// 关注通知
export interface followNoticeInfo extends baseNoticeInfo, targetPostInfo {
    type: 'follow';
    objectType: 'user';
}

// 转发通知
export interface repostNoticeInfo extends baseNoticeInfo, targetPostInfo {
    type: 'repost';
    repostContent: string; // 转发内容
    // 当 objectType 为 'comment' 时，以下字段为必需
    targetCommentId?: number;
    targetCommentContent?: string;
    targetCommentAuthor?: string;
}

// 通知联合类型
export type notice = likeNoticeInfo | commentNoticeInfo | atNoticeInfo | followNoticeInfo | repostNoticeInfo;

export type noticeList = notice[];