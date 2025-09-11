
# UserTagService API 总结

以下是 UserTagService 项目的 API 列表，供前端开发参考。所有请求需身份认证。

---

## 1. GET /api/users/{id}/tags
- **功能**：获取用户的所有标签 ID 列表

---

## 2. PUT /api/users/{id}/tags
- **功能**：替换用户当前的标签列表
- **请求体**：
```json
{
  "tags": [1, 2, 3]
}
```

---

## 3. POST /api/users/{id}/tags/{tagId}
- **功能**：添加一个标签 ID

---

## 4. DELETE /api/users/{id}/tags/{tagId}
- **功能**：移除一个标签 ID
