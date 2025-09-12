// modules/Founding/types.ts
export interface HotTopic {
  topic: string
  desc?: string
  count: number
}

export interface RecommendUser {
  user_id: number
  username: string
}

export interface TodayHot {
  topic: string
  category?: string
  count: number
}
