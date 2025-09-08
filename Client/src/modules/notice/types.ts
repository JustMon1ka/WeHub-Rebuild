import type { user } from "../message/types";

// 基础响应类型
export type BaseResp<T = unknown> = {
    code: number;
    msg: string | null;
    data: T;
};


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


// 评论通知响应
export interface commentNoticeItem {
    commentId: number // 评论ID
    userId: number // 评论者用户ID
    postId: number // 所属帖子ID
    contentPreview: string // 评论内容摘要（前50字符）
    createdAt: string // 创建时间
}


export type commentNoticeResponse = BaseResp<{
    total: number // 评论总数
    items: commentNoticeItem[]
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

// 获取指定目标的点赞者列表参数
export interface getLikersByTargetParams {
    targetType: 'post' | 'comment'
    targetId: number
    page?: number
    pageSize?: number
}

// 获取指定目标的点赞者列表响应
export type getLikersByTargetResponse = BaseResp<{
    total: number // 点赞者总数
    items: number[] // 分页的点赞者用户ID列表
}>

export function unwrap<T>(payload: BaseResp<T>): T {
    // 兼容BaseResp包装结构
    // 如果没有data字段，就当作T直接返回
    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-expect-error
    return payload?.data ?? payload;
}


// 帖子详细信息
export interface postDetail {
    postId: number; // 帖子ID
    userId: number; // 用户ID
    title: string; // 帖子标题
    content: string; // 帖子内容
    tags: string[]; // 标签
    createdAt: string; // 创建时间
    views: number; // 浏览次数
    likes: number; // 点赞数
    circleId: number; // 圈子ID
}

// 帖子详细信息响应
export type postDetailResponse = BaseResp<postDetail>;

// 评论详细信息
export interface commentDetail {
    commentId: number; // 评论ID
    userId: number; // 用户ID
    postId: number; // 所属帖子ID
    content: string; // 评论内容
    createdAt: string; // 创建时间
    likes: number; // 点赞数
    parentCommentId?: number; // 父评论ID（回复评论时使用）
}

// 评论详细信息响应
export type commentDetailResponse = BaseResp<commentDetail>;

// 通知基础信息
export interface baseNotice {
    noticeId: number;
    type: 'comment' | 'repost' | 'at' | 'like';
    sender: {
        id: number;
        nickname: string;
        avatar: string;
        url: string;
    };
    time: string;
    isRead: boolean;
    objectType: 'post' | 'comment' | 'user';
    targetPostId: number;
    targetPostTitle: string;
    targetPostTitleImage: string;
}

// 评论通知
export interface commentNotice extends baseNotice {
    type: 'comment';
    newCommentContent: string;
}

// 转发通知
export interface repostNotice extends baseNotice {
    type: 'repost';
    repostContent: string;
}

// @通知
export interface atNotice extends baseNotice {
    type: 'at';
    atContent: string;
}

// 点赞通知
export interface likeNotice extends baseNotice {
    type: 'like';
}

// 通知联合类型
export type notice = commentNotice | repostNotice | atNotice | likeNotice;

export type noticeList = notice[];