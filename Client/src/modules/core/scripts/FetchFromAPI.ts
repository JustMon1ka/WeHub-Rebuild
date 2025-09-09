import User from '@/modules/auth/scripts/User.ts'

/**
 * 一个通用的从API端点获取数据的函数。它包括错误处理和授权令牌获取。
 * 如果请求成功，它将返回JSON响应。
 * 如果请求失败，它将抛出一个错误，包含错误消息。
 * @param url       API的具体URL，例如 'http://localhost:5001/api/auth/login'
 * @param method    HTTP方法（GET, POST, PUT, DELETE）
 * @param data      请求的主体（对于POST和PUT请求）
 */
export default async function fetchFromAPI(url: string, method: string, data: string | null = null) {
  const token = User.getInstance()?.userAuth?.token || null;
  const result =  await fetch(url, {
    method: method,
    headers: {
      'accept': 'application/json',
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : '',
    },
    body: data ? data : undefined,
  });
  const resultData = await result.json();
  if (!result.ok) {
    throw new Error(resultData.msg);
  }
  return resultData;
}
