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
  data: T | null;
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
