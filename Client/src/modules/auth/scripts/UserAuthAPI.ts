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

async function fetchFromAPI(url: string, method: string, data: string | undefined = undefined, token: string | null = null) {
  const result =  await fetch(url, {
    method: method,
    headers: {
      'accept': 'application/json',
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : '',
    },
    body: data,
  });
  const resultData = await result.json();
  if (!result.ok) {
    throw new Error(resultData.msg);
  }
  return resultData;
}

async function registerAPI(userData: registerData) {
  return await fetchFromAPI(`${BASE_URL}/api/auth/register`, 'POST', JSON.stringify(userData));
}

async function loginAPI(userData: loginData) {
  return await fetchFromAPI(`${BASE_URL}/api/auth/login`, 'POST', JSON.stringify(userData));
}

async function refreshTokenAPI(token: string) {
  return await fetchFromAPI(`${BASE_URL}/api/auth/refresh-token`, 'GET', undefined, token);
}

async function meAPI(token: string) {
  return await fetchFromAPI(`${BASE_URL}/api/auth/me`, 'GET', undefined, token);
}

async function sendCodeAPI(email: string) {
  return await fetchFromAPI(`${BASE_URL}/api/auth/send-code-email`, 'POST', JSON.stringify({ email: email }));
}

async function verifyCodeAPI(data: codeVerifyData) {
  return await fetchFromAPI(`${BASE_URL}/api/auth/login-email-code`, 'POST', JSON.stringify(data));
}

export { registerAPI, loginAPI, refreshTokenAPI , meAPI, sendCodeAPI, verifyCodeAPI , type registerData, type loginData };
