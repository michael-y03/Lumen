using Lumen.Application.Dtos;
using Lumen.Application.Models;
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
            var photo = await _dbContext.Photos
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (photo is null)
                return null;
            return MapPhotoToDto(photo);
        }

        public async Task<PagedResult<PhotoDto>> GetPhotosAsync(int page, int pageSize, string? tag = null)
        {
            IQueryable<Photo> photoQuery = _dbContext.Photos.Include(p => p.Tags);
            if ((!string.IsNullOrWhiteSpace(tag)))
            {
                string normalizedTag = tag.Trim().ToLower();
                photoQuery = photoQuery.Where(p => p.Tags.Any(t => t.Name == normalizedTag));
            }
            photoQuery = photoQuery.OrderByDescending(p => p.DateImported);

            int totalCount = await photoQuery.CountAsync();
            int startIndex = (page - 1) * pageSize;

            List<Photo> pagedPhotos = await photoQuery.Skip(startIndex).Take(pageSize).ToListAsync();
            List<PhotoDto> photoDtos = pagedPhotos.Select(MapPhotoToDto).ToList();

            return new PagedResult<PhotoDto> 
            { 
                Items = photoDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<string?> GetPhotoFilePathByIdAsync(int id)
        {
            var photo = await _dbContext.Photos.FindAsync(id);
            return photo?.StoredFilePath;
        }

        public async Task<string?> GetPhotoThumbnailPathByIdAsync(int id)
        {
            var photo = await _dbContext.Photos.FindAsync(id);
            return photo?.ThumbnailPath;
        }

        public async Task<bool> DeletePhotoByIdAsync(int id)
        {
            var photo = await _dbContext.Photos.FindAsync(id);
            if (photo is null)
                return false;
            string absoluteFilePath = Path.GetFullPath(photo.StoredFilePath);
            if (File.Exists(absoluteFilePath))
                File.Delete(absoluteFilePath);
            if (photo.ThumbnailPath is not null)
            {
                string absoluteThumbnailPath = Path.GetFullPath(photo.ThumbnailPath);
                if (File.Exists(absoluteThumbnailPath))
                    File.Delete(absoluteThumbnailPath);
            }
            _dbContext.Photos.Remove(photo);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<PhotoDto?> UpdatePhotoByIdAsync(int id, PhotoUpdateRequest request)
        {
            var photo = await _dbContext.Photos
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (photo is null)
                return null;
            photo.Description = request.Description;
            photo.DateTaken = request.DateTaken;

            await _dbContext.SaveChangesAsync();
            PhotoDto updatedPhoto = MapPhotoToDto(photo);
            return updatedPhoto;
        }

        public async Task<PhotoDto?> AddTagToPhotoByIdAsync(int id, AddTagRequest request)
        {
            var photo = await _dbContext.Photos
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);

            string tag = request.TagName.Trim().ToLower();

            if (photo is null || tag.Length == 0)
                return null;

            Tag? existingTag = await _dbContext.Tags
                .FirstOrDefaultAsync(t => t.Name == tag);

            if (existingTag is not null)
            {
                if (photo.Tags.Any(t => t.Id == existingTag.Id))
                    return MapPhotoToDto(photo);

                photo.Tags.Add(existingTag);
            }
            else
            {
                Tag newTag = new Tag { Name = tag };
                photo.Tags.Add(newTag);
            }

            await _dbContext.SaveChangesAsync();
            return MapPhotoToDto(photo);
        }

        public async Task<PhotoDto?> RemoveTagFromPhotoByIdAsync(int id, string tagName)
        {
            var photo = await _dbContext.Photos
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);

            string tag = tagName.Trim().ToLower();

            if (photo is null || tag.Length == 0)
                return null;

            var tagToRemove = photo.Tags.FirstOrDefault(t => t.Name == tag);

            if (tagToRemove is not null)
            {
                photo.Tags.Remove(tagToRemove);
                await _dbContext.SaveChangesAsync();

                var remainingPhotosWithTag = await _dbContext.Photos.AnyAsync(p => p.Tags.Any(t => t.Id == tagToRemove.Id));
                if (!remainingPhotosWithTag)
                {
                    _dbContext.Tags.Remove(tagToRemove);
                    await _dbContext.SaveChangesAsync();
                }
            }
            return MapPhotoToDto(photo);
        }

        public async Task<List<TagDto>> GetTagsAsync()
        {
            var tags = await _dbContext.Tags
                .Include(t => t.Photos)
                .ToListAsync();

            return tags.Select(t => new TagDto 
            { 
                Name = t.Name, 
                PhotoCount = t.Photos.Count 
            }).OrderBy(t => t.Name).ToList();
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
                GpsLongitude = photo.GpsLongitude,
                Tags = photo.Tags.Select(t => t.Name).ToList()
            };
        }
    }
}