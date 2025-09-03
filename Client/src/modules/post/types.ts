// 点赞请求体（按你们后端 LikeRequest）
export type ToggleLikeRequest = {
  type: "post";       // 如果后续要点赞评论，可传 'comment'
  target_id: number;
  like: boolean;      // true=点赞，false=取消点赞
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