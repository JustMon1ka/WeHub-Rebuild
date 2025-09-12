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
  CommentRequest,
  PostListItem
} from "./types";
import User from '@/modules/auth/scripts/User.ts';

// ✅ 新增：为帖子模块创建“专用实例”，不再依赖全局 defaults
const API_BASE = "http://localhost:5000/api"; // '/api' 或 'http://localhost:5000/api'
const postHttp = axios.create({ baseURL: API_BASE });

// 携带 token（仍然保留原有逻辑）
postHttp.interceptors.request.use(config => {
  const token = User.getInstance()?.userAuth?.token || null;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// ❌ 删除这一句（会和别处冲突，造成“偶尔被覆盖”）：
// axios.defaults.baseURL = 'http://localhost:5000';

// 下面所有接口统一改成：不要以 “/” 开头（重要！）
// 这样 baseURL='/api' ➜ 最终 /api/xxx，不会出现 /api/api/xxx

export async function toggleLike(data: ToggleLikeRequest) {
  const resp = await axios.post<BaseResp>("/posts/like", data);
  return resp.data;
}

export async function toggleFavorite(data: { type: string; target_id: number; favorite: boolean }) {
  const resp = await axios.post<BaseResp>("/posts/favorite", data);
  return resp.data;
}

export async function getMyFavorites() {
  const resp = await axios.get<FavoriteListResp>("/posts/favorite");
  return resp.data;
}

export async function getSearchSuggestion(keyword?: string, limits: number = 10) {
  const resp = await postHttp.get<SearchSuggestions>("posts/search/suggest", { params: { keyword, limits }});
  return resp.data;
}

export async function getSearch(query?: string, limits?: number) {
  const resp = await postHttp.get<SearchResponse>("posts/search", { params: { query, limits }});
  return resp.data;
}

export async function getPostDetail(postId: number): Promise<PostDetail> {
  const res = await postHttp.get(`posts/${postId}`);
  return unwrap<PostDetail>(res.data);
}

export async function sharePost(targetId: number, comment: string): Promise<any> {
  const res = await axios.post("/posts/share", {
    targetId,
    comment
  });
  return unwrap(res.data);
}

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

export async function publishPost(postData: any) {
  const resp = await postHttp.post("posts/publish", postData);
  return unwrap(resp.data);
}

export async function deletePost(postId: number) {
  const resp = await postHttp.delete("posts", { params: { post_id: postId }});
  return unwrap(resp.data);
}

// ✅ 重点：getPostList —— 永远正确地命中 /api/posts/list
export async function getPostList(num: number, tailPostId?: number): Promise<PostListItem[]> {
  const resp = await postHttp.get<BaseResp<PostListItem[]>>("posts/list", {
    params: { num, lastId: tailPostId }
  });
  return unwrap<PostListItem[]>(resp.data);
}

export async function getPosts(ids?: string, userId?: number): Promise<PostListItem[]>{
  const resp = await postHttp.get<BaseResp<PostListItem[]>>("posts", {params: {ids, userId}});
  return unwrap<PostListItem[]>(resp.data);
}

// （可选）调试日志，看看最终请求是什么
postHttp.interceptors.request.use(cfg => {
  console.debug('[postHttp]', { baseURL: cfg.baseURL, url: cfg.url, params: cfg.params });
  return cfg;
});
