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
import User from '@/modules/auth/scripts/User.ts';

// 设置基础URL - 修正为正确的API根路径
axios.defaults.baseURL = 'http://localhost:5000';
axios.interceptors.request.use(config => {
  const token =  User.getInstance()?.userAuth?.token || null;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// 点赞/取消赞 - 修正端点路径
export async function toggleLike(data: ToggleLikeRequest) {
  const resp = await axios.post<BaseResp>("/posts/like", data);
  return resp.data;
}

// 收藏/取消收藏 - 修正端点路径
export async function toggleFavorite(data: { type: string; target_id: number; favorite: boolean }) {
  const resp = await axios.post<BaseResp>("/posts/favorite", data);
  return resp.data;
}

// 获取我的收藏
export async function getMyFavorites() {
  const resp = await axios.get<FavoriteListResp>("/posts/favorite");
  return resp.data;
}

// 获取搜索建议
export async function getSearchSuggestion(keyword?: string, limits: number = 10) {
  const resp = await axios.get<SearchSuggestions>("/posts/search/suggest", {
    params: { keyword, limits }
  });
  return resp.data;
}

// 搜索相关帖子
export async function getSearch(query?: string, limits?: number) {
  const resp = await axios.get<SearchResponse>("/posts/search", {
    params: { query, limits }
  });
  return resp.data;
}

// 获取帖子详情 - 修正端点路径
export async function getPostDetail(postId: number): Promise<PostDetail> {
  const res = await axios.get(`/posts/${postId}`);
  return unwrap<PostDetail>(res.data);
}

// 分享帖子 - 修正端点路径
export async function sharePost(targetId: number, comment: string): Promise<any> {
  const res = await axios.post("/posts/share", {
    targetId,
    comment
  });
  return unwrap(res.data);
}

// 评论相关功能 - 统一使用axios
export const postService = {
  // 获取帖子评论 - 修正参数传递
  async getComments(postId: number): Promise<Comment[]> {
    const resp = await axios.get("/posts/comments", {
      params: { post_id: postId }
    });
    const data = unwrap<any>(resp.data);
    return data.data || [];
  },

  // 发表评论 - 修正参数
  async submitComment(commentData: CommentRequest): Promise<any> {
    const resp = await axios.post("/posts/comment", commentData);
    return unwrap(resp.data);
  },

  // 删除评论 - 修正参数传递方式
  async deleteComment(type: 'comment' | 'reply', targetId: number): Promise<boolean> {
    const resp = await axios.delete("/posts/comment", {
      params: { 
        type: type,
        target_id: targetId 
      }
    });
    const data = unwrap<any>(resp.data);
    return data.code === 200;
  },

  async toggleLike(data: ToggleLikeRequest): Promise<any> {
    const resp = await axios.post("/posts/like", data);
    return unwrap(resp.data);
  }
};

// 发布新帖子
export async function publishPost(postData: any) {
  const resp = await axios.post("/posts/publish", postData);
  return unwrap(resp.data);
}

// 删除帖子
export async function deletePost(postId: number) {
  const resp = await axios.delete("/posts", {
    params: { post_id: postId }
  });
  return unwrap(resp.data);
}
