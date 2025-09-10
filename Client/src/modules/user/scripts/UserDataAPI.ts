import { fetchFromAPI, GATEWAY } from '@/modules/core/public.ts'
import User from '@/modules/auth/scripts/User.ts'

interface userReadOnlyData {
  userId: string,
  createdAt: string,
  status: string,
}

interface userAuthData {
  username: string,
  password: string,
  email: string,
  phone: string,
}

interface userProfileData{
  profileUrl: string,
  avatarUrl: string,
  nickname: string,
  bio: string,
  gender: string,
  birthday: string,
  location: string,
  experience: number,
  level: number,
}

interface UserData extends userAuthData, userProfileData, userReadOnlyData {}

const BASE_URL : string = `${GATEWAY}/api/user_data`;

/* API from user data service */

async function getUserDataAPI(userId: string){
  return await fetchFromAPI(`${BASE_URL}/${userId}`, 'GET');
}

async function setUserAuthDataAPI(userId: string, userData: userAuthData) {
  return await fetchFromAPI(`${BASE_URL}/${userId}/user`, 'PUT', JSON.stringify(userData));
}

async function setUserProfileAPI(userId: string, userProfileData: userProfileData) {
  return await fetchFromAPI(`${BASE_URL}/${userId}/profile`, 'PUT', JSON.stringify(userProfileData));
}

async function deleteUserAPI(userId: string) {
  return await fetchFromAPI(`${BASE_URL}/${userId}/delete`, 'DELETE');
}

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

export { getUserDataAPI, setUserAuthDataAPI, setUserProfileAPI, deleteUserAPI, uploadMediaAPI, MEDIA_BASE_URL,
  type userAuthData, type userProfileData, type userReadOnlyData, type UserData };
