
using Lumen.Application.Dtos;
using Lumen.Application.Models;

namespace Lumen.Application.Services
{
    public interface IPhotoService
    {
        Task<PhotoDto> UploadPhotoAsync(Stream fileStream, string fileName, long fileSize, string contentType);
        Task<PhotoDto?> GetPhotoByIdAsync(int id);
        Task<PagedResult<PhotoDto>> GetPhotosAsync(int page, int pageSize, string? tag = null, string? camera = null, DateTime? from = null, DateTime? to = null, string? q = null, string? sort = "dateImported", string? order = "desc");
        Task<string?> GetPhotoFilePathByIdAsync(int id);
        Task<string?> GetPhotoThumbnailPathByIdAsync(int id);
        Task<bool> DeletePhotoByIdAsync(int id);
        Task<PhotoDto?> UpdatePhotoByIdAsync(int id, PhotoUpdateRequest request);
        Task<PhotoDto?> AddTagToPhotoByIdAsync(int id, AddTagRequest request);
        Task<PhotoDto?> RemoveTagFromPhotoByIdAsync(int id, string tagName);
        Task<List<TagDto>> GetTagsAsync();
    }
}