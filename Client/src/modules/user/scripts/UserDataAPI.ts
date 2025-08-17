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

async function getUserDataAPI(userId: string){
  const response = await connect(`${BASE_URL}/api/users/${userId}`, 'GET');
  if (response.status !== 401 &&!response.ok) {
    throw new Error("Network Error");
  }
  return await response.json();
}

async function setUserDataAPI(userId: string, userData: userAuthData) {
  const response = await connect(`${BASE_URL}/api/users/${userId}/user`, 'PUT', JSON.stringify(userData));
  if (response.status === 401) {
    return "Unauthorized";
  }
  if (!response.ok) {
    return "Network Error";
  }
  return await response.json();
}

async function setUserProfileAPI(userId: string, userProfileData: userProfileData) {
  const response = await connect(`${BASE_URL}/api/users/${userId}/profile`, 'PUT', JSON.stringify(userProfileData));
  if (response.status === 401) {
    return "Unauthorized";
  }
  if (!response.ok) {
    return "Network Error";
  }
  return await response.json();
}

async function deleteUserAPI(userId: string) {
  const response = await connect(`${BASE_URL}/api/users/${userId}/delete`, 'DELETE');
  if (response.status === 401) {
    return "Unauthorized";
  }
  if (!response.ok) {
    return "Network Error";
  }
  return await response.json();
}


export { getUserDataAPI, setUserDataAPI, setUserProfileAPI, deleteUserAPI,
  type userAuthData, type userProfileData, type userReadOnlyData, type UserData };
