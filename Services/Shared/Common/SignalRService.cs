#nullable enable
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SignalRService
{
    /// <summary>
    /// 通知类型枚举，定义支持的通知类型
    /// </summary>
    public enum NotificationType
    {
        Like,       // 点赞通知
        Comment,    // 评论通知
        Follow,     // 关注通知
        Mention,    // 提及通知
        Retweet,    // 转发通知
        Chat,       // 私信通知
        Group       // 群聊通知
    }

    /// <summary>
    /// 实体类型常量，定义业务实体的类型
    /// </summary>
    public static class EntityTypes
    {
        /// <summary>
        /// 帖子类型实体
        /// </summary>
        public const string Post = "post";

        /// <summary>
        /// 评论类型实体
        /// </summary>
        public const string Comment = "comment";
    }

    /// <summary>
    /// 表示系统内的通知消息实体
    /// </summary>
    /// <remarks>
    /// <para>使用场景：</para>
    /// <list type="bullet">
    ///   <item>用户间私信（Chat）</item>
    ///   <item>社交互动通知（Like/Comment/Follow/Mention/Retweet）</item>
    ///   <item>群聊通知（通过 ReceiverId 表示群组 ID）</item>
    /// </list>
    /// </remarks>
    public class Notification
    {
        /// <summary>
        /// 发送方用户 ID，默认为空字符串
        /// </summary>
        public string SenderId { get; set; } = string.Empty;

        /// <summary>
        /// 接收方用户 ID 或群组 ID，默认为空字符串
        /// </summary>
        /// <remarks>
        /// <para>对于点对点通知，表示目标用户 ID</para>
        /// <para>对于群聊通知，表示群组 ID</para>
        /// </remarks>
        public string ReceiverId { get; set; } = string.Empty;

        /// <summary>
        /// 通知类型
        /// </summary>
        /// <seealso cref="NotificationType"/>
        public NotificationType Type { get; set; }

        /// <summary>
        /// 关联的业务实体 ID（可选）
        /// </summary>
        /// <remarks>
        /// <para>适用场景：</para>
        /// <list type="bullet">
        ///   <item>点赞/评论的帖子 ID</item>
        ///   <item>回复的评论 ID</item>
        ///   <item>空值表示无关联实体（如私信或群聊通知）</item>
        /// </list>
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? EntityId { get; set; }

        /// <summary>
        /// 关联的业务实体类型（可选）
        /// </summary>
        /// <remarks>
        /// <para>应使用 <see cref="EntityTypes"/> 中定义的常量值</para>
        /// </remarks>
        /// <seealso cref="EntityTypes"/>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? EntityType { get; set; }

        /// <summary>
        /// 通知创建时间，自动初始化为当前 UTC 时间
        /// </summary>
        /// <remarks>
        /// 使用 <see cref="DateTime.UtcNow"/> 初始化，避免时区问题
        /// </remarks>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// SignalR Hub，用于处理点对点和群组通知
    /// </summary>
    public class NotificationHub : Hub
    {
        /// <summary>
        /// 发送点对点通知
        /// </summary>
        /// <param name="notification">通知消息实体</param>
        /// <returns>表示异步操作的任务</returns>
        /// <remarks>
        /// <para>通过 SignalR 将通知推送给目标用户的客户端</para>
        /// <para>客户端需监听 "ReceiveNotification" 消息</para>
        /// <para>示例：</para>
        /// <code>
        /// var notification = new Notification
        /// {
        ///     SenderId = "user123",
        ///     ReceiverId = "user456",
        ///     Type = NotificationType.Like,
        ///     EntityId = 1001,
        ///     EntityType = EntityTypes.Post
        /// };
        /// await Clients.User(notification.ReceiverId).SendAsync("ReceiveNotification", notification);
        /// </code>
        /// </remarks>
        /// <exception cref="ArgumentNullException">当 notification 为 null 时抛出</exception>
        public async Task SendNotification(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);
            await Clients.User(notification.ReceiverId).SendAsync("ReceiveNotification", notification);
        }

        /// <summary>
        /// 加入群组
        /// </summary>
        /// <param name="groupId">群组 ID</param>
        /// <returns>表示异步操作的任务</returns>
        /// <remarks>
        /// 将当前连接加入指定群组，允许接收群组通知
        /// </remarks>
        /// <exception cref="ArgumentNullException">当 groupId 为 null 或空时抛出</exception>
        public async Task JoinGroup(string groupId)
        {
            ArgumentNullException.ThrowIfNull(groupId);
            if (string.IsNullOrEmpty(groupId))
                throw new ArgumentNullException(nameof(groupId));

            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        }

        /// <summary>
        /// 离开群组
        /// </summary>
        /// <param name="groupId">群组 ID</param>
        /// <returns>表示异步操作的任务</returns>
        /// <remarks>
        /// 从指定群组中移除当前连接，停止接收群组通知
        /// </remarks>
        /// <exception cref="ArgumentNullException">当 groupId 为 null 或空时抛出</exception>
        public async Task LeaveGroup(string groupId)
        {
            ArgumentNullException.ThrowIfNull(groupId);
            if (string.IsNullOrEmpty(groupId))
                throw new ArgumentNullException(nameof(groupId));

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        }

        /// <summary>
        /// 发送群组通知
        /// </summary>
        /// <param name="groupId">目标群组 ID</param>
        /// <param name="notification">通知消息实体</param>
        /// <returns>表示异步操作的任务</returns>
        /// <remarks>
        /// <para>向指定群组的所有成员推送通知</para>
        /// <para>客户端需监听 "ReceiveNotification" 消息</para>
        /// <para>示例：</para>
        /// <code>
        /// var notification = new Notification
        /// {
        ///     SenderId = "user123",
        ///     ReceiverId = "group123",
        ///     Type = NotificationType.Chat,
        ///     EntityId = null,
        ///     EntityType = null
        /// };
        /// await Clients.Group("group123").SendAsync("ReceiveNotification", notification);
        /// </code>
        /// </remarks>
        /// <exception cref="ArgumentNullException">当 notification 或 groupId 为 null 时抛出</exception>
        public async Task SendGroupNotification(string groupId, Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);
            ArgumentNullException.ThrowIfNull(groupId);
            if (string.IsNullOrEmpty(groupId))
                throw new ArgumentNullException(nameof(groupId));

            await Clients.Group(groupId).SendAsync("ReceiveNotification", notification);
        }
    }

    /// <summary>
    /// 通知服务，负责处理点对点和群组通知的发送与订阅
    /// </summary>
    /// <param name="hubContext">SignalR Hub 上下文</param>
    public class NotificationService(IHubContext<NotificationHub> hubContext)
    {
        private readonly IHubContext<NotificationHub> _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        private readonly Dictionary<string, HashSet<NotificationType>> _subscriptions = new();

        /// <summary>
        /// 异步发送点对点通知
        /// </summary>
        /// <param name="senderId">发送用户的唯一标识符</param>
        /// <param name="receiverId">接收用户的唯一标识符</param>
        /// <param name="type">通知类型</param>
        /// <param name="entityId">关联的业务实体 ID（可选）</param>
        /// <param name="entityType">关联的业务实体类型（可选）</param>
        /// <returns>表示异步操作的任务</returns>
        /// <remarks>
        /// <para>功能：</para>
        /// <list type="number">
        ///   <item><description>构造 Notification 对象</description></item>
        ///   <item><description>检查用户是否订阅了指定通知类型</description></item>
        ///   <item><description>通过 SignalR 推送给在线用户</description></item>
        /// </list>
        /// <para>示例：</para>
        /// <code>
        /// await SendNotificationAsync("user123", "user456", NotificationType.Like, 1001, EntityTypes.Post);
        /// </code>
        /// </remarks>
        /// <exception cref="ArgumentNullException">当 senderId 或 receiverId 为 null 或空时抛出</exception>
        /// <exception cref="InvalidOperationException">当通知发送失败时抛出</exception>
        public async Task SendNotificationAsync(
            string senderId,
            string receiverId,
            NotificationType type,
            int? entityId = null,
            string? entityType = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(senderId);
            ArgumentNullException.ThrowIfNullOrEmpty(receiverId);

            try
            {
                var notification = new Notification
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Type = type,
                    EntityId = entityId,
                    EntityType = entityType
                };

                // 仅向订阅了该类型通知的用户发送
                if (IsSubscribed(receiverId, type))
                {
                    await _hubContext.Clients.User(receiverId).SendAsync("ReceiveNotification", notification);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("通知发送失败", ex);
            }
        }

        /// <summary>
        /// 异步发送群组通知
        /// </summary>
        /// <param name="senderId">发送用户的唯一标识符</param>
        /// <param name="groupId">目标群组 ID</param>
        /// <param name="type">通知类型</param>
        /// <param name="entityId">关联的业务实体 ID（可选）</param>
        /// <param name="entityType">关联的业务实体类型（可选）</param>
        /// <returns>表示异步操作的任务</returns>
        /// <remarks>
        /// <para>功能：</para>
        /// <list type="number">
        ///   <item><description>构造 Notification 对象</description></item>
        ///   <item><description>通过 SignalR 推送给群组成员</description></item>
        /// </list>
        /// <para>示例：</para>
        /// <code>
        /// await SendGroupNotificationAsync("user123", "group123", NotificationType.Chat, null, null);
        /// </code>
        /// </remarks>
        /// <exception cref="ArgumentNullException">当 senderId 或 groupId 为 null 或空时抛出</exception>
        /// <exception cref="InvalidOperationException">当通知发送失败时抛出</exception>
        public async Task SendGroupNotificationAsync(
            string senderId,
            string groupId,
            NotificationType type,
            int? entityId = null,
            string? entityType = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(senderId);
            ArgumentNullException.ThrowIfNullOrEmpty(groupId);

            try
            {
                var notification = new Notification
                {
                    SenderId = senderId,
                    ReceiverId = groupId,
                    Type = type,
                    EntityId = entityId,
                    EntityType = entityType
                };

                await _hubContext.Clients.Group(groupId).SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("群组通知发送失败", ex);
            }
        }

        /// <summary>
        /// 异步订阅用户的通知
        /// </summary>
        /// <param name="userId">目标用户 ID</param>
        /// <param name="type">订阅的通知类型</param>
        /// <param name="cancellationToken">用于取消订阅的令牌</param>
        /// <returns>持续运行的任务，直到取消令牌被触发</returns>
        /// <remarks>
        /// <para>功能：</para>
        /// <list type="number">
        ///   <item><description>记录用户订阅的指定通知类型</description></item>
        ///   <item><description>确保后续通知按类型过滤推送</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">当 userId 为 null 或空时抛出</exception>
        public async Task SubscribeAsync(
            string userId,
            NotificationType type,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userId);

            try
            {
                lock (_subscriptions)
                {
                    if (!_subscriptions.TryGetValue(userId, out var types))
                    {
                        types = new HashSet<NotificationType> { type };
                        _subscriptions[userId] = types;
                    }
                    else
                    {
                        types.Add(type);
                    }
                }

                // 保持订阅活跃，直到取消
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(5000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                lock (_subscriptions)
                {
                    if (_subscriptions.TryGetValue(userId, out var types))
                    {
                        types.Remove(type);
                        if (types.Count == 0)
                            _subscriptions.Remove(userId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"订阅异常: {ex.Message}");
                await Task.Delay(1000, cancellationToken);
            }
        }

        /// <summary>
        /// 异步取消用户的所有订阅
        /// </summary>
        /// <param name="userId">目标用户 ID</param>
        /// <returns>表示异步操作的任务</returns>
        /// <remarks>
        /// 在用户离线或显式取消订阅时清理其订阅记录
        /// </remarks>
        /// <exception cref="ArgumentNullException">当 userId 为 null 或空时抛出</exception>
        public async Task UnsubscribeAsync(string userId)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userId);

            lock (_subscriptions)
            {
                _subscriptions.Remove(userId);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 异步订阅群组通知
        /// </summary>
        /// <param name="userId">目标用户 ID</param>
        /// <param name="groupId">目标群组 ID</param>
        /// <param name="cancellationToken">用于取消订阅的令牌</param>
        /// <returns>表示异步操作的任务</returns>
        /// <remarks>
        /// 将用户加入指定群组，允许接收群组通知
        /// </remarks>
        /// <exception cref="ArgumentNullException">当 userId 或 groupId 为 null 或空时抛出</exception>
        public async Task SubscribeToGroupAsync(
            string userId,
            string groupId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userId);
            ArgumentNullException.ThrowIfNullOrEmpty(groupId);

            await _hubContext.Groups.AddToGroupAsync(userId, groupId, cancellationToken);
        }

        /// <summary>
        /// 异步取消群组订阅
        /// </summary>
        /// <param name="userId">目标用户 ID</param>
        /// <param name="groupId">目标群组 ID</param>
        /// <param name="cancellationToken">用于取消操作的令牌</param>
        /// <returns>表示异步操作的任务</returns>
        /// <remarks>
        /// 从指定群组中移除用户，停止接收群组通知
        /// </remarks>
        /// <exception cref="ArgumentNullException">当 userId 或 groupId 为 null 或空时抛出</exception>
        public async Task UnsubscribeFromGroupAsync(
            string userId,
            string groupId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userId);
            ArgumentNullException.ThrowIfNullOrEmpty(groupId);

            await _hubContext.Groups.RemoveFromGroupAsync(userId, groupId, cancellationToken);
        }

        /// <summary>
        /// 检查用户是否订阅了指定类型的通知
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="type">通知类型</param>
        /// <returns>如果用户订阅了指定类型通知，返回 true；否则返回 false</returns>
        private bool IsSubscribed(string userId, NotificationType type)
        {
            lock (_subscriptions)
            {
                return _subscriptions.TryGetValue(userId, out var types) && types.Contains(type);
            }
        }
    }
}