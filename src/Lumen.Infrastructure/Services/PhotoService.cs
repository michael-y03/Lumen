using Lumen.Application.Dtos;
using Lumen.Application.Services;
using Lumen.Domain;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Infrastructure.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly LumenDbContext _dbContext;
        private readonly IMetadataExtractor _metadataExtractor;
        private readonly IThumbnailService _thumbnailService;

        public PhotoService(
            IFileStorageService fileStorageService,
            LumenDbContext dbContext,
            IMetadataExtractor metadataExtractor,
            IThumbnailService thumbnailService)
        {
            _fileStorageService = fileStorageService;
            _dbContext = dbContext;
            _metadataExtractor = metadataExtractor;
            _thumbnailService = thumbnailService;
        }

        public async Task<PhotoDto> UploadPhotoAsync(Stream fileStream, string fileName, long fileSize)
        {
            var (storedPath, fileHash) = await _fileStorageService.SavePhotoAsync(fileStream, fileName);

            string absoluteFilePath = Path.GetFullPath(storedPath);

            await using Stream savedFileStream = new FileStream(absoluteFilePath, FileMode.Open, FileAccess.Read);
            var metadata = _metadataExtractor.ExtractMetadata(savedFileStream);

            string thumbnailPath = await _thumbnailService.GenerateThumbnailAsync(absoluteFilePath);

            Photo photo = new Photo
            {
                OriginalFileName = fileName,
                FileExtension = Path.GetExtension(fileName),
                StoredFilePath = storedPath,
                ThumbnailPath = thumbnailPath,
                FileSizeBytes = fileSize,
                FileHash = fileHash,
                DateImported = DateTime.UtcNow,

                WidthPx = metadata.WidthPx,
                HeightPx = metadata.HeightPx,
                DateTaken = metadata.DateTaken,
                CameraMake = metadata.CameraMake,
                CameraModel = metadata.CameraModel,
                LensModel = metadata.LensModel,
                Iso = metadata.Iso,
                ShutterSpeed = metadata.ShutterSpeed,
                Aperture = metadata.Aperture,
                FocalLength = metadata.FocalLength,
                GpsLatitude = metadata.GpsLatitude,
                GpsLongitude = metadata.GpsLongitude,
                Orientation = metadata.Orientation
            };

            _dbContext.Photos.Add(photo);
            await _dbContext.SaveChangesAsync();

            return MapPhotoToDto(photo);
        }

        public async Task<PhotoDto?> GetPhotoByIdAsync(int id)
        {
            var photo = await _dbContext.Photos.FindAsync(id);
            if (photo is null)
                return null;
            return MapPhotoToDto(photo);
        }

        public async Task<List<PhotoDto>> GetPhotosAsync()
        {
            List<Photo> photos = await _dbContext.Photos.OrderByDescending(p => p.DateImported).ToListAsync();
            List<PhotoDto> photoDtos = photos.Select(MapPhotoToDto).ToList();

            return photoDtos;
        }

        private PhotoDto MapPhotoToDto(Photo photo)
        {
            return new PhotoDto
            {
                Id = photo.Id,
                OriginalFileName = photo.OriginalFileName,
                FileExtension = photo.FileExtension,
                MimeType = photo.MimeType,
                Description = photo.Description,
                FileSizeBytes = photo.FileSizeBytes,
                WidthPx = photo.WidthPx,
                HeightPx = photo.HeightPx,
                DateTaken = photo.DateTaken,
                DateImported = photo.DateImported,
                CameraMake = photo.CameraMake,
                CameraModel = photo.CameraModel,
                LensModel = photo.LensModel,
                Iso = photo.Iso,
                ShutterSpeed = photo.ShutterSpeed,
                Aperture = photo.Aperture,
                FocalLength = photo.FocalLength,
                GpsLatitude = photo.GpsLatitude,
                GpsLongitude = photo.GpsLongitude
            };
        }
    }
}