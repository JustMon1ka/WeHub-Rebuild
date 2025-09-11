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
  post_id: number;
  content: string;
  parent_id: number;
  reply_to_user_id?: number;
}
