# NoticeService API 总结

以下是 NoticeService 项目的 API 列表，包含功能描述和返回内容，供前端开发参考。所有 API 需要 JWT 认证，返回 JSON 格式，基于当前用户 ID（从 JWT 获取）过滤数据。

## 1. GET /api/notifications
- **功能**：获取所有未读通知的总数和按类型（reply, like, repost, mention）的未读数。
- **返回内容**：
  - `totalUnread` (int)：未读通知总数。
  - `unreadByType` (object)：每类未读数，键为 "reply", "like", "repost", "mention"。
- **示例**：
  ```json
  {
    "totalUnread": 25,
    "unreadByType": {
      "reply": 5,
      "like": 10,
      "repost": 3,
      "mention": 7
    }
  }
  ```

## 2. POST /api/notifications/read
- **功能**：将指定类型的通知标记为已读。
- **请求参数**：
  - `type` (string)：通知类型（"reply", "like", "repost", "mention"）。
- **返回内容**：
  - 成功状态：`{ "success": true }`。
- **示例**：
  ```json
  { "success": true }
  ```

## 3. GET /api/notifications/likes?page={page}&pageSize={pageSize}
- **功能**：同时返回未读和已读点赞通知：
  - 未读：全量，按 `target_type` 和 `target_id` 分组，包含点赞数、最后点赞时间和点赞者 ID 摘要。
  - 已读：分页返回，分组，包含相同字段。
- **查询参数**：
  - `page` (int, 默认 1)：已读点赞页码。
  - `pageSize` (int, 默认 20)：已读点赞每页大小。
- **返回内容**：
  - `unread` (array)：未读点赞列表。
    - `targetId` (int)：目标 ID（帖子/评论）。
    - `targetType` (string)："post" 或 "comment"。
    - `lastLikedAt` (datetime)：最后点赞时间。
    - `likeCount` (int)：未读点赞数。
    - `likerIds` (array)：点赞者 ID（最多 10 个）。
  - `read` (object)：
    - `total` (int)：已读点赞分组总数。
    - `items` (array)：已读点赞列表（同未读字段）。
- **示例**：
  ```json
  {
    "unread": [
      {
        "targetId": 101,
        "targetType": "post",
        "lastLikedAt": "2025-08-20T10:00:00Z",
        "likeCount": 3,
        "likerIds": [123, 456, 789]
      }
    ],
    "read": {
      "total": 10,
      "items": [
        {
          "targetId": 303,
          "targetType": "post",
          "lastLikedAt": "2025-08-19T15:00:00Z",
          "likeCount": 2,
          "likerIds": [111, 222]
        }
      ]
    }
  }
  ```

## 4. GET /api/notifications/replies?page={page}&pageSize={pageSize}&unreadOnly={true/false}
- **功能**：获取回复通知，按创建时间倒序，支持仅未读或全部。
- **查询参数**：
  - `page` (int, 默认 1)：页码。
  - `pageSize` (int, 默认 20)：每页大小。
  - `unreadOnly` (bool, 默认 false)：是否仅返回未读。
- **返回内容**：
  - `total` (int)：回复总数。
  - `items` (array)：
    - `replyId` (int)：回复 ID。
    - `replyPoster` (int)：回复者用户 ID。
    - `contentPreview` (string)：回复内容摘要（前 50 字符）。
    - `createdAt` (datetime)：创建时间。
- **示例**：
  ```json
  {
    "total": 8,
    "items": [
      {
        "replyId": 1,
        "replyPoster": 123,
        "contentPreview": "Great post! Thanks...",
        "createdAt": "2025-08-20T08:00:00Z"
      }
    ]
  }
  ```

## 5. GET /api/notifications/reposts?page={page}&pageSize={pageSize}&unreadOnly={true/false}
- **功能**：获取转发通知，按创建时间倒序，支持仅未读或全部。
- **查询参数**：
  - `page` (int, 默认 1)：页码。
  - `pageSize` (int, 默认 20)：每页大小。
  - `unreadOnly` (bool, 默认 false)：是否仅返回未读。
- **返回内容**：
  - `total` (int)：转发总数。
  - `items` (array)：
    - `repostId` (int)：转发 ID。
    - `userId` (int)：转发者用户 ID。
    - `postId` (int)：原帖 ID。
    - `commentPreview` (string)：转发评论摘要（可为空）。
    - `createdAt` (datetime)：创建时间。
- **示例**：
  ```json
  {
    "total": 5,
    "items": [
      {
        "repostId": 1,
        "userId": 789,
        "postId": 101,
        "commentPreview": "Interesting post!...",
        "createdAt": "2025-08-20T09:00:00Z"
      }
    ]
  }
  ```

## 6. GET /api/notifications/mentions?page={page}&pageSize={pageSize}&unreadOnly={true/false}
- **功能**：获取提及通知，按创建时间倒序，支持仅未读或全部。
- **查询参数**：
  - `page` (int, 默认 1)：页码。
  - `pageSize` (int, 默认 20)：每页大小。
  - `unreadOnly` (bool, 默认 false)：是否仅返回未读。
- **返回内容**：
  - `total` (int)：提及总数。
  - `items` (array)：
    - `targetId` (int)：目标 ID（帖子/评论）。
    - `targetType` (string)："post" 或 "comment"。
    - `userId` (int)：提及者用户 ID。
    - `createdAt` (datetime)：创建时间。
- **示例**：
  ```json
  {
    "total": 7,
    "items": [
      {
        "targetId": 101,
        "targetType": "post",
        "userId": 234,
        "createdAt": "2025-08-20T10:15:00Z"
      }
    ]
  }
  ```

## 7. GET /api/notifications/likes/target?targetType={type}&targetId={id}&page={page}&pageSize={pageSize}
- **功能**：获取指定目标（帖子/评论）的所有点赞者用户 ID，分页返回。
- **查询参数**：
  - `targetType` (string, 必填)："post" 或 "comment"。
  - `targetId` (int, 必填)：目标 ID。
  - `page` (int, 默认 1)：页码。
  - `pageSize` (int, 默认 20)：每页大小。
- **返回内容**：
  - `total` (int)：点赞者总数。
  - `items` (array)：分页的点赞者用户 ID 列表。
- **示例**：
  ```json
  {
    "total": 50,
    "items": [123, 456, 789]
  }
  ```