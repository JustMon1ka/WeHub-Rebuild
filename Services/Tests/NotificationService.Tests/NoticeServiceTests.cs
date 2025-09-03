using Moq;
using NoticeService.Data;
using NoticeService.Models;
using NoticeService.Repositories;
using NoticeService.Services;
using NoticeService.DTOs;
using AutoMapper;
using StackExchange.Redis;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NoticeService.Tests
{
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IDatabase> _mockRedis;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _mockRepository = new Mock<INotificationRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockRedis = new Mock<IDatabase>();
            _service = new NotificationService(_mockRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetNotificationSummaryAsync_ReturnsCorrectSummary()
        {
            // Arrange
            int userId = 1;
            var mockCounts = new Dictionary<string, int>
            {
                { "reply", 5 },
                { "like", 3 },
                { "repost", 2 },
                { "mention", 1 }
            };

            _mockRepository.Setup(r => r.GetUnreadCountsAsync(userId, It.IsAny<IDatabase>()))
                           .ReturnsAsync(mockCounts);

            // Act
            var result = await _service.GetNotificationSummaryAsync(userId, _mockRedis.Object);

            // Assert
            Assert.Equal(11, result.TotalUnread);
            Assert.Equal(mockCounts, result.UnreadByType);
            _mockRepository.Verify(r => r.GetUnreadCountsAsync(userId, _mockRedis.Object), Times.Once());
        }

        [Fact]
        public async Task GetNotificationSummaryAsync_ThrowsArgumentNullException_WhenRedisIsNull()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetNotificationSummaryAsync(1, null));
        }

        [Fact]
        public async Task MarkAsReadAsync_CallsRepositoryWithCorrectParams()
        {
            // Arrange
            int userId = 1;
            string type = "like";

            _mockRepository.Setup(r => r.MarkAsReadAsync(userId, type, It.IsAny<IDatabase>()))
                           .Returns(Task.CompletedTask);

            // Act
            await _service.MarkAsReadAsync(userId, type, _mockRedis.Object);

            // Assert
            _mockRepository.Verify(r => r.MarkAsReadAsync(userId, type, _mockRedis.Object), Times.Once());
        }

        [Fact]
        public async Task MarkAsReadAsync_ThrowsArgumentNullException_WhenRedisIsNull()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.MarkAsReadAsync(1, "like", null));
        }

        [Fact]
        public async Task MarkAsReadAsync_ThrowsArgumentException_WhenTypeIsNullOrEmpty()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.MarkAsReadAsync(1, null, _mockRedis.Object));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.MarkAsReadAsync(1, "", _mockRedis.Object));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.MarkAsReadAsync(1, "   ", _mockRedis.Object));
        }

        [Fact]
        public async Task GetLikesAsync_ReturnsUnreadAndReadLikes()
        {
            // Arrange
            int userId = 1;
            int page = 1;
            int pageSize = 2;

            var unreadLikes = new List<Like>
            {
                new Like
                {
                    TargetId = 101,
                    TargetType = "post",
                    CreatedAt = DateTime.UtcNow,
                    UserId = 3,
                    LikerIds = new List<int> { 123, 456, 789 }
                }
            };

            var readLikes = new List<Like>
            {
                new Like
                {
                    TargetId = 303,
                    TargetType = "post",
                    CreatedAt = DateTime.UtcNow.AddHours(-1),
                    UserId = 2,
                    LikerIds = new List<int> { 111, 222 }
                }
            };

            var readTotal = 10;

            _mockRepository.Setup(r => r.GetLikesAsync(userId, page, pageSize, It.IsAny<IDatabase>()))
                           .ReturnsAsync((unreadLikes, (readLikes, readTotal)));

            var unreadDtos = new List<LikeNotificationDto>
            {
                new LikeNotificationDto
                {
                    TargetId = 101,
                    TargetType = "post",
                    LastLikedAt = DateTime.UtcNow,
                    LikeCount = 3,
                    LikerIds = new List<int> { 123, 456, 789 }
                }
            };

            var readDtos = new List<LikeNotificationDto>
            {
                new LikeNotificationDto
                {
                    TargetId = 303,
                    TargetType = "post",
                    LastLikedAt = DateTime.UtcNow.AddHours(-1),
                    LikeCount = 2,
                    LikerIds = new List<int> { 111, 222 }
                }
            };

            _mockMapper.Setup(m => m.Map<List<LikeNotificationDto>>(unreadLikes)).Returns(unreadDtos);
            _mockMapper.Setup(m => m.Map<List<LikeNotificationDto>>(readLikes)).Returns(readDtos);

            // Act
            var result = await _service.GetLikesAsync(userId, page, pageSize, _mockRedis.Object);

            // Assert
            Assert.IsType<LikeResponseDto>(result);
            Assert.Equal(unreadDtos, result.Unread);
            Assert.Equal(readTotal, result.Read.Total);
            Assert.Equal(readDtos, result.Read.Items);
            _mockRepository.Verify(r => r.GetLikesAsync(userId, page, pageSize, _mockRedis.Object), Times.Once());
        }

        [Fact]
        public async Task GetLikesAsync_ThrowsArgumentNullException_WhenRedisIsNull()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetLikesAsync(1, 1, 10, null));
        }

        [Fact]
        public async Task GetRepliesAsync_ReturnsMappedDtos()
        {
            // Arrange
            int userId = 1;
            int page = 1;
            int pageSize = 10;
            bool unreadOnly = false;

            var mockReplies = new List<Reply>
            {
                new Reply
                {
                    ReplyId = 1,
                    ReplyPoster = 2,
                    Content = "Test reply",
                    CreatedAt = DateTime.UtcNow,
                    CommentId = 101,
                    TargetUserId = userId,
                    IsDeleted = false
                }
            };

            _mockRepository.Setup(r => r.GetRepliesAsync(userId, page, pageSize, unreadOnly, It.IsAny<IDatabase>()))
                           .ReturnsAsync(mockReplies);

            var mockDtos = new List<ReplyNotificationDto>
            {
                new ReplyNotificationDto
                {
                    ReplyId = 1,
                    ReplyPoster = 2,
                    ContentPreview = "Test...",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockMapper.Setup(m => m.Map<List<ReplyNotificationDto>>(mockReplies)).Returns(mockDtos);

            // Act
            var result = await _service.GetRepliesAsync(userId, page, pageSize, unreadOnly, _mockRedis.Object);

            // Assert
            Assert.Equal(mockDtos, result);
            _mockRepository.Verify(r => r.GetRepliesAsync(userId, page, pageSize, unreadOnly, _mockRedis.Object), Times.Once());
        }

        [Fact]
        public async Task GetRepliesAsync_ThrowsArgumentNullException_WhenRedisIsNull()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetRepliesAsync(1, 1, 10, false, null));
        }

        [Fact]
        public async Task GetRepostsAsync_ReturnsMappedDtos()
        {
            // Arrange
            int userId = 1;
            int page = 1;
            int pageSize = 10;
            bool unreadOnly = false;

            var mockReposts = new List<Repost>
            {
                new Repost
                {
                    RepostId = 1,
                    UserId = 2,
                    PostId = 101,
                    Comment = "Test repost",
                    CreatedAt = DateTime.UtcNow,
                    TargetUserId = userId
                }
            };

            _mockRepository.Setup(r => r.GetRepostsAsync(userId, page, pageSize, unreadOnly, It.IsAny<IDatabase>()))
                           .ReturnsAsync(mockReposts);

            var mockDtos = new List<RepostNotificationDto>
            {
                new RepostNotificationDto
                {
                    RepostId = 1,
                    UserId = 2,
                    PostId = 101,
                    CommentPreview = "Test...",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockMapper.Setup(m => m.Map<List<RepostNotificationDto>>(mockReposts)).Returns(mockDtos);

            // Act
            var result = await _service.GetRepostsAsync(userId, page, pageSize, unreadOnly, _mockRedis.Object);

            // Assert
            Assert.Equal(mockDtos, result);
            _mockRepository.Verify(r => r.GetRepostsAsync(userId, page, pageSize, unreadOnly, _mockRedis.Object), Times.Once());
        }

        [Fact]
        public async Task GetRepostsAsync_ThrowsArgumentNullException_WhenRedisIsNull()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetRepostsAsync(1, 1, 10, false, null));
        }

        [Fact]
        public async Task GetMentionsAsync_ReturnsMappedDtos()
        {
            // Arrange
            int userId = 1;
            int page = 1;
            int pageSize = 10;
            bool unreadOnly = false;

            var mockMentions = new List<Mention>
            {
                new Mention
                {
                    UserId = 234,
                    TargetType = "post",
                    TargetId = 101,
                    CreatedAt = DateTime.UtcNow,
                    TargetUserId = userId
                }
            };

            _mockRepository.Setup(r => r.GetMentionsAsync(userId, page, pageSize, unreadOnly, It.IsAny<IDatabase>()))
                           .ReturnsAsync(mockMentions);

            var mockDtos = new List<MentionNotificationDto>
            {
                new MentionNotificationDto
                {
                    TargetId = 101,
                    TargetType = "post",
                    UserId = 234,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockMapper.Setup(m => m.Map<List<MentionNotificationDto>>(mockMentions)).Returns(mockDtos);

            // Act
            var result = await _service.GetMentionsAsync(userId, page, pageSize, unreadOnly, _mockRedis.Object);

            // Assert
            Assert.Equal(mockDtos, result);
            _mockRepository.Verify(r => r.GetMentionsAsync(userId, page, pageSize, unreadOnly, _mockRedis.Object), Times.Once());
        }

        [Fact]
        public async Task GetMentionsAsync_ThrowsArgumentNullException_WhenRedisIsNull()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetMentionsAsync(1, 1, 10, false, null));
        }

        [Fact]
        public async Task GetTargetLikersAsync_ReturnsLikerIds()
        {
            // Arrange
            int userId = 1;
            string targetType = "post";
            int targetId = 101;
            int page = 1;
            int pageSize = 5;

            var likerIds = new List<int> { 123, 456, 789 };
            var total = 50;

            _mockRepository.Setup(r => r.GetTargetLikersAsync(userId, targetType, targetId, page, pageSize))
                           .ReturnsAsync((likerIds, total));

            // Act
            var result = await _service.GetTargetLikersAsync(userId, targetType, targetId, page, pageSize);

            // Assert
            Assert.Equal(total, result.Total);
            Assert.Equal(likerIds, result.Items);
            _mockRepository.Verify(r => r.GetTargetLikersAsync(userId, targetType, targetId, page, pageSize), Times.Once());
        }

        [Fact]
        public async Task GetTargetLikersAsync_ThrowsArgumentException_WhenTargetTypeIsEmpty()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetTargetLikersAsync(1, "", 101, 1, 10));

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetTargetLikersAsync(1, "   ", 101, 1, 10));
        }

        [Fact]
        public async Task GetLikesAsync_HandlesEmptyUnreadLikes()
        {
            // Arrange
            int userId = 1;
            int page = 1;
            int pageSize = 10;

            var unreadLikes = new List<Like>();
            var readLikes = new List<Like>
            {
                new Like
                {
                    TargetId = 303,
                    TargetType = "post",
                    CreatedAt = DateTime.UtcNow.AddHours(-1),
                    UserId = 2,
                    LikerIds = new List<int> { 111, 222 }
                }
            };

            var readTotal = 1;

            _mockRepository.Setup(r => r.GetLikesAsync(userId, page, pageSize, It.IsAny<IDatabase>()))
                           .ReturnsAsync((unreadLikes, (readLikes, readTotal)));

            var unreadDtos = new List<LikeNotificationDto>();
            var readDtos = new List<LikeNotificationDto>
            {
                new LikeNotificationDto
                {
                    TargetId = 303,
                    TargetType = "post",
                    LastLikedAt = DateTime.UtcNow.AddHours(-1),
                    LikeCount = 2,
                    LikerIds = new List<int> { 111, 222 }
                }
            };

            _mockMapper.Setup(m => m.Map<List<LikeNotificationDto>>(unreadLikes)).Returns(unreadDtos);
            _mockMapper.Setup(m => m.Map<List<LikeNotificationDto>>(readLikes)).Returns(readDtos);

            // Act
            var result = await _service.GetLikesAsync(userId, page, pageSize, _mockRedis.Object);

            // Assert
            Assert.Empty(result.Unread);
            Assert.Single(result.Read.Items);
            Assert.Equal(readTotal, result.Read.Total);
        }

        [Fact]
        public async Task GetRepliesAsync_UnreadOnly_ReturnsOnlyUnreadReplies()
        {
            // Arrange
            int userId = 1;
            int page = 1;
            int pageSize = 10;
            bool unreadOnly = true;

            var mockReplies = new List<Reply>
            {
                new Reply
                {
                    ReplyId = 1,
                    ReplyPoster = 2,
                    Content = "Unread reply",
                    CreatedAt = DateTime.UtcNow,
                    CommentId = 101,
                    TargetUserId = userId,
                    IsDeleted = false
                }
            };

            _mockRepository.Setup(r => r.GetRepliesAsync(userId, page, pageSize, unreadOnly, It.IsAny<IDatabase>()))
                           .ReturnsAsync(mockReplies);

            var mockDtos = new List<ReplyNotificationDto>
            {
                new ReplyNotificationDto
                {
                    ReplyId = 1,
                    ReplyPoster = 2,
                    ContentPreview = "Unread...",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockMapper.Setup(m => m.Map<List<ReplyNotificationDto>>(mockReplies)).Returns(mockDtos);

            // Act
            var result = await _service.GetRepliesAsync(userId, page, pageSize, unreadOnly, _mockRedis.Object);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].ReplyId);
            _mockRepository.Verify(r => r.GetRepliesAsync(userId, page, pageSize, unreadOnly, _mockRedis.Object), Times.Once());
        }

        [Fact]
        public async Task GetRepostsAsync_UnreadOnly_ReturnsOnlyUnreadReposts()
        {
            // Arrange
            int userId = 1;
            int page = 1;
            int pageSize = 10;
            bool unreadOnly = true;

            var mockReposts = new List<Repost>
            {
                new Repost
                {
                    RepostId = 1,
                    UserId = 2,
                    PostId = 101,
                    Comment = "Unread repost",
                    CreatedAt = DateTime.UtcNow,
                    TargetUserId = userId
                }
            };

            _mockRepository.Setup(r => r.GetRepostsAsync(userId, page, pageSize, unreadOnly, It.IsAny<IDatabase>()))
                           .ReturnsAsync(mockReposts);

            var mockDtos = new List<RepostNotificationDto>
            {
                new RepostNotificationDto
                {
                    RepostId = 1,
                    UserId = 2,
                    PostId = 101,
                    CommentPreview = "Unread...",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockMapper.Setup(m => m.Map<List<RepostNotificationDto>>(mockReposts)).Returns(mockDtos);

            // Act
            var result = await _service.GetRepostsAsync(userId, page, pageSize, unreadOnly, _mockRedis.Object);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].RepostId);
            _mockRepository.Verify(r => r.GetRepostsAsync(userId, page, pageSize, unreadOnly, _mockRedis.Object), Times.Once());
        }

        [Fact]
        public async Task GetMentionsAsync_UnreadOnly_ReturnsOnlyUnreadMentions()
        {
            // Arrange
            int userId = 1;
            int page = 1;
            int pageSize = 10;
            bool unreadOnly = true;

            var mockMentions = new List<Mention>
            {
                new Mention
                {
                    UserId = 234,
                    TargetType = "post",
                    TargetId = 101,
                    CreatedAt = DateTime.UtcNow,
                    TargetUserId = userId
                }
            };

            _mockRepository.Setup(r => r.GetMentionsAsync(userId, page, pageSize, unreadOnly, It.IsAny<IDatabase>()))
                           .ReturnsAsync(mockMentions);

            var mockDtos = new List<MentionNotificationDto>
            {
                new MentionNotificationDto
                {
                    TargetId = 101,
                    TargetType = "post",
                    UserId = 234,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockMapper.Setup(m => m.Map<List<MentionNotificationDto>>(mockMentions)).Returns(mockDtos);

            // Act
            var result = await _service.GetMentionsAsync(userId, page, pageSize, unreadOnly, _mockRedis.Object);

            // Assert
            Assert.Single(result);
            Assert.Equal(234, result[0].UserId);
            _mockRepository.Verify(r => r.GetMentionsAsync(userId, page, pageSize, unreadOnly, _mockRedis.Object), Times.Once());
        }
    }
}