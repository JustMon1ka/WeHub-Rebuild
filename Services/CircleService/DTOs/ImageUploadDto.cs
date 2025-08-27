using System.ComponentModel.DataAnnotations;

namespace CircleService.DTOs
{
    /// <summary>
    /// 图片上传响应DTO
    /// </summary>
    public class ImageUploadResponseDto
    {
        /// <summary>
        /// 上传成功后的图片URL
        /// </summary>
        public string ImageUrl { get; set; } = "";

        /// <summary>
        /// 上传的文件名
        /// </summary>
        public string FileName { get; set; } = "";

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string ContentType { get; set; } = "";
    }

    /// <summary>
    /// 圈子头像上传请求DTO
    /// </summary>
    public class CircleAvatarUploadDto
    {
        /// <summary>
        /// 圈子ID
        /// </summary>
        [Required(ErrorMessage = "圈子ID不能为空")]
        public int CircleId { get; set; }
    }

    /// <summary>
    /// 圈子背景图上传请求DTO
    /// </summary>
    public class CircleBannerUploadDto
    {
        /// <summary>
        /// 圈子ID
        /// </summary>
        [Required(ErrorMessage = "圈子ID不能为空")]
        public int CircleId { get; set; }
    }
}
