import fetchFromAPI from '@/modules/user/scripts/FetchFromAPI.ts'

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
  profileURL: string,
  avatarURL: string,
  nickname: string,
  bio: string,
  gender: string,
  birthday: string,
  location: string,
  experience: number,
  level: number,
}

interface UserData extends userAuthData, userProfileData, userReadOnlyData {}

const BASE_URL = 'http://localhost:5002';

async function getUserDataAPI(userId: string){
  return await fetchFromAPI(`${BASE_URL}/api/users/${userId}`, 'GET');
}

async function setUserAuthDataAPI(userId: string, userData: userAuthData) {
  return await fetchFromAPI(`${BASE_URL}/api/users/${userId}/user`, 'PUT', JSON.stringify(userData));
}

async function setUserProfileAPI(userId: string, userProfileData: userProfileData) {
  return await fetchFromAPI(`${BASE_URL}/api/users/${userId}/profile`, 'PUT', JSON.stringify(userProfileData));
}

async function deleteUserAPI(userId: string) {
  return await fetchFromAPI(`${BASE_URL}/api/users/${userId}/delete`, 'DELETE');
}


export { getUserDataAPI, setUserAuthDataAPI, setUserProfileAPI, deleteUserAPI,
  type userAuthData, type userProfileData, type userReadOnlyData, type UserData };
