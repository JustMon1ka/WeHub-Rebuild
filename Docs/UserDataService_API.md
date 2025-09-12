
# UserDataService API 总结

以下是 UserDataService 项目的 API 列表，供前端开发参考。所有接口需 JWT 鉴权，用户 ID 从 token 中获得。

---

## 1. GET /api/users/{id}
- **功能**：获取指定用户的基本信息

---

## 2. PUT /api/users/{id}/user
- **功能**：更新用户主表信息（用户名、邮箱、手机号、密码）
- **请求体**：
```json
{
  "username": "newname",
  "password": "newpass",
  "email": "new@example.com",
  "phone": "18888888888"
}
```

---

## 3. PUT /api/users/{id}/profile
- **功能**：更新扩展资料（昵称、性别、简介等）
- **请求体**：
```json
{
  "nickname": "测试用户",
  "bio": "我喜欢编程",
  "gender": 1
}
```

---

## 4. DELETE /api/users/{id}/delete
- **功能**：注销当前用户，删除其所有相关数据
