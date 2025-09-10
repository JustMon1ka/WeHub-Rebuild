import { GATEWAY } from '@/modules/core/public.ts'

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

const BASE_URL : string = `${GATEWAY}/api/auth`;

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
  return await fetchFromAPI(`${BASE_URL}/register`, 'POST', JSON.stringify(userData));
}

async function loginAPI(userData: loginData) {
  return await fetchFromAPI(`${BASE_URL}/login`, 'POST', JSON.stringify(userData));
}

async function refreshTokenAPI(token: string) {
  return await fetchFromAPI(`${BASE_URL}/refresh-token`, 'GET', undefined, token);
}

async function meAPI(token: string) {
  return await fetchFromAPI(`${BASE_URL}/me`, 'GET', undefined, token);
}

async function sendCodeAPI(email: string) {
  return await fetchFromAPI(`${BASE_URL}/send-code-email`, 'POST', JSON.stringify({ email: email }));
}

async function verifyCodeAPI(data: codeVerifyData) {
  return await fetchFromAPI(`${BASE_URL}/login-email-code`, 'POST', JSON.stringify(data));
}

export { registerAPI, loginAPI, refreshTokenAPI , meAPI, sendCodeAPI, verifyCodeAPI , type registerData, type loginData };
