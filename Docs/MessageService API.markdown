# MessageService API 总结

以下是 MessageService 项目的 API 列表，包含功能描述和返回内容，供前端开发参考。所有 API 需要 JWT 认证，返回 JSON 格式，基于当前用户 ID（从 JWT 的 Name/NameIdentifier 解析）过滤数据。

## 1. GET /api/messages
- **功能**：获取当前用户的会话列表（每个会话包含对端用户、最后一条消息和未读数量）。
- **返回内容**：
  - `OtherUserId` (int)：对端用户 ID。
  - `LastMessage` (object)：最后一条消息。
    - `MessageId` (int)
    - `SenderId` (int)
    - `ReceiverId` (int)
    - `Content` (string)
    - `SentAt` (datetime, UTC)
    - `IsRead` (bool)
  - `UnreadCount` (int)：该会话未读消息数。
- **示例**：
```json
[
  {
    "OtherUserId": 123,
    "LastMessage": {
      "MessageId": 1001,
      "SenderId": 123,
      "ReceiverId": 456,
      "Content": "Hi there!",
      "SentAt": "2025-08-20T10:00:00Z",
      "IsRead": false
    },
    "UnreadCount": 2
  }
]
```

## 2. GET /api/messages/{userId}
- **功能**：获取与指定用户的双向聊天记录（按时间顺序返回）。
- **路径参数**：
  - `userId` (int)：对端用户 ID。
- **返回内容**：消息数组：
  - `MessageId` (int)
  - `SenderId` (int)
  - `ReceiverId` (int)
  - `Content` (string)
  - `SentAt` (datetime, UTC)
  - `IsRead` (bool)
- **示例**：
```json
[
  {
    "MessageId": 1001,
    "SenderId": 456,
    "ReceiverId": 123,
    "Content": "Hello!",
    "SentAt": "2025-08-20T09:59:00Z",
    "IsRead": true
  },
  {
    "MessageId": 1002,
    "SenderId": 123,
    "ReceiverId": 456,
    "Content": "Hi there!",
    "SentAt": "2025-08-20T10:00:00Z",
    "IsRead": false
  }
]
```

## 3. POST /api/messages/{userId}
- **功能**：向指定用户发送一条消息。
- **路径参数**：
  - `userId` (int)：接收方用户 ID。
- **请求体**：
```json
{
  "content": "How are you?"
}
```
- **返回内容**：已创建的消息对象（同 MessageDto）。
- **返回状态**：201 Created，并在 Location 中指向 `GET /api/messages/{userId}`。
- **示例**：
```json
{
  "MessageId": 2001,
  "SenderId": 456,
  "ReceiverId": 123,
  "Content": "How are you?",
  "SentAt": "2025-08-20T10:05:00Z",
  "IsRead": false
}
```

## 4. PUT /api/messages/{userId}/read
- **功能**：将与指定用户的对话中，对方发给当前用户的消息标记为已读。
- **路径参数**：
  - `userId` (int)：对端用户 ID。
- **返回内容**：无内容。
- **返回状态**：204 No Content。

## 错误与认证
- 若未携带有效 JWT 或无法解析当前用户身份：返回 401 Unauthorized（控制器中判定 `User.Identity?.Name` 为空）。
- 服务器内部错误：返回 500，并包含错误信息字符串。
- 发送消息时请求体验证：`content` 为空时返回 400 Bad Request。

## 备注
- 所有时间字段为 UTC（由服务层以 `DateTime.UtcNow` 生成）。
- DTO 对应：`ConversationDto`、`MessageDto`、`SendMessageDto`；服务接口见 `IMessageService`。
