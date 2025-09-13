// modules/Founding/api.ts
import type { HotTopic, RecommendUser, TodayHot } from "./types"
import { currentUserId } from "@/modules/Founding/store/CurrentUser";
import { GATEWAY } from '@/modules/core/public.ts'

const BASE_URL = GATEWAY;

// 🔹 发现页相关
const RECOMMEND_URL = `${BASE_URL}/recommend`

export async function fetchHotTopics(): Promise<HotTopic[]> {
  const res = await fetch(`${RECOMMEND_URL}/topics?user_id=${currentUserId.value}`)
  const data = await res.json()
  return data.topics || []
}

export async function fetchRecommendUsers(): Promise<RecommendUser[]> {
  const res = await fetch(`${RECOMMEND_URL}/users?user_id=${currentUserId.value}`)
  const data = await res.json()
  return data.recommended_users || []
}

export async function fetchTodayHot(): Promise<TodayHot[]> {
  const res = await fetch(`${RECOMMEND_URL}/hot?user_id=${currentUserId.value}`)
  const data = await res.json()
  return data.hots || []
}

// 🔹 新增: 获取话题帖子数
export async function fetchTopicCount(tagName: string): Promise<number> {
  const res = await fetch(`${BASE_URL}/api/topics/${encodeURIComponent(tagName)}`)
  const data = await res.json()
  return data.count || 0
}
