using AutoMapper;
using NoticeService.Models;
using NoticeService.DTOs;
using System.Text.RegularExpressions;

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
                .ForMember(dest => dest.ContentPreview, opt => opt.MapFrom<CommentContentResolver>());
        }

        private static bool ContainsGarbledText(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            // 检查是否包含乱码字符（中文字符被错误编码）
            // 乱码通常包含一些特定的Unicode字符范围
            var garbledPattern = @"[\u4e00-\u9fff]*[\u00c0-\u00ff]+[\u4e00-\u9fff]*";
            return Regex.IsMatch(text, garbledPattern);
        }
    }

    public class CommentContentResolver : IValueResolver<Comment, CommentNotificationDto, string>
    {
        public string Resolve(Comment source, CommentNotificationDto destination, string destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.Content)) return string.Empty;

            // 直接返回原始内容，让前端处理编码问题
            // 或者尝试简单的编码修复
            var content = source.Content;

            // 简单的编码修复：如果看起来像乱码，尝试转换
            if (ContainsGarbledText(content))
            {
                try
                {
                    // 尝试从GB2312重新编码
                    var bytes = System.Text.Encoding.GetEncoding("GB2312").GetBytes(content);
                    var utf8Content = System.Text.Encoding.UTF8.GetString(bytes);

                    // 如果转换后的内容看起来更正常，使用它
                    if (utf8Content.Length > 0 && !ContainsGarbledText(utf8Content))
                    {
                        content = utf8Content;
                    }
                }
                catch
                {
                    // 转换失败，保持原始内容
                }
            }

            return content.Length > 50 ? content.Substring(0, 50) + "..." : content;
        }

        private static bool ContainsGarbledText(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            // 检查是否包含乱码字符（中文字符被错误编码）
            // 乱码通常包含一些特定的Unicode字符范围
            var garbledPattern = @"[\u4e00-\u9fff]*[\u00c0-\u00ff]+[\u4e00-\u9fff]*";
            return Regex.IsMatch(text, garbledPattern);
        }
    }
}