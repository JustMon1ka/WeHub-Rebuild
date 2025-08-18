using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FollowService.DTOs;
using FollowService.Models;
using FollowService.Repositories;
using Microsoft.AspNetCore.Http;

namespace FollowService.Services
{
    public class FollowService : IFollowService
    {
        private readonly IFollowRepository _followRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FollowService(IFollowRepository followRepository, IHttpContextAccessor httpContextAccessor)
        {
            _followRepository = followRepository;
            _httpContextAccessor = httpContextAccessor;
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
                throw new InvalidOperationException("已关注该用户");
            }

            var follow = new Follow
            {
                FollowerId = currentUserId,
                FolloweeId = createFollowDto.FolloweeId,
                CreatedAt = DateTime.UtcNow
            };

            var createdFollow = await _followRepository.CreateFollowAsync(follow);

            return new FollowDto
            {
                FollowerId = createdFollow.FollowerId,
                FolloweeId = createdFollow.FolloweeId,
                CreatedAt = createdFollow.CreatedAt
            };
        }

        public async Task<bool> UnfollowAsync(int followeeId)
        {
            int currentUserId = GetCurrentUserId();
            return await _followRepository.DeleteFollowAsync(currentUserId, followeeId);
        }
    }
}