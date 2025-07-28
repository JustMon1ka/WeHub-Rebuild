using Microsoft.AspNetCore.SignalR;
using Moq;
using SignalRService;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace SignalRService.Tests
{
    public class NotificationServiceTests
    {
        private readonly Mock<IHubContext<NotificationHub>> _hubContextMock;
        private readonly Mock<IHubClients> _clientsMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly Mock<IGroupManager> _groupsMock;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _hubContextMock = new Mock<IHubContext<NotificationHub>>();
            _clientsMock = new Mock<IHubClients>();
            _clientProxyMock = new Mock<IClientProxy>();
            _groupsMock = new Mock<IGroupManager>();

            _hubContextMock.Setup(h => h.Clients).Returns(_clientsMock.Object);
            _hubContextMock.Setup(h => h.Groups).Returns(_groupsMock.Object);

            _service = new NotificationService(_hubContextMock.Object);
        }

        [Fact]
        public async Task SendNotificationAsync_UserSubscribed_SendsNotification()
        {
            // Arrange
            string senderId = "user123";
            string receiverId = "user456";
            NotificationType type = NotificationType.Like;
            var cts = new CancellationTokenSource();

            // 模拟用户订阅
            var subscribeTask = _service.SubscribeAsync(receiverId, type, cts.Token);
            await Task.Delay(10, CancellationToken.None); // 确保订阅添加
           

            // 验证订阅状态（调试用）
            var isSubscribedMethod = typeof(NotificationService)
                .GetMethod("IsSubscribed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(isSubscribedMethod);
            bool isSubscribed = (bool)isSubscribedMethod.Invoke(_service, new object[] { receiverId, type });
            Assert.True(isSubscribed, $"用户 {receiverId} 未订阅类型 {type}");

            // 模拟 Clients.User
            _clientsMock.Setup(c => c.User(receiverId)).Returns(_clientProxyMock.Object);

            // Act
            await _service.SendNotificationAsync(senderId, receiverId, type, 1001, EntityTypes.Post);

            // Assert
            _clientsMock.Verify(c => c.User(receiverId), Times.Once());
            _clientProxyMock.Verify(
                p =>
                    p.SendCoreAsync(
                        "ReceiveNotification",
                        It.Is<object[]>(
                            args =>
                                args.Length > 0 &&
                                args[0] != null &&
                                args[0].GetType() == typeof(Notification) &&
                                ((Notification)args[0]).SenderId == senderId &&
                                ((Notification)args[0]).ReceiverId == receiverId &&
                                ((Notification)args[0]).Type == type &&
                                ((Notification)args[0]).EntityId == 1001 &&
                                ((Notification)args[0]).EntityType == EntityTypes.Post
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                    Times.Once()
            );

            // 调试：检查 _subscriptions`
            var subscriptionsField = typeof(NotificationService)
                .GetField("_subscriptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var subscriptions = (Dictionary<string, HashSet<NotificationType>>)subscriptionsField.GetValue(_service);
            Console.WriteLine($"_subscriptions: {System.Text.Json.JsonSerializer.Serialize(subscriptions)}");
            cts.Cancel();
            try
            {
                await subscribeTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // 预期取消
            }
        }

        [Fact]
        public async Task SendNotificationAsync_UserNotSubscribed_DoesNotSendNotification()
        {
            // Arrange
            string senderId = "user123";
            string receiverId = "user456";
            NotificationType type = NotificationType.Like;
            var cts = new CancellationTokenSource();

            // 用户订阅其他类型
            var subscribeTask = _service.SubscribeAsync(receiverId, NotificationType.Chat, cts.Token);
            await Task.Delay(10, CancellationToken.None); // 确保订阅添加
            

            // 验证订阅状态（调试用）
            var isSubscribedMethod = typeof(NotificationService)
                .GetMethod("IsSubscribed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(isSubscribedMethod);
            bool isSubscribed = (bool)isSubscribedMethod.Invoke(_service, new object[] { receiverId, NotificationType.Chat });
            Assert.True(isSubscribed, $"用户 {receiverId} 未订阅类型 {NotificationType.Chat}");
            isSubscribed = (bool)isSubscribedMethod.Invoke(_service, new object[] { receiverId, type });
            Assert.False(isSubscribed, $"用户 {receiverId} 不应订阅类型 {type}");

            // 模拟 Clients.User（确保不调用）
            _clientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(_clientProxyMock.Object);

            // Act
            await _service.SendNotificationAsync(senderId, receiverId, type, 1001, EntityTypes.Post);

            // Assert
            _clientsMock.Verify(c => c.User(It.IsAny<string>()), Times.Never());
            _clientProxyMock.Verify(p => p.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()), Times.Never());

            // 调试：检查 _subscriptions
            var subscriptionsField = typeof(NotificationService)
                .GetField("_subscriptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var subscriptions = (Dictionary<string, HashSet<NotificationType>>)subscriptionsField.GetValue(_service);
            Console.WriteLine($"_subscriptions: {System.Text.Json.JsonSerializer.Serialize(subscriptions)}");

            cts.Cancel();
            try
            {
                await subscribeTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // 预期取消
            }
        }

        [Fact]
        public async Task SendGroupNotificationAsync_SendsToGroup()
        {
            // Arrange
            string senderId = "user123";
            string groupId = "group123";
            NotificationType type = NotificationType.Chat;

            // 模拟 Clients.Group 返回 IClientProxy
            _clientsMock.Setup(c => c.Group(groupId)).Returns(_clientProxyMock.Object);

            // Act
            await _service.SendGroupNotificationAsync(senderId, groupId, type, null, null);

            // Assert
            _clientsMock.Verify(c => c.Group(groupId), Times.Once());
            _clientProxyMock.Verify(p => p.SendCoreAsync(
                "ReceiveNotification",
                It.Is<object[]>(args => args.Length > 0 && // 确保数组不为空
                               args[0] != null &&
                               args[0].GetType() == typeof(Notification) && // 替换 is
                               ((Notification)args[0]).SenderId == senderId &&
                               ((Notification)args[0]).ReceiverId == groupId &&
                               ((Notification)args[0]).Type == type &&
                               ((Notification)args[0]).EntityId == null &&
                               ((Notification)args[0]).EntityType == null),
                It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task SubscribeAsync_AddsSubscription()
        {
            // Arrange
            string userId = "user123";
            NotificationType type = NotificationType.Like;
            var cts = new CancellationTokenSource();

            // Act
            var subscribeTask = _service.SubscribeAsync(userId, type, cts.Token);
            await Task.Delay(10, CancellationToken.None); 
            

            // Assert
            var isSubscribedMethod = typeof(NotificationService)
                .GetMethod("IsSubscribed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(isSubscribedMethod); // 确保方法存在
            bool isSubscribed = (bool)isSubscribedMethod.Invoke(_service, new object[] { userId, type });
            Xunit.Assert.True(isSubscribed, $"用户 {userId} 未订阅类型 {type}");
            cts.Cancel(); // 同步取消
            try
            {
                await subscribeTask; // 等待任务完成
            }
            catch (OperationCanceledException)
            {
                // 预期取消
            }
        }

        [Fact]
        public async Task UnsubscribeAsync_RemovesSubscription()
        {
            // Arrange
            string userId = "user123";
            NotificationType type = NotificationType.Like;
            var cts = new CancellationTokenSource();

            // 先订阅
            var subscribeTask = _service.SubscribeAsync(userId, type, cts.Token);
            await Task.Delay(10, CancellationToken.None); // 短暂等待，确保订阅添加
            // 验证订阅已添加
            var isSubscribedMethod = typeof(NotificationService)
                .GetMethod("IsSubscribed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(isSubscribedMethod);
            bool isSubscribed = (bool)isSubscribedMethod.Invoke(_service, new object[] { userId, type });
            Assert.True(isSubscribed, $"用户 {userId} 未订阅类型 {type}");

            // Act: 取消订阅
            await _service.UnsubscribeAsync(userId);

            // Assert: 验证订阅已移除
            isSubscribed = (bool)isSubscribedMethod.Invoke(_service, new object[] { userId, type });
            Assert.False(isSubscribed, $"用户 {userId} 的类型 {type} 订阅未移除");

            // 调试：检查 _subscriptions 状态
            var subscriptionsField = typeof(NotificationService)
                .GetField("_subscriptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var subscriptions = (Dictionary<string, HashSet<NotificationType>>)subscriptionsField.GetValue(_service);
            Console.WriteLine($"_subscriptions after unsubscribe: {System.Text.Json.JsonSerializer.Serialize(subscriptions)}");

        }

        [Fact]
        public async Task SubscribeToGroupAsync_AddsUserToGroup()
        {
            // Arrange
            string userId = "user123";
            string groupId = "group123";

            // Act
            await _service.SubscribeToGroupAsync(userId, groupId);

            // Assert
            _groupsMock.Verify(g => g.AddToGroupAsync(userId, groupId, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UnsubscribeFromGroupAsync_RemovesUserFromGroup()
        {
            // Arrange
            string userId = "user123";
            string groupId = "group123";

            // Act
            await _service.UnsubscribeFromGroupAsync(userId, groupId);

            // Assert
            _groupsMock.Verify(g => g.RemoveFromGroupAsync(userId, groupId, It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task SendNotificationAsync_ThrowsArgumentNullException_WhenSenderIdIsNull()
        {
            // Arrange
            string senderId = null;
            string receiverId = "user456";
            NotificationType type = NotificationType.Like;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.SendNotificationAsync(senderId, receiverId, type));
        }

        [Fact]
        public async Task SubscribeAsync_ThrowsArgumentNullException_WhenUserIdIsNull()
        {
            // Arrange
            string userId = null;
            NotificationType type = NotificationType.Like;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.SubscribeAsync(userId, type));
        }
    }
}