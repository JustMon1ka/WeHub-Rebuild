import { fetchFromAPI, GATEWAY } from '@/modules/core/public.ts'

interface FollowData {
  followerId : number,
  followeeId : number,
  createdAt : string,
}

const BASE_URL = `${GATEWAY}/api/Follows`;

async function addFollowingAPI(userId: string){
  return await fetchFromAPI(`${BASE_URL}`, 'POST', JSON.stringify({ followeeId: userId }));
}

async function removeFollowingAPI(following: string){
  return await fetchFromAPI(`${BASE_URL}/${following}`, 'DELETE');
}

async function getFollowCountAPI(userId: string){
  return await fetchFromAPI(`${BASE_URL}/count?userId=${userId}`, 'GET');
}

async function getFollowingAPI(page: number, pageSize: number, userId: string){
  return await fetchFromAPI(`${BASE_URL}/following?page=${page}&pageSize=${pageSize}&userId=${userId}`, 'GET');
}

async function getFollowerAPI(page: number, pageSize: number, userId: string){
  return await fetchFromAPI(`${BASE_URL}/followers?page=${page}&pageSize=${pageSize}&userId=${userId}`, 'GET');
}

export {
  addFollowingAPI,
  removeFollowingAPI,
  getFollowCountAPI,
  getFollowingAPI,
  getFollowerAPI,
  type FollowData
}
