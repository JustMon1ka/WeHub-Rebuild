#nullable enable
using NATS.Client;
using NATS.Client.JetStream;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// 通知类型枚举
    /// </summary>
    public enum NotificationType
    {
        Like,       // 点赞
        Comment,    // 评论
        Follow,     // 关注
        Mention,    // 提及
        Retweet,    // 转发
        Chat        // 私信
    }

    /// <summary>
        /// 实体类型常量定义
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
    /// /// <remarks>
    /// <para>使用场景：</para>
    /// <list type="bullet">
    ///   <item>用户间私信（Chat）</item>
    ///   <item>社交互动通知（Like/Comment/Follow）</item>
    /// </list>
    /// </remarks>
    public class Notification
    {
        /// <summary>
        /// 发送方用户ID（初始化默认为空字符串）
        /// </summary>
        public string SenderId { get; set; } = string.Empty;

        /// <summary>
        /// 接收方用户ID（初始化默认为空字符串）
        /// </summary>
        public string ReceiverId { get; set; } = string.Empty;

        /// <summary>
        /// 通知类型
        /// </summary>
        /// <seealso cref="NotificationType"/>
        public NotificationType Type { get; set; }

        /// <summary>
        /// 关联的业务实体ID
        /// </summary>
        /// <remarks>
        /// <para>适用场景：</para>
        /// <list type="bullet">
        ///   <item>点赞/评论的帖子ID</item>
        ///   <item>回复的评论ID</item>
        ///   <item>null表示无关联实体（如私信通知）</item>
        /// </list>
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? EntityId { get; set; }

        /// <summary>
        /// 关联的业务实体类型
        /// </summary>
        /// <remarks>
        /// <para>应使用<see cref="EntityTypes"/>中定义的常量值</para>
        /// </remarks>
        /// <seealso cref="EntityTypes"/>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? EntityType { get; set; }

        /// <summary>
        /// 通知创建时间（自动初始化为当前UTC时间）
        /// </summary>
        /// <remarks>
        /// 默认使用<see cref="DateTime.UtcNow"/>初始化，避免时区问题
        /// </remarks>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 消息处理状态标记
        /// </summary>
        /// <remarks>
        /// 用于标识离线消息是否已被处理（默认false）
        /// </remarks>
        public bool IsProcessed { get; set; }
    }

    /// <summary>
    /// NATS消息服务
    /// </summary>
    public class NatsService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IJetStream _js;

        // 主题前缀映射
        private static readonly Dictionary<NotificationType, string> SubjectPrefixes = new()
        {
            [NotificationType.Chat] = "user.{0}.messages",
            [NotificationType.Like] = "user.{0}.notifications",
            [NotificationType.Comment] = "user.{0}.notifications",
            [NotificationType.Follow] = "user.{0}.notifications",
            [NotificationType.Mention] = "user.{0}.notifications",
            [NotificationType.Retweet] = "user.{0}.notifications"
        };

        /// <summary>
        /// 初始化NATS客户端连接并创建JetStream上下文
        /// </summary>
        /// <param name="natsUrl">NATS服务器地址，支持以下格式：
        /// <list type="bullet">
        ///   <item><description>nats://localhost:4222（默认值）</description></item>
        ///   <item><description>nats://user:pass@host:4222（带认证）</description></item>
        ///   <item><description>tls://host:4222（TLS加密连接）</description></item>
        /// </list>
        /// </param>
        /// <exception cref="NATSConnectionException">当连接服务器失败时抛出</exception>
        /// <exception cref="NATSNoRespondersException">当服务器未启用JetStream时抛出</exception>
        /// <remarks>
        /// 创建连接后会立即:
        /// <list type="number">
        ///   <item><description>建立TCP长连接</description></item>
        ///   <item><description>验证服务器协议兼容性</description></item>
        ///   <item><description>初始化JetStream上下文（如服务器支持）</description></item>
        /// </list>
        /// </remarks>
        public NatsService(string natsUrl = "nats://localhost:4222") 
        {
            var cf = new ConnectionFactory();
            _connection = cf.CreateConnection(natsUrl);
            _js = _connection.CreateJetStreamContext();
        }

        /// <summary>
        /// 异步发送通知消息到指定用户
        /// </summary>
        /// <param name="receiverId">接收用户的唯一标识符</param>
        /// <param name="senderId">发送用户的唯一标识符</param>
        /// <param name="type">通知类型，决定消息路由和处理方式</param>
        /// <param name="entityId">关联的业务实体ID（可选）</param>
        /// <param name="entityType">关联的业务实体类型（可选）</param>
        /// <returns>表示异步操作的任务</returns>
        /// <remarks>
        /// <para>消息将通过 JetStream 持久化存储，确保可靠传递。</para>
        /// <para>主题(Topic)生成规则：</para>
        /// <list type="bullet">
        ///   <item>私信(chat): user.{receiverId}.messages</item>
        ///   <item>其他通知: user.{receiverId}.notifications</item>
        /// </list>
        /// <para>示例：</para>
        /// <code>
        /// // 发送点赞通知
        /// await SendMessageAsync("user123", "user456", 
        ///     NotificationType.Like, 1001, "post");
        /// </code>
        /// </remarks>
        /// <exception cref="NATSException">当 NATS 连接失败时抛出</exception>
        /// <exception cref="TimeoutException">当 NATS 连接失败时抛出</exception>
        /// <exception cref="ArgumentNullException">当 receiverId 或 senderId 为 null 时抛出</exception>
        public async Task SendMessageAsync(
            string receiverId,
            string senderId,
            NotificationType type,
            int? entityId = null,
            string? entityType = null)
        {
            // 参数校验
            if (receiverId == null)
                ArgumentNullException.ThrowIfNull(receiverId);
            if (senderId == null)
                ArgumentNullException.ThrowIfNull(senderId);;

            try
            {
                var subject = string.Format(SubjectPrefixes[type], receiverId);
                var notification = new Notification
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Type = type,
                    EntityId = entityId,
                    EntityType = entityType
                };

                var json = JsonSerializer.Serialize(notification);
                var data = Encoding.UTF8.GetBytes(json);

                // 添加超时控制
                await _js.PublishAsync(subject, data).WaitAsync(TimeSpan.FromSeconds(3));
            }
            catch (NATSException ex)
            {
                throw new NATSException("消息发布失败", ex);
            }
            catch (TimeoutException ex)
            {
                throw new NATSException("发布操作超时", ex);
            }
        }

        /// <summary>
        /// 异步订阅指定用户的通知消息
        /// </summary>
        /// <param name="userId">目标用户ID</param>
        /// <param name="type">订阅的通知类型</param>
        /// <param name="onMessageReceived">消息处理回调函数，需返回Task</param>
        /// <param name="cancellationToken">用于取消订阅操作的令牌</param>
        /// <returns>持续运行的任务，直到取消令牌被触发</returns>
        /// <remarks>
        /// <para><b>消息拉取机制：</b></para>
        /// <list type="number">
        ///   <item><description>批量拉取500条消息（最大等待2秒）</description></item>
        ///   <item><description>若获取满500条，继续以1秒超时追加拉取</description></item>
        ///   <item><description>每条消息需手动<see cref="Msg.Ack"/>确认</description></item>
        /// </list>
        ///
        /// <para><b>消费者配置：</b></para>
        /// <list type="bullet">
        ///   <item>持久化消费者名称格式：user_[userId]_[type]_consumer</item>
        ///   <item>显式确认模式（AckPolicy.Explicit）</item>
        ///   <item>自动创建缺失的消费者</item>
        /// </list>
        ///
        /// <para><b>错误处理：</b></para>
        /// <list type="table">
        ///   <listheader>
        ///     <term>异常类型</term>
        ///     <description>处理方式</description>
        ///   </listheader>
        ///   <item>
        ///     <term><see cref="NATSTimeoutException"/></term>
        ///     <description>静默重试</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="OperationCanceledException"/></term>
        ///     <description>终止订阅</description>
        ///   </item>
        ///   <item>
        ///     <term>其他异常</term>
        ///     <description>记录日志并延迟1秒后重试</description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// 示例：订阅点赞通知
        /// <code>
        /// var cts = new CancellationTokenSource();
        /// await service.SubscribeAsync("user123", NotificationType.Like, 
        ///     async notification => {
        ///         Console.WriteLine($"收到来自{notification.SenderId}的点赞");
        ///         await SaveToDBAsync(notification);
        ///     }, 
        ///     cts.Token);
        /// </code>
        /// </example>
        /// <exception cref="ArgumentNullException">当userId为null或空时抛出</exception>
        /// <exception cref="NATSJetStreamException">当JetStream配置无效时抛出</exception>
        public async Task SubscribeAsync(
            string userId,
            NotificationType type,
            Func<Notification, Task> onMessageReceived,
            CancellationToken cancellationToken = default)
        {
            var subject = string.Format(SubjectPrefixes[type], userId);
            var durableName = $"user_{userId}_{type}_consumer";

            var options = PullSubscribeOptions.Builder()
                .WithDurable(durableName)
                .WithConfiguration(ConsumerConfiguration.Builder()
                    .WithAckPolicy(AckPolicy.Explicit)
                    .Build())
                .Build();

            var subscription = _js.PullSubscribe(subject, options);

            var allMessages = new List<Msg>();

            int emptyFetchCount = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 首次获取
                    var messages = subscription.Fetch(500, 2000); // 最大拉取消息条数和超时时间
                    allMessages.AddRange(messages);

                    // 如果满载则继续获取
                    while (messages.Count == 500)
                    {
                        var nextBatch = subscription.Fetch(500, 1000);
                        allMessages.AddRange(nextBatch);
                        if (nextBatch.Count < 500) break;
                    } 

                    foreach (var msg in allMessages)
                    {
                        try
                        {
                            var notification = JsonSerializer.Deserialize<Notification>(msg.Data);
                            if (notification == null) continue;

                            notification.IsProcessed = true;
                            await onMessageReceived(notification);
                            msg.Ack();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"处理消息失败: {ex.Message}");
                        }
                    }
                    allMessages.Clear();

                    // 空轮询间隔（逐步退避）
                    var emptyFetchDelay = messages.Count == 0 ?
                        Math.Min(emptyFetchCount++ * 1000, 5000) : 1000;
                    if (messages.Count == 0)
                    {
                        await Task.Delay(emptyFetchDelay, cancellationToken);
                    }
                    else
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                }
                catch (NATS.Client.NATSTimeoutException)
                {
                    continue;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"订阅异常: {ex.Message}");
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        /// <summary>
        /// 释放NATS客户端占用的所有资源
        /// </summary>
        /// <remarks>
        /// 执行以下操作：
        /// <list type="number">
        ///   <item><description>关闭所有活跃的消息订阅</description></item>
        ///   <item><description>终止TCP连接</description></item>
        ///   <item><description>释放内存缓冲区</description></item>
        /// </list>
        /// </remarks>
        public void Dispose()
        {
            _connection?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}