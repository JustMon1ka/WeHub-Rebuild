using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FollowService.DTOs;
using FollowService.Models; 
using FollowService.Repositories;
using Microsoft.AspNetCore.Http;
using Models;

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

        public async Task<UserCountDto> GetFollowCountsAsync(int userId)
        {
            var followingCount = await _followRepository.GetFollowingCountAsync(userId);
            var followerCount = await _followRepository.GetFollowerCountAsync(userId);

            return new UserCountDto
            {
                FollowingCount = followingCount,
                FollowerCount = followerCount
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