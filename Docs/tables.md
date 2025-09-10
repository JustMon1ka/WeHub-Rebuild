一、用户与认证模块
1. 用户表 User
字段名	类型	描述
user_id	INT PK	用户ID（主键）
username	VARCHAR	用户名
password_hash	VARCHAR	加密密码
email	VARCHAR	邮箱
phone	VARCHAR	手机号
created_at	DATETIME	注册时间
status	TINYINT	状态（正常/封禁）
2. 用户信息表 UserProfile
字段名	类型	描述
user_id	INT PK/FK	外键，关联用户
avatar_url	VARCHAR	头像链接
bio	TEXT	简介
gender	ENUM	性别
birthday	DATE	生日
location	VARCHAR	所在地
experience	INT	经验值
level	INT	等级
3. 用户标签表 UserTag
字段名	类型	描述
user_id	INT FK	用户ID
tag_id	INT FK	标签ID
PRIMARY KEY(user_id, tag_id)		
二、内容与互动模块
4. 帖子表 Post
字段名	类型	描述
post_id	INT PK	帖子ID
user_id	INT FK	作者ID
content	TEXT	文本内容
created_at	DATETIME	发布时间
is_deleted	BOOLEAN	是否被删除
is_hidden	BOOLEAN	是否被隐藏
views	INT	浏览量
likes	INT	点赞数
dislikes	INT	点踩数
5. 媒体文件表 Media
字段名	类型	描述
media_id	INT PK	媒体ID
post_id	INT FK	所属帖子ID
media_type	ENUM	类型（图片/视频）
media_url	VARCHAR	链接
6. 帖子标签关联表 PostTag
字段名	类型	描述
post_id	INT FK	帖子ID
tag_id	INT FK	标签ID
PRIMARY KEY(post_id, tag_id)		
7. 标签表 Tag
字段名	类型	描述
tag_id	INT PK	标签ID
tag_name	VARCHAR	标签名称
8. 评论表 Comment
字段名	类型	描述
comment_id	INT PK	评论ID
post_id	INT FK	所属帖子
user_id	INT FK	评论者ID
content	TEXT	评论内容
created_at	DATETIME	评论时间
likes	INT	点赞数
dislikes	INT	点踩数
is_deleted	BOOLEAN	是否被删除
9.	回复表 Reply
字段名	类型	描述
reply_id	INT PK	评论ID
comment_id	INT FK	所属评论
reply_poster	INT FK	发布者ID
content	TEXT	评论内容
created_at	DATETIME	评论时间
is_deleted	BOOLEAN	是否被删除
10. 点赞表 Like
字段名	类型	描述
user_id	INT FK	用户ID
target_type	ENUM	类型（帖子/评论）
target_id	INT FK	目标ID
PRIMARY KEY(user_id, target_id)		
11. 转发表 Repost
字段名	类型	描述
repost_id	INT PK	转发ID
user_id	INT FK	转发者
post_id	INT FK	原帖ID
comment	TEXT	附加评论
created_at	DATETIME	转发时间
三、社交与消息模块
12. 关注表 Follow
字段名	类型	描述
follower_id	INT FK	关注者ID
followee_id	INT FK	被关注者ID
created_at	DATETIME	关注时间
PRIMARY KEY(follower_id, followee_id)		
13. 私信表 Message
字段名	类型	描述
message_id	INT PK	消息ID
sender_id	INT FK	发送者
receiver_id	INT FK	接收者
content	TEXT	消息内容
sent_at	DATETIME	发送时间
is_read	BOOLEAN	是否已读
14. 举报表 Report
字段名	类型	描述
report_id	INT PK	举报ID
reporter_id	INT FK	举报人ID
target_type	ENUM	类型（post/comment/user）
target_id	INT	被举报内容ID
reason	TEXT	举报理由
reported_at	DATETIME	举报时间

四、圈子与活动模块
15. 圈子表 Circle
字段名	类型	描述
circle_id	INT PK	圈子ID
name	VARCHAR	名称
description	TEXT	描述
owner_id	INT FK	创建者ID
created_at	DATETIME	创建时间
16. 圈子成员表 CircleMember
字段名	类型	描述
circle_id	INT FK	圈子ID
user_id	INT FK	用户ID
role	ENUM	角色（成员/管理员）
status	ENUM	状态（待审/已审）
points	INT	积分
PRIMARY KEY(circle_id, user_id)		
17. 活动表 Activity
字段名	类型	描述
activity_id	INT PK	活动ID
circle_id	INT FK	所属圈子
title	VARCHAR	活动标题
description	TEXT	描述
reward	VARCHAR	奖励
start_time	DATETIME	开始时间
end_time	DATETIME	结束时间
五、用户成长与互动模块
18. 投票表 Vote
字段名	类型	描述
vote_id	INT PK	投票ID
creator_id	INT FK	发起者ID
topic	VARCHAR	投票主题
created_at	DATETIME	创建时间
deadline	DATETIME	截止时间

19. 投票选项表 VoteOption
字段名	类型	描述
option_id	INT PK	选项ID
vote_id	INT FK	所属投票ID
content	VARCHAR	选项内容
20. 投票参与表 VoteRecord
字段名	类型	描述
user_id	INT FK	用户ID
option_id	INT FK	投票选项ID
PRIMARY KEY(user_id, option_id)		
六、后台管理模块
21. 管理员操作日志 AdminLog
字段名	类型	描述
log_id	INT PK	日志ID
admin_id	INT FK	管理员ID
action	TEXT	操作内容
target_type	VARCHAR	目标类型
target_id	INT	操作目标ID
created_at	DATETIME	操作时间
22. 管理员Admin
字段名	类型	描述
admin_id	INT PK	管理员id
admin_name	VARCHAR	管理员名称

