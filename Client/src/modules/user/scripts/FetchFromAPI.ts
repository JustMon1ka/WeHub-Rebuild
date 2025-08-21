import User from '@/modules/auth/scripts/User.ts'

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
  if (!result.ok || resultData.code !== 200) {
    throw new Error(resultData.msg);
  }
  return resultData;
}
