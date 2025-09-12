import { fetchFromAPI, GATEWAY} from '@/modules/core/public.ts'

/* API from user tag service */

interface TagData{
  "tags": number[],
}

const BASE_URL = `${GATEWAY}/api/user_tags`;

async function getTagsAPI(userId: string) {
  return await fetchFromAPI(`${BASE_URL}/${userId}`, 'GET');
}

async function setTagsAPI(userId: string, tags: TagData) {
  return await fetchFromAPI(`${BASE_URL}/${userId}`, 'PUT', JSON.stringify(tags));
}

async function addUserTagAPI(userId: string, tagId: string) {
  return await fetchFromAPI(`${BASE_URL}/${userId}/${tagId}`, 'POST');
}

async function deleteUserTagAPI(userId: string, tagId: string) {
  return await fetchFromAPI(`${BASE_URL}/${userId}/${tagId}`, 'DELETE');
}

export { getTagsAPI, setTagsAPI, addUserTagAPI, deleteUserTagAPI , type TagData };
