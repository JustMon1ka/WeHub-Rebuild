import type { user } from "../message/types";

// 通知类型
export const noticeType = {
    like: 0, // 点赞
    comment: 1, // 评论
    at: 2, // @
    follow: 3, // 关注
} as const;
export type noticeType = typeof noticeType[keyof typeof noticeType];

// 未读通知数量
export interface unreadNoticeCount {
    totalUnread: number // 总未读数
    unreadByType: {
        reply: number  // 回复
        like: number  // 点赞
        repost: number  // 转发
        mention: number  // @
    }
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

// 通知联合类型
export type notice = likeNoticeInfo | commentNoticeInfo | atNoticeInfo | followNoticeInfo;

export type noticeList = notice[];