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

// 收藏/取消收藏
export async function toggleFavorite(data: { type: string; target_id: number; favorite: boolean }) {
  return axios.post("/api/favorite/toggle", data);
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

export async function getPostDetail(postId: number): Promise<PostDetail> {
  const res = await axios.get(`/api/posts/${postId}`);
  return unwrap<PostDetail>(res.data);
}

//评论
export const postService = {
  // 获取帖子评论
  async getComments(postId: number): Promise<Comment[]> {
    const token = localStorage.getItem('auth_token');
    const response = await fetch(`${axios.defaults.baseURL}/api/posts/comments?post_id=${postId}`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    if (data.code === 200) {
      return data.data;
    } else {
      throw new Error(data.msg || '获取评论失败');
    }
  },

  // 发表评论
  async submitComment(commentData: CommentRequest): Promise<any> {
    const { authToken } = useAuthState();
    const response = await fetch(`${axios.defaults.baseURL}/api/posts/comment`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': authToken.value ? `Bearer ${authToken.value}` : ''
      },
      body: JSON.stringify(commentData)
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    if (data.code === 200) {
      return data.data;
    } else {
      throw new Error(data.msg || '提交评论失败');
    }
  },

  // 点赞/取消点赞
  async toggleLike(likeData: ToggleLikeRequest): Promise<boolean> {
    const { authToken } = useAuthState();
    const response = await fetch(`${axios.defaults.baseURL}/api/posts/like`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': authToken.value ? `Bearer ${authToken.value}` : ''
      },
      body: JSON.stringify(likeData)
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    return data.code === 200;
  },

  // 删除评论
  async deleteComment(type: 'comment' | 'reply', targetId: number): Promise<boolean> {
    const { authToken } = useAuthState();
    const response = await fetch(`${axios.defaults.baseURL}/api/posts/comment?type=${type}&target_id=${targetId}`, {
      method: 'DELETE',
      headers: {
        'Authorization': authToken.value ? `Bearer ${authToken.value}` : ''
      }
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    return data.code === 200;
  }
};