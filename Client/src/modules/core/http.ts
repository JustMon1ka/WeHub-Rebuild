/* eslint-disable @typescript-eslint/no-explicit-any */
import * as axios from "axios";

const http = axios.create({
    baseURL: "/",       // 前端统一走 /api → 交给 Vite 代理到后端（见 vite.config.ts）
    timeout: 15000,
});

function getToken(): string | null {
    // 和你们登录存 token 的方式保持一致（如 localStorage.setItem('token', xxx)）
    return localStorage.getItem("token");
}

http.interceptors.request.use((config) => {
    const token = getToken();
    if (token) {
        config.headers = config.headers ?? {};
        (config.headers as any).Authorization = `Bearer ${token}`;
    }
    return config;
});

export default http;
