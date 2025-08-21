// Client/src/modules/report/types.ts
export type ReportTargetType = 'post' | 'comment' | 'message' | 'user'

export interface ReportMeta {
    targetType: ReportTargetType        // 举报类型：帖子/评论/私信/用户
    targetId: number                    // 被举报目标ID（帖子ID/评论ID/消息ID/用户ID）
    reporterId: number                  // 举报者ID
    reportedUserId?: number             // 被举报者用户ID（若能确定）
    reportTime: string                   // 举报时间
}

export interface ReportSubmitPayload extends ReportMeta {
    reasons: string[]                   // 选择的举报理由
    description: string                 // 详细描述
}