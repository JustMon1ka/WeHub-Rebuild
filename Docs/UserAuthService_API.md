
# UserAuthService API 总结

以下是 UserAuthService 项目的 API 列表，供前端开发参考。所有响应均为 JSON 格式，支持统一的响应封装结构：

```json
{
  "code": 200,
  "msg": "OK",
  "data": ...
}
```

---

## 1. POST /api/auth/register
- **功能**：注册新用户（用户名、密码、邮箱、手机号）
- **请求体**：
```json
{
  "username": "john",
  "password": "123456",
  "email": "john@example.com",
  "phone": "13800000000"
}
```
- **返回**：注册成功或失败的提示信息

---

## 2. POST /api/auth/login
- **功能**：用户名 / 手机号 / 邮箱 登录，返回 JWT Token
- **请求体**：
```json
{
  "identifier": "john",
  "password": "123456"
}
```
- **返回**：
```json
{
  "code": 200,
  "msg": "OK",
  "data": "eyJhbGciOi..." // JWT Token
}
```

---

## 3. GET /api/auth/me
- **功能**：获取当前登录用户的基本信息（从 token 中解析）
- **认证**：需要 Bearer Token

---

## 4. POST /api/auth/send-code-email
- **功能**：向已注册邮箱发送验证码，用于验证码登录

---

## 5. POST /api/auth/login-email-code
- **功能**：通过邮箱 + 验证码登录
- **请求体**：
```json
{
  "email": "john@example.com",
  "code": "123456"
}
```
- **返回**：JWT Token
