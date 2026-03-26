
using Lumen.Application.Dtos;
using Lumen.Application.Models;

namespace Lumen.Application.Services
{
    public interface IPhotoService
    {
        Task<PhotoDto> UploadPhotoAsync(Stream fileStream, string fileName, long fileSize);
        Task<PhotoDto?> GetPhotoByIdAsync(int id);
        Task<PagedResult<PhotoDto>> GetPhotosAsync(int page, int pageSize);
    }
}