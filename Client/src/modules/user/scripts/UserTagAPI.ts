import User from '@/modules/auth/scripts/User.ts'
import fetchFromAPI from '@/modules/user/scripts/FetchFromAPI.ts'

interface TagData{
  "tags": number[],
}


const BASE_URL = 'http://localhost:5003';

async function getTagsAPI(userId: string) {
  return await fetchFromAPI(`${BASE_URL}/api/users/${userId}/tags`, 'GET');
}

async function setTagsAPI(userId: string, tags: TagData) {
  return await fetchFromAPI(`${BASE_URL}/api/users/${userId}/tags`, 'PUT', JSON.stringify(tags));
}

async function addTagAPI(userId: string, tagId: string) {
  return await fetchFromAPI(`${BASE_URL}/api/users/${userId}/tags/${tagId}`, 'POST');
}

async function deleteTagAPI(userId: string, tagId: string) {
  return await fetchFromAPI(`${BASE_URL}/api/users/${userId}/tags/${tagId}`, 'DELETE');
}

export { getTagsAPI, setTagsAPI, addTagAPI, deleteTagAPI , type TagData };
