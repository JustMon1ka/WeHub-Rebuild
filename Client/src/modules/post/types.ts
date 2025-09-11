// 点赞请求体（按你们后端 LikeRequest）
export type ToggleLikeRequest = {
  type: 'post' | 'comment' | 'reply'; 
  target_id: number;
  like: boolean;      // true=点赞，false=取消点赞
  user_id: number;    // 点赞用户ID
};

// 标准响应
export type BaseResp<T = unknown> = {
  code: number;
  msg: string | null;
  data: T;
};

// （可选）我的收藏列表响应
export type FavoriteListResp = BaseResp<{
  post_ids: number[];
}>;

// 前端展示用的帖子模型（可按真实返回调整）
export type PostViewModel = {
  id: number;
  content: string;
  likeCount: number;
  isLiked: boolean;
  isFavorited: boolean;
};

export enum KeywordType
{
  Title,
  Tag,
  Content,
  User,
  Circle,
  Other
}

export type SearchSuggestions = BaseResp<{
  keyword: string,
  type: KeywordType
}[]>;

export type SearchResponse = BaseResp<{
  postId: number,
  title: string,
  tags: Array<string>,
  content: string
}[]>;

// 帖子列表项
export type PostListItem = {
  postId: number;
  userId: number;
  title: string;
  content?: string;
  tags?: string[];
  createdAt: string; // ISO 时间字符串
  views?: number;
  likes: number;
  circleId?: number | null;
};

export interface PostDetail {
  postId: number;
  userId: number;
  title: string;
  content: string;
  tags: string[];
  createdAt: string | Date;
  views: number;
  likes: number;
  circleId?: number | null;
}

export function unwrap<T>(payload: BaseResp<T>): T {
  // 兼容你们的 BaseHttpResponse 包装结构
  // 如果没有 data 字段，就当作 T 直接返回
  // eslint-disable-next-line @typescript-eslint/ban-ts-comment
  // @ts-expect-error
  return payload?.data ?? payload;
}

export interface User {
  id: number;
  name: string;
  username: string;
  avatar: string;
}

export interface Comment {
  type: 'comment' | 'reply';
  comment_id?: number;
  reply_id?: number;
  user_id: number;
  user?: {
    id: number;
    name: string;
    username: string;
    avatar: string;
  };
  content: string;
  created_at: string;
  likes: number;
  parent_id?: number;
  reply_to_user_id?: number;
  replies?: Comment[];
}

export interface Post {
  post_id: number;
  user_id: number;
  title: string;
  content: string;
  tags: string[];
  created_at: string;
  views: number;
  likes: number;
  comment_count: number;
  user?: User; // 前端扩展字段
  comments?: Comment[]; // 前端扩展字段
}

export interface CommentRequest {
  type: number;        // 使用数字：0 = Comment, 1 = Reply
  targetId: number;    // 目标ID
  content: string;     // 内容不能为空
}

// 添加枚举常量
export const CommentType = {
  Comment: 0,
  Reply: 1
} as const;

// 后端返回的评论响应类型
export interface CommentResponse {
  type: number;
  id: number;
  targetId: number;
  userId: number;
  userName: string;        // 添加 userName
  avatarUrl: string | null; // 添加 avatarUrl
  content: string;
  createdAt: string;
  likes: number;
  postTitle: string | null;
}

// 更新转换函数
export function convertCommentResponseToFrontend(response: CommentResponse): Comment {
  return {
    type: response.type === 1 ? 'reply' : 'comment',
    comment_id: response.id,
    reply_id: response.type === 1 ? response.id : undefined,
    user_id: response.userId,
    user: {
      id: response.userId,
      name: response.userName,          // 使用后端返回的 userName
      username: response.userName,      // 如果没有单独的用户名字段，可以用userName
      avatar: response.avatarUrl || getDefaultAvatar(response.userId) // 使用avatarUrl或默认头像
    },
    content: response.content,
    created_at: response.createdAt,
    likes: response.likes || 0,
  };
}

// 确保默认头像函数可用
export function getDefaultAvatar(userId: number): string {
  const colors = ['7dd3fc', 'ec4899', '8b5cf6', '34d399', 'facc15'];
  const color = colors[userId % colors.length];
  return `https://placehold.co/100x100/${color}/0f172a?text=用户${userId}`;
}

// 转换函数：将前端Comment转换为后端CommentRequest
export function convertCommentToBackendRequest(comment: Partial<Comment>): CommentRequest {
  return {
    type: comment.type === 'reply' ? CommentType.Reply : CommentType.Comment,
    targetId: comment.comment_id || comment.reply_id || 0,
    content: comment.content || ''
  };
}