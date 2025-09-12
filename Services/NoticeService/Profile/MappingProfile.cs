using AutoMapper;
using NoticeService.Models;
using NoticeService.DTOs;

namespace NoticeService.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Like, LikeNotificationDto>()
                .ForMember(dest => dest.LastLikedAt, opt => opt.MapFrom(src => src.LikeTime))
                .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.LikerIds, opt => opt.Ignore());

            CreateMap<Reply, ReplyNotificationDto>()
                .ForMember(dest => dest.ReplyPoster, opt => opt.MapFrom(src => src.ReplyPoster))
                .ForMember(dest => dest.CommentId, opt => opt.MapFrom(src => src.CommentId)) // 新增
                .ForMember(dest => dest.ContentPreview, opt => opt.MapFrom(src => src.Content.Length > 50 ? src.Content.Substring(0, 50) + "..." : src.Content))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            CreateMap<Repost, RepostNotificationDto>()
                .ForMember(dest => dest.CommentPreview, opt => opt.MapFrom(src => src.Comment != null && src.Comment.Length > 50 ? src.Comment.Substring(0, 50) + "..." : src.Comment));

            CreateMap<Mention, MentionNotificationDto>();
            CreateMap<Comment, CommentNotificationDto>()
                .ForMember(dest => dest.ContentPreview, opt => opt.MapFrom(src => src.Content.Length > 50 ? src.Content.Substring(0, 50) + "..." : src.Content));
        }
    }
}