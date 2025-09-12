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

// 活动相关类型
export interface Activity {
  activityId: number
  circleId: number
  title: string
  description?: string
  reward?: string
  startTime: string
  endTime: string
}

export interface CreateActivityRequest {
  title: string
  description?: string
  reward?: string
  startTime: string
  endTime: string
}

export interface UpdateActivityRequest {
  title: string
  description?: string
  reward?: string
  startTime: string
  endTime: string
}

// 活动参与状态枚举
export enum ParticipantStatus {
  InProgress = 'InProgress',
  Completed = 'Completed',
}

export enum RewardStatus {
  NotClaimed = 'NotClaimed',
  Claimed = 'Claimed',
}

// 活动参与者信息
export interface ActivityParticipant {
  activityId: number
  userId: number
  joinTime: string
  status: ParticipantStatus
  rewardStatus: RewardStatus
}

// 活动状态（用于前端显示）
export interface ActivityStatus {
  isActive: boolean
  isUpcoming: boolean
  isExpired: boolean
  canJoin: boolean
  hasJoined: boolean
  canComplete: boolean
  canClaimReward: boolean
}

// 活动相关类型
export interface Activity {
  activityId: number
  circleId: number
  title: string
  description?: string
  reward?: string
  startTime: string
  endTime: string
}

export interface CreateActivityRequest {
  title: string
  description?: string
  reward?: string
  startTime: string
  endTime: string
}

export interface UpdateActivityRequest {
  title: string
  description?: string
  reward?: string
  startTime: string
  endTime: string
}

// 活动参与状态枚举
export enum ParticipantStatus {
  InProgress = 'InProgress',
  Completed = 'Completed',
}

export enum RewardStatus {
  NotClaimed = 'NotClaimed',
  Claimed = 'Claimed',
}

// 活动参与者信息
export interface ActivityParticipant {
  activityId: number
  userId: number
  joinTime: string
  status: ParticipantStatus
  rewardStatus: RewardStatus
}

// 活动状态（用于前端显示）
export interface ActivityStatus {
  isActive: boolean
  isUpcoming: boolean
  isExpired: boolean
  canJoin: boolean
  hasJoined: boolean
  canComplete: boolean
  canClaimReward: boolean
}

export interface ApiResponse<T = any> {
  success: boolean
  data: T
  message?: string
  msg?: string
  code?: number
}

// 活动统计类型
export interface ActivityStats {
  total: number
  active: number
  participated: number
}

// 活动图片上传响应
export interface ActivityImageUploadResponse {
  imageUrl: string
  fileName: string
  fileSize: number
}

// 参与心得提交请求
export interface ParticipationNoteRequest {
  content: string
  contact?: string
}

// Activity 接口
export interface Activity {
  activityId: number
  circleId: number
  title: string
  description?: string
  reward?: string
  startTime: string
  endTime: string
  imageUrl?: string // 新增
  participantCount?: number // 新增
  completedCount?: number // 新增
  // 如果后端返回的字段名不同，请根据实际情况调整
}

// 在 types.ts 中添加帖子相关类型定义

// 帖子相关类型
export interface Post {
  postId: number
  userId: number
  circleId?: number
  title: string
  content: string
  tags: string[]
  createdAt: string
  views: number
  likes: number
}

export interface CreatePostRequest {
  circleId?: number
  title: string
  content: string
  tags?: number[]
}

export interface PostPublishResponse {
  postId: number
  createdAt: string
}

// 扩展现有的 ApiResponse 类型
export interface PostApiResponse<T = any> extends ApiResponse<T> {
  success: boolean
  data: T
  message?: string
}
