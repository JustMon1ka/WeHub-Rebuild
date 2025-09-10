import type { PostDetail } from "../../post/types";

const KEY = "wehub:share:originalPost";

export function stashOriginalPost(post: PostDetail) {
  sessionStorage.setItem(KEY, JSON.stringify(post));
}

// 读取后顺手清掉，避免污染后续页面
export function takeOriginalPost(): PostDetail | null {
  const raw = sessionStorage.getItem(KEY);
  if (!raw) return null;
  sessionStorage.removeItem(KEY);
  try {
    return JSON.parse(raw) as PostDetail;
  } catch {
    return null;
  }
}