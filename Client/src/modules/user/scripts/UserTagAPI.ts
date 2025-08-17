import User from '@/modules/auth/scripts/User.ts'

interface TagData{
  "tags": string[],
}


const BASE_URL = 'http://localhost:5003';

async function connect(url: string, method: string, data: string | null = null) {
  const token = User.getInstance()?.userAuth?.token || null;
  return await fetch(url, {
    method: method,
    headers: {
      'accept': 'application/json',
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : '',
    },
    body: data ? data : undefined,
  });
}

async function getTagsAPI(userId: string) {
  const response = await connect(`${BASE_URL}/api/users/${userId}/tags`, 'GET');
  if (response.status === 401) {
    return "Unauthorized";
  }
  if (!response.ok) {
    return "Network Error";
  }
  return await response.json();
}

async function setTagsAPI(userId: string, tags: TagData) {
  const response = await connect(`${BASE_URL}/api/users/${userId}/tags`, 'PUT', JSON.stringify(tags));
  if (response.status === 401) {
    return "Unauthorized";
  }
  if (!response.ok) {
    return "Network Error";
  }
  return await response.json();
}

export { getTagsAPI, setTagsAPI, type TagData };
