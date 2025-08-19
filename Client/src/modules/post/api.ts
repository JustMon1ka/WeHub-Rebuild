import axios from 'axios';
import type {
  ToggleLikeRequest,
  BaseResp,
  FavoriteListResp,
  SearchSuggestions,
  SearchResponse
} from "./types";

axios.defaults.baseURL = 'http://localhost:5000/api/posts';
axios.interceptors.request.use(config => {
  // 设置 Bearer 认证
  // const token = localStorage.getItem('token');
  // 临时测试用token，一天后过期
  const token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMDAxNDAiLCJ1bmlxdWVfbmFtZSI6InRlc3R1c2VyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoidGVzdHVzZXJAZXhhbXBsZS5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEwMDE0MCIsInN0YXR1cyI6IjAiLCJleHAiOjE3NTUxNjI3NjIsImlzcyI6IllvdXJBcHAiLCJhdWQiOiJZb3VyQXBwVXNlcnMifQ.sUN81A9VyyR69RwlGgjfT9QMmlRSqrXBlOW7T74V4OY";

  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// 点赞/取消赞
export async function toggleLike(data: ToggleLikeRequest) {
  // 对应后端：POST /api/posts/like  body: { type, target_id, like }
  const resp = await axios.post<BaseResp>("/like", data);
  return resp.data;
}

// 收藏/取消收藏（很多后端是“切换收藏”，只要 post_id）
// 如果你们后端要求 { favorite: boolean }，把第二个参数加上即可
export async function toggleFavorite(postId: number) {
  const resp = await axios.post<BaseResp>("/favorite", { post_id: postId });
  return resp.data;
}

// （可选）获取我的收藏
export async function getMyFavorites() {
  const resp = await axios.get<FavoriteListResp>("/favorite");
  return resp.data;
}

// 获取搜索建议
export async function getSearchSuggestion(keyword?: string, limits: number = 10){
  const resp = await axios.get<SearchSuggestions>("/search/suggest", {params: {keyword: keyword, limits: limits}});
  return resp.data;
}

// 搜索相关帖子
export async function getSearch(query?: string, limits?: number){
  const resp = await axios.get<SearchResponse>("/search", {params: {query: query, limits: limits}});
  return resp.data;
}
