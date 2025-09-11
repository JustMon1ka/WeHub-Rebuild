import { GATEWAY } from '@/modules/core/public.ts'
import User from '@/modules/auth/scripts/User.ts'

/* API from media service */

const MEDIA_BASE_URL : string = `${GATEWAY}/api/media`;

async function uploadMediaAPI(userId: string, file: File) {
  const formData = new FormData();
  formData.append('file', file); // file 是 File 类型对象

  const token = User.getInstance()?.userAuth?.token || null;
  if (!token)
    throw new Error('请先登录');

  const result =  await fetch(`${MEDIA_BASE_URL}/upload`, {
    method: 'POST',
    headers: {
      'accept': 'application/json',
      'Authorization': token ? `Bearer ${token}` : '',
    },
    body: formData,
  });
  console.log(result);
  const resultData = await result.json();
  if (!result.ok) {

    throw new Error(resultData.msg);
  }
  return resultData;
}

export { uploadMediaAPI, MEDIA_BASE_URL };
