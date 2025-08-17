
interface registerData {
  "username": string,
  "password": string,
  "email": string,
  "code": string,
  "phone": string,
}

interface loginData {
  "identifier": string,
  "password": string,
}

interface codeVerifyData {
  "email": string,
  "code": string,
}

const BASE_URL = 'http://localhost:5001';

async function connect(url: string, method: string, data: string | null, token: string | null = null) {
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

async function registerAPI(userData: registerData) {
  const response = await connect(`${BASE_URL}/api/auth/register`, 'POST', JSON.stringify(userData));
  if (response.status !== 401 &&!response.ok) {
    console.log(response.status, response.statusText);
    throw new Error("Network Error");
  }
  return await response.json();
}

async function loginAPI(userData: loginData) {
  const response = await connect(`${BASE_URL}/api/auth/login`, 'POST', JSON.stringify(userData));
  if (response.status !== 401 && !response.ok) {
    return "Network Error";
  }
  return await response.json();
}

async function meAPI(token: string) {
  const response = await connect(`${BASE_URL}/api/auth/me`, 'GET', null, token);
  if (!response.ok) {
    return "Network Error";
  }
  if (response.status === 401) {
    return "Unauthorized";
  }
  return await response.json();
}

export { registerAPI, loginAPI, meAPI, type registerData, type loginData };
