// src/types/community.ts
export interface Community {
  id: string | number
  name: string
  description: string
  memberCount: number
  isJoined: boolean
  category?: string
  isPrivate?: boolean
  ownerId?: number
  maxMembers?: number
  createdAt?: string
  updatedAt?: string
}

export interface CreateCommunityData {
  name: string
  description: string
  category: string
  isPrivate: boolean
  maxMembers?: number
}

export interface TrendingTopic {
  id: string
  tag: string
  category: string
  postCount: number
}

export interface User {
  id: string
  username: string
  displayName: string
}

// 后端返回的圈子数据格式
export interface CircleResponse {
  circleId: number
  name: string
  description: string
  category: string
  isPrivate: boolean
  ownerId: number
  memberCount: number
  maxMembers?: number
  createdAt: string
  updatedAt: string
  isMember?: boolean
}

// API响应格式
export interface JoinLeaveResponse {
  success: boolean
  message?: string
  data?: any
}

export interface MembershipResponse {
  isMember: boolean
  message?: string
}
