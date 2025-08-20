using Moq;
using NoticeService.Data;
using NoticeService.Models;
using NoticeService.Repositories;
using NoticeService.Services;
using NoticeService.DTOs;
using AutoMapper;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoticeService.Tests
{
    public class NoticeServiceTests
    {
        private readonly Mock<INotificationRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly NotificationService _service;

        public NoticeServiceTests()
        {
            _mockRepository = new Mock<INotificationRepository>();
            _mockMapper = new Mock<IMapper>();
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
            _mockRepository.Setup(r => r.GetUnreadCountsAsync(userId)).ReturnsAsync(mockCounts);

            // Act
            var result = await _service.GetNotificationSummaryAsync(userId);

            // Assert
            Assert.Equal(11, result.TotalUnread); // 5+3+2+1
            Assert.Equal(mockCounts, result.UnreadByType);
            _mockRepository.Verify(r => r.GetUnreadCountsAsync(userId), Times.Once());
        }

        [Fact]
        public async Task MarkAsReadAsync_CallsRepositoryWithCorrectParams()
        {
            // Arrange
            int userId = 1;
            string type = "like";

            // Act
            await _service.MarkAsReadAsync(userId, type);

            // Assert
            _mockRepository.Verify(r => r.MarkAsReadAsync(userId, type), Times.Once());
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
                    UserId = 3, // LikeCount
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
                    UserId = 2, // LikeCount
                    LikerIds = new List<int> { 111, 222 }
                }
            };
            var readTotal = 10;

            _mockRepository.Setup(r => r.GetLikesAsync(userId, page, pageSize))
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
            var result = await _service.GetLikesAsync(userId, page, pageSize);

            // Assert
            Assert.IsType<LikeResponseDto>(result);
            Assert.Equal(unreadDtos, result.Unread);
            Assert.Equal(readTotal, result.Read.Total);
            Assert.Equal(readDtos, result.Read.Items);
            _mockRepository.Verify(r => r.GetLikesAsync(userId, page, pageSize), Times.Once());
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
                    CreatedAt = DateTime.UtcNow
                }
            };
            _mockRepository.Setup(r => r.GetRepliesAsync(userId, page, pageSize, unreadOnly)).ReturnsAsync(mockReplies);

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
            var result = await _service.GetRepliesAsync(userId, page, pageSize, unreadOnly);

            // Assert
            Assert.Equal(mockDtos, result);
            _mockRepository.Verify(r => r.GetRepliesAsync(userId, page, pageSize, unreadOnly), Times.Once());
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
    }
}