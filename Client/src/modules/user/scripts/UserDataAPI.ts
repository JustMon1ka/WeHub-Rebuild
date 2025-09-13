import { fetchFromAPI, GATEWAY } from '@/modules/core/public.ts'

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



export { getUserDataAPI, setUserAuthDataAPI, setUserProfileAPI, deleteUserAPI,
  type userAuthData, type userProfileData, type userReadOnlyData, type UserData };
