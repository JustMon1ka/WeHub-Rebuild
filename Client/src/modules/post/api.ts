import http from "../core/http";
import type { ToggleLikeRequest, BaseResp, FavoriteListResp } from "./types";

// 点赞/取消赞
export async function toggleLike(data: ToggleLikeRequest) {
  // 对应后端：POST /api/posts/like  body: { type, target_id, like }
  const resp = await http.post<BaseResp>("/api/posts/like", data);
  return resp.data;
}

// 收藏/取消收藏（很多后端是“切换收藏”，只要 post_id）
// 如果你们后端要求 { favorite: boolean }，把第二个参数加上即可
export async function toggleFavorite(postId: number) {
  const resp = await http.post<BaseResp>("/api/posts/favorite", { post_id: postId });
  return resp.data;
}

// （可选）获取我的收藏
export async function getMyFavorites() {
  const resp = await http.get<FavoriteListResp>("/api/posts/favorite");
  return resp.data;
}
