import axios from 'axios';
import User from '@/modules/auth/scripts/User.ts';

axios.defaults.baseURL = 'http://localhost:5000/api';
axios.interceptors.request.use(config => {
  // 设置 Bearer 认证
  let token = User.getInstance()?.userAuth?.token || null;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export async function getCircles() {
  // TODO: replace with real API
  // return axios.get('/circles')
  //   .then(res => res.data);
  return {data:
      [
        {circleId: 100000, name: "无"},
        {circleId: 100001, name: "科技生活"},
        {circleId: 100002, name: "电子竞技"},
        {circleId: 100003, name: "体育赛事"}
      ]
  }
}

export async function uploadMedia(file: File) {
  const form = new FormData();
  form.append('file', file);
  return axios.post('/media/upload', form)
    .then(res => res.data);
}

export async function getMediaUrl(fileId: string) {
  return axios.get(`/media/${fileId}`, { responseType: 'blob' })
    .then(res => URL.createObjectURL(res.data));
}

export async function addTags(name: string[]) {
  return axios.post('/tags/add', { tagsName: name }).then(res => res.data);
}

export async function getTagsByIds(ids: number[]) {
  return axios.get(`/tags`, { params: { ids: ids.join(',') } }).then(res => res.data);
}

export async function getPopularTags() {
  return axios.get('/tags/poplular').then(res => res.data);
}

export async function publishPost(payload: {
  circleId: number | null;
  title: string;
  content: string;
  tags: number[];
}) {
  return axios.post('/posts/publish', payload).then(res => res.data);
}
