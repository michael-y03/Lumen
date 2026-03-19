namespace Lumen.Application.Services
{
    public interface IFileStorageService
    {
        Task<(string storedPath, string fileHash)> SavePhotoAsync(Stream fileStream, string fileName);
    }
}