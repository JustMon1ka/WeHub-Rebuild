import axios from 'axios';
import type { PostDetail } from "./types";
import { unwrap } from "./types";
import { useAuthState } from './utils/useAuthState';
import type {
  ToggleLikeRequest,
  BaseResp,
  FavoriteListResp,
  SearchSuggestions,
  SearchResponse,
  Comment,
  CommentRequest
} from "./types";

// 设置基础URL - 修正为正确的API根路径
axios.defaults.baseURL = 'http://localhost:5000';
axios.interceptors.request.use(config => {
  const token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMDAxNDAiLCJ1bmlxdWVfbmFtZSI6InRlc3R1c2VyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoidGVzdHVzZXJAZXhhbXBsZS5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEwMDE0MCIsInN0YXR1cyI6IjAiLCJleHAiOjE3NTUxNjI3NjIsImlzcyI6IllvdXJBcHAiLCJhdWQiOiJZb3VyQXBwVXNlcnMifQ.sUN81A9VyyR69RwlGgjfT9QMmlRSqrXBlOW7T74V4OY";
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// 点赞/取消赞 - 修正端点路径
export async function toggleLike(data: ToggleLikeRequest) {
  const resp = await axios.post<BaseResp>("/api/posts/like", data);
  return resp.data;
}

// 收藏/取消收藏 - 修正端点路径
export async function toggleFavorite(data: { type: string; target_id: number; favorite: boolean }) {
  const resp = await axios.post<BaseResp>("/api/posts/favorite", data);
  return resp.data;
}

// 获取我的收藏
export async function getMyFavorites() {
  const resp = await axios.get<FavoriteListResp>("/api/posts/favorite");
  return resp.data;
}

// 获取搜索建议
export async function getSearchSuggestion(keyword?: string, limits: number = 10) {
  const resp = await axios.get<SearchSuggestions>("/api/posts/search/suggest", {
    params: { keyword, limits }
  });
  return resp.data;
}

// 搜索相关帖子
export async function getSearch(query?: string, limits?: number) {
  const resp = await axios.get<SearchResponse>("/api/posts/search", {
    params: { query, limits }
  });
  return resp.data;
}

// 获取帖子详情 - 修正端点路径
export async function getPostDetail(postId: number): Promise<PostDetail> {
  const res = await axios.get(`/api/posts/${postId}`);
  return unwrap<PostDetail>(res.data);
}

// 分享帖子 - 修正端点路径
export async function sharePost(targetId: number, comment: string): Promise<any> {
  const res = await axios.post("/api/posts/share", {
    targetId,
    comment
  });
  return unwrap(res.data);
}

// 评论相关功能 - 统一使用axios
export const postService = {
  // 获取帖子评论
  async getComments(postId: number): Promise<Comment[]> {
    const resp = await axios.get("/api/posts/comments", {
      params: { post_id: postId }
    });
    const data = unwrap<any>(resp.data);
    return data.data || [];
  },

  // 发表评论
  async submitComment(commentData: CommentRequest): Promise<any> {
    const resp = await axios.post("/api/posts/comment", commentData);
    return unwrap(resp.data);
  },

  // 删除评论
  async deleteComment(type: 'comment' | 'reply', targetId: number): Promise<boolean> {
    const resp = await axios.delete("/api/posts/comment", {
      params: { type, target_id: targetId }
    });
    const data = unwrap<any>(resp.data);
    return data.code === 200;
  }
};

// 发布新帖子
export async function publishPost(postData: any) {
  const resp = await axios.post("/api/posts/publish", postData);
  return unwrap(resp.data);
}

// 删除帖子
export async function deletePost(postId: number) {
  const resp = await axios.delete("/api/posts", {
    params: { post_id: postId }
  });
  return unwrap(resp.data);
}