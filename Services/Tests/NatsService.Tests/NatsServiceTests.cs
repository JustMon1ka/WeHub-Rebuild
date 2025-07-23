using NATS.Client;
using NATS.Client.JetStream;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace Services.Tests
{
    public class NatsServiceTests : IAsyncLifetime
    {
        private NatsService _service;
        private IConnection _testConnection;
        private readonly string _natsUrl = "nats://localhost:4222";

        public async Task InitializeAsync()
        {
            var cf = new ConnectionFactory();
            _testConnection = cf.CreateConnection(_natsUrl);
            _service = new NatsService(_natsUrl);

            // È·±£Á¬½Ó½¨Á¢
            await Task.Delay(100);
        }

        public async Task DisposeAsync()
        {
            if (_service != null)
            {
                _service.Dispose();
                await Task.Delay(100); 
            }

            if (_testConnection != null)
            {
                _testConnection.Dispose();
            }
        }

        [Fact]
        public async Task SendMessageAsync_Should_Publish_To_Correct_Subject()
        {
            // Arrange
            var testSubject = "user.test_user.notifications";
            var streamName = "TEST_STREAM_" + Guid.NewGuid().ToString("N");

            // Ê¹ÓÃ JetStreamManagement À´¹ÜÀíÁ÷
            var jsm = _testConnection.CreateJetStreamManagementContext();

            // ´´½¨Á÷ÅäÖÃ
            var streamConfig = StreamConfiguration.Builder()
                .WithName(streamName)
                .WithSubjects("user.*.notifications")
                .WithStorageType(StorageType.Memory)
                .Build();

            try
            {
                // Ìí¼ÓÁ÷
                jsm.AddStream(streamConfig);

                // »ñÈ¡ JetStream ÉÏÏÂÎÄÓÃÓÚÕý³£²Ù×÷
                var js = _testConnection.CreateJetStreamContext();

                // ´´½¨Pull¶©ÔÄ
                var sub = js.PullSubscribe(
                    subject: testSubject,
                    options: PullSubscribeOptions.Builder()
                        .WithDurable(Guid.NewGuid().ToString())
                        .Build());

                try
                {
                    // Act
                    await _service.SendMessageAsync(
                        "test_user",
                        "test_sender",
                        NotificationType.Like,
                        123);

                    // Assert
                    var messages = sub.Fetch(1, 1000);
                    Assert.Single(messages);

                    var received = JsonSerializer.Deserialize<Notification>(messages[0].Data);
                    Assert.NotNull(received);
                    Assert.Equal("test_sender", received!.SenderId);
                    Assert.Equal(123, received!.EntityId);
                }
                finally
                {
                    sub.Unsubscribe();
                }
            }
            finally
            {
                // ÇåÀí²âÊÔÁ÷
                var jsmForCleanup = _testConnection.CreateJetStreamManagementContext();
                jsmForCleanup.DeleteStream(streamName);
            }
        }

        [Fact]
        public void SubscribeAsync_Should_Receive_Messages_SyncVersion()
        {
            // Arrange
            var testSubject = "user.test_user.messages";
            var streamName = "TEST_STREAM_" + Guid.NewGuid().ToString("N");

            // ´´½¨Á÷
            var jsm = _testConnection.CreateJetStreamManagementContext();
            var config = StreamConfiguration.Builder()
                .WithName(streamName)
                .WithSubjects("user.*.messages")
                .WithStorageType(StorageType.Memory)
                .Build();

            jsm.AddStream(config); 

            try
            {
                var received = false;
                var manualReset = new ManualResetEvent(false);

                // 2. Æô¶¯¶©ÔÄ
                _service.SubscribeAsync("test_user", NotificationType.Chat,
                    notification => {
                        received = true;
                        manualReset.Set();
                        return Task.CompletedTask;
                    },
                    CancellationToken.None);

                // 3. È·±£¶©ÔÄÒÑ×¢²á
                Thread.Sleep(500);

                // Act
                var js = _testConnection.CreateJetStreamContext();

                // ·¢²¼
                js.Publish(testSubject,
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
                        new Notification { SenderId = "test" , ReceiverId = "test_user" , Type = NotificationType.Chat, EntityId = null, EntityType = null})));

                // µÈ´ýÏûÏ¢£¨×î¶à20Ãë£©
                manualReset.WaitOne(20000);

                // Assert
                Assert.True(received, "Message was not received within 20 seconds");
            }
            finally
            {
                // ÇåÀíÁ÷
                jsm.DeleteStream(streamName);
            }
        }

        [Fact]
        public void Constructor_Should_Fail_If_Server_Unavailable()
        {
            var ex = Assert.Throws<NATSConnectionException>(() =>
                new NatsService("nats://invalid:4222"));
            Assert.True(
                ex.Message.Contains("invalid") ||
                ex.Message.Contains("timeout") ||
                ex.Message.Contains("failed"),
                $"Actual message: {ex.Message}"
            );
        }
    }
}