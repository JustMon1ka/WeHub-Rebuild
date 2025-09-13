using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FollowService.DTOs;
using FollowService.Models;
using FollowService.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FollowService.Services
{
    public class ExtendedFollowService : IFollowService
    {
        private readonly IFollowRepository _followRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExtendedFollowService> _logger;
        private readonly string _messageServiceUrl;

        public ExtendedFollowService(
            IFollowRepository followRepository,
            IHttpContextAccessor httpContextAccessor,
            HttpClient httpClient,
            ILogger<ExtendedFollowService> logger,
            IConfiguration configuration)
        {
            _followRepository = followRepository;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _logger = logger;
            _messageServiceUrl = configuration["MessageService:BaseUrl"] ?? "http://localhost:5082";
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("无法获取当前用户ID");
            }
            return userId;
        }

        public async Task<FollowDto> FollowAsync(CreateFollowDto createFollowDto)
        {
            int currentUserId = GetCurrentUserId();

            if (createFollowDto.FolloweeId == currentUserId)
            {
                throw new InvalidOperationException("不能关注自己");
            }

            var existingFollow = await _followRepository.GetFollowAsync(currentUserId, createFollowDto.FolloweeId);
            if (existingFollow != null)
            {
                // 如果已经关注，检查是否互相关注
                await CheckAndSendMutualFollowMessageAsync(currentUserId, createFollowDto.FolloweeId);
                throw new InvalidOperationException("已关注该用户");
            }

            var follow = new Follow
            {
                FollowerId = currentUserId,
                FolloweeId = createFollowDto.FolloweeId,
                CreatedAt = DateTime.UtcNow
            };

            var createdFollow = await _followRepository.CreateFollowAsync(follow);

            // 检查是否互相关注
            await CheckAndSendMutualFollowMessageAsync(currentUserId, createFollowDto.FolloweeId);

            return new FollowDto
            {
                FollowerId = createdFollow.FollowerId,
                FolloweeId = createdFollow.FolloweeId,
                CreatedAt = createdFollow.CreatedAt
            };
        }

        private async Task CheckAndSendMutualFollowMessageAsync(int currentUserId, int followeeId)
        {
            try
            {
                // 检查反向关注关系
                var reverseFollow = await _followRepository.GetFollowAsync(followeeId, currentUserId);

                if (reverseFollow != null)
                {
                    // 互相关注，发送欢迎消息
                    await SendMutualFollowMessageAsync(currentUserId, followeeId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查互相关注时发生错误");
                // 不抛出异常，避免影响关注功能
            }
        }

        private async Task SendMutualFollowMessageAsync(int senderId, int receiverId)
        {
            try
            {
                var messageData = new
                {
                    Content = "We're now following each other, so let's start chatting!"
                };

                var json = JsonSerializer.Serialize(messageData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 添加认证头
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authHeader.Replace("Bearer ", ""));
                }

                var response = await _httpClient.PostAsync($"{_messageServiceUrl}/api/Messages/{receiverId}", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("互相关注欢迎消息发送成功: {SenderId} -> {ReceiverId}", senderId, receiverId);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("互相关注欢迎消息发送失败: {StatusCode}, {Error}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送互相关注欢迎消息时发生错误");
            }
        }

        public async Task<bool> UnfollowAsync(int followeeId)
        {
            int currentUserId = GetCurrentUserId();
            return await _followRepository.DeleteFollowAsync(currentUserId, followeeId);
        }

        public async Task<UserCountDto> GetFollowCountsAsync(int userId)
        {
            var followingCount = await _followRepository.GetFollowingCountAsync(userId);
            var followerCount = await _followRepository.GetFollowerCountAsync(userId);

            return new UserCountDto
            {
                followingCount = followingCount,
                followerCount = followerCount
            };
        }

        public async Task<PagedFollowListDto> GetFollowingListAsync(int userId, int page, int pageSize)
        {
            var totalCount = await _followRepository.GetFollowingCountAsync(userId);
            var follows = await _followRepository.GetFollowingListAsync(userId, page, pageSize);

            return new PagedFollowListDto
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = follows.Select(f => new FollowDto
                {
                    FollowerId = f.FollowerId,
                    FolloweeId = f.FolloweeId,
                    CreatedAt = f.CreatedAt
                }).ToList()
            };
        }

        public async Task<PagedFollowListDto> GetFollowersListAsync(int userId, int page, int pageSize)
        {
            var totalCount = await _followRepository.GetFollowerCountAsync(userId);
            var follows = await _followRepository.GetFollowerListAsync(userId, page, pageSize);

            return new PagedFollowListDto
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = follows.Select(f => new FollowDto
                {
                    FollowerId = f.FollowerId,
                    FolloweeId = f.FolloweeId,
                    CreatedAt = f.CreatedAt
                }).ToList()
            };
        }
    }
}
