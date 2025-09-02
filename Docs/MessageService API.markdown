# MessageService API 总结

以下是 MessageService 项目的 API 列表，包含功能描述、请求/响应与示例，供前端开发参考。所有 API 需要 JWT 认证，返回 JSON 格式。

## 1. GET /api/messages

- 功能：获取当前用户的会话列表（按最近消息排序）。
- 返回内容：
  - 数组，元素为会话对象：
    - `otherUserId` (int)：会话对方用户 ID。
    - `lastMessage` (object)：最近一条消息。
      - `messageId` (int)：消息 ID。
      - `senderId` (int)：发送者用户 ID。
      - `receiverId` (int)：接收者用户 ID。
      - `content` (string)：消息内容。
      - `sentAt` (datetime)：发送时间（ISO8601）。
      - `isRead` (bool)：是否已读。
    - `unreadCount` (int)：与该用户之间的未读消息数量。
- 示例：
```json
[
  {
    "otherUserId": 123,
    "lastMessage": {
      "messageId": 1001,
      "senderId": 123,
      "receiverId": 456,
      "content": "你好，最近怎么样？",
      "sentAt": "2025-09-01T14:30:00Z",
      "isRead": false
    },
    "unreadCount": 3
  }
]
```

## 2. GET /api/messages/{userId}

- 功能：获取与指定用户的聊天记录，按时间排序。
- 路径参数：
  - `userId` (int)：对方用户 ID。
- 返回内容：
  - 数组，元素为消息对象：
    - `messageId` (int)
    - `senderId` (int)
    - `receiverId` (int)
    - `content` (string)
    - `sentAt` (datetime)
    - `isRead` (bool)
- 示例：
```json
[
  {
    "messageId": 1001,
    "senderId": 123,
    "receiverId": 456,
    "content": "你好，最近怎么样？",
    "sentAt": "2025-09-01T14:30:00Z",
    "isRead": false
  },
  {
    "messageId": 1003,
    "senderId": 456,
    "receiverId": 123,
    "content": "还不错，工作比较忙",
    "sentAt": "2025-09-01T14:35:00Z",
    "isRead": false
  }
]
```

## 3. POST /api/messages/{userId}

- 功能：向指定用户发送一条消息。
- 路径参数：
  - `userId` (int)：接收者用户 ID。
- 请求体：
  - `content` (string, 必填)：消息内容。
- 返回内容：
  - 新创建的消息对象（Created 201）：
    - `messageId` (int)
    - `senderId` (int)
    - `receiverId` (int)
    - `content` (string)
    - `sentAt` (datetime)
    - `isRead` (bool)
- 示例：
请求：
```json
{ "content": "你好，这是一条新消息" }
```
响应：
```json
{
  "messageId": 1005,
  "senderId": 456,
  "receiverId": 123,
  "content": "你好，这是一条新消息",
  "sentAt": "2025-09-01T15:00:00Z",
  "isRead": false
}
```

## 4. PUT /api/messages/{userId}/read

- 功能：将与指定用户的未读消息全部标记为已读。
- 路径参数：
  - `userId` (int)：对方用户 ID。
- 返回内容：
  - HTTP 204 No Content（无响应体）。

## 5. 错误响应规范

- 认证失败：`401 Unauthorized`，响应体为字符串消息（例如：`"用户未认证"`）。
- 参数校验失败：`400 Bad Request`，响应体为字符串消息（例如：`"消息内容不能为空"`）。
- 服务器错误：`500 Internal Server Error`，响应体为字符串消息（例如：`"发送消息失败: <错误信息>"`）。

## 备注

- 当前未采用统一的 `{ code, msg, data }` 包装，控制器直接返回 DTO 或状态码。
- 如果需要统一响应壳，可在控制器层改为返回 `BaseHttpResponse<T>` 并在前端适配。


