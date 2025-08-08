import type { user } from "./message";

// 通知类型
export const noticeType = {
    like: 0, // 点赞
    comment: 1, // 评论
    at: 2, // @
    follow: 3, // 关注
} as const;
export type noticeType = typeof noticeType[keyof typeof noticeType];

// 通知
export interface baseNoticeInfo {
    sender: user; // 发送者
    time: string; // 发送时间
};

export interface targetPostInfo {
    targetPostId: number; // 目标帖子id
    targetPostTitle: string; // 目标帖子简介
    targetPostTitleImage: string; // 目标帖子简介图片
}

export interface likeNoticeInfo extends baseNoticeInfo, targetPostInfo {
    type: 'like';
}

export interface commentNoticeInfo extends baseNoticeInfo, targetPostInfo {
    type: 'comment';
    commentContent: string; // 评论内容
    commentType: "post" | "comment"; // 评论类型
}

export interface atNoticeInfo extends baseNoticeInfo, targetPostInfo {
    type: 'at';
    atContent: string; // @内容
}

export interface followNoticeInfo extends baseNoticeInfo, targetPostInfo {
    type: 'follow';
}

// 通知列表
export type notice = likeNoticeInfo | commentNoticeInfo | atNoticeInfo | followNoticeInfo;

export type noticeList = notice[];