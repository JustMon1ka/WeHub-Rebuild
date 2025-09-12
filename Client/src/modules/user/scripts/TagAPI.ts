import { fetchFromAPI, GATEWAY} from '@/modules/core/public.ts'

/* API from tag service */

interface SingleTagData {
  tagId: string,
  tagName: string,
  count: number,
}

const BASE_URL = `${GATEWAY}/api/tags`;

async function  addTagsAPI(tagsName: Array<string>) {
  return await fetchFromAPI(`${BASE_URL}/add`, 'POST', JSON.stringify({tagsName: tagsName}));
}

async function getTagsNameAPI(tagId: Array<string>) {
  return await fetchFromAPI(`${BASE_URL}?ids=${tagId.toString()}`, 'GET');
}

async function getPopularTagsAPI() {
  return await fetchFromAPI(`${BASE_URL}/popular`, 'GET');
}

export { addTagsAPI, getTagsNameAPI, getPopularTagsAPI, type SingleTagData };
