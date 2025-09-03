using System.Net;
using System.Net.Http.Headers;
using MediaService.Data;
using MediaService.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Repositories
{
    public interface IMediaRepository
    {
        Task InsertMediaAsync(Media entity);
        Task<Media?> GetByIdAsync(string mediaId);
    }
    
    public class MediaRepository : IMediaRepository
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpFactory;
        private readonly string _fbBase;
        private readonly string _fbToken;
        private readonly string _fbUploadPath;

        public MediaRepository(AppDbContext db,
            IHttpClientFactory httpFactory,
            IConfiguration cfg)
        {
            _db           = db;
            _httpFactory  = httpFactory;
            _fbBase       = cfg["FileBrowser:BaseUrl"]!;
            _fbToken      = cfg["FileBrowser:ApiToken"]!;
            _fbUploadPath = cfg["FileBrowser:UploadPath"]!;
        }

        public async Task InsertMediaAsync(Media entity)
        {
            _db.Medias.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<Media?> GetByIdAsync(string mediaId)
        {
            return await _db.Medias
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MediaId == mediaId);
        }
    }
}

