using Lumen.Application.Dtos;
using Lumen.Application.Services;
using Lumen.Domain;

namespace Lumen.Infrastructure.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly LumenDbContext _dbContext;

        public PhotoService(IFileStorageService fileStorageService, LumenDbContext dbContext)
        {
            _fileStorageService = fileStorageService;
            _dbContext = dbContext;
        }

        public async Task<PhotoDto> UploadPhotoAsync(Stream fileStream, string fileName, long fileSize)
        {
            var (storedPath, fileHash) = await _fileStorageService.SavePhotoAsync(fileStream, fileName);

            Photo photo = new Photo
            {
                OriginalFileName = fileName,
                StoredFilePath = storedPath,
                FileSizeBytes = fileSize,
                FileHash = fileHash,
                DateImported = DateTime.UtcNow
            };

            _dbContext.Photos.Add(photo);
            await _dbContext.SaveChangesAsync();

            PhotoDto dto = new PhotoDto
            {
                Id = photo.Id,
                OriginalFileName = photo.OriginalFileName,
                FileSizeBytes = photo.FileSizeBytes,
                DateImported = photo.DateImported
            };

            return dto;
        }
    }
}