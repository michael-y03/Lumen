
using Lumen.Application.Dtos;

namespace Lumen.Application.Services
{
    public interface IPhotoService
    {
        Task<PhotoDto> UploadPhotoAsync(Stream fileStream, string fileName, long fileSize);
    }
}