
using Lumen.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Lumen.Domain;
using Lumen.Application.Services;
using Lumen.Infrastructure.Services;
using Lumen.Application.Models;
using Lumen.Application.Dtos;


namespace Lumen.Tests
{
    public class PhotoServiceTagTests
    {
        [Fact]
        public async Task AddTagToPhotoByIdAsync_WhenTagDoesNotExist_CreatesAndAssociatesTag()
        {
            DbContextOptions<LumenDbContext> options = new DbContextOptionsBuilder<LumenDbContext>()
                .UseInMemoryDatabase(databaseName: "AddTagToPhotoByIdAsync_WhenTagDoesNotExist_CreatesAndAssociatesTag")
                .Options;
            var dbContext = new LumenDbContext(options);

            Photo photo = new Photo();
            photo.OriginalFileName = "test.jpg";
            photo.FileExtension = ".jpg";
            photo.MimeType = "image/jpeg";
            photo.StoredFilePath = "/photos/test.jpg";
            photo.FileHash = "abc123";
            photo.FileSizeBytes = 1024;
            photo.DateImported = DateTime.UtcNow;

            dbContext.Photos.Add(photo);
            await dbContext.SaveChangesAsync();

            DummyFileStorageService storageService = new DummyFileStorageService();
            DummyMetadataExtractor metadataExtractor = new DummyMetadataExtractor();
            DummyThumbnailService thumbnailService = new DummyThumbnailService();

            PhotoService service = new PhotoService(storageService, dbContext, metadataExtractor, thumbnailService);

            AddTagRequest request = new AddTagRequest { TagName = "Edinburgh" };
            var result = await service.AddTagToPhotoByIdAsync(photo.Id, request);

            Assert.NotNull(result);
            Assert.Contains("edinburgh", result.Tags);

            var verifyContext = new LumenDbContext(options);
            
            var photoWithTags = await verifyContext.Photos
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == photo.Id);

            Assert.NotNull(photoWithTags);
            Assert.Single(photoWithTags!.Tags);
            Assert.Equal("edinburgh", photoWithTags.Tags.First().Name);
        }

        [Fact]
        public async Task AddTagToPhotoByIdAsync_WhenTagAlreadyExists_ReusesExistingTag()
        {
            DbContextOptions<LumenDbContext> options = new DbContextOptionsBuilder<LumenDbContext>()
                .UseInMemoryDatabase(databaseName: "AddTagToPhotoByIdAsync_WhenTagAlreadyExists_ReusesExistingTag")
                .Options;
            var dbContext = new LumenDbContext(options);

            Tag tag = new Tag();
            tag.Name = "edinburgh";

            Photo existingTaggedPhoto = new Photo();
            existingTaggedPhoto.OriginalFileName = "existingTaggedPhoto.jpg";
            existingTaggedPhoto.FileExtension = ".jpg";
            existingTaggedPhoto.MimeType = "image/jpeg";
            existingTaggedPhoto.StoredFilePath = "/photos/existingTaggedPhoto.jpg";
            existingTaggedPhoto.FileHash = "abc123";
            existingTaggedPhoto.FileSizeBytes = 1024;
            existingTaggedPhoto.DateImported = DateTime.UtcNow;
            existingTaggedPhoto.Tags.Add(tag);

            Photo targetPhoto = new Photo();
            targetPhoto.OriginalFileName = "targetPhoto.jpg";
            targetPhoto.FileExtension = ".jpg";
            targetPhoto.MimeType = "image/jpeg";
            targetPhoto.StoredFilePath = "/photos/targetPhoto.jpg";
            targetPhoto.FileHash = "def456";
            targetPhoto.FileSizeBytes = 2048;
            targetPhoto.DateImported = DateTime.UtcNow;

            dbContext.Photos.Add(existingTaggedPhoto);
            dbContext.Photos.Add(targetPhoto);
            dbContext.Tags.Add(tag);
            await dbContext.SaveChangesAsync();

            DummyFileStorageService storageService = new DummyFileStorageService();
            DummyMetadataExtractor metadataExtractor = new DummyMetadataExtractor();
            DummyThumbnailService thumbnailService = new DummyThumbnailService();

            PhotoService service = new PhotoService(storageService, dbContext, metadataExtractor, thumbnailService);

            AddTagRequest request = new AddTagRequest { TagName = "Edinburgh" };
            var result = await service.AddTagToPhotoByIdAsync(targetPhoto.Id, request);

            Assert.NotNull(result);
            Assert.Contains("edinburgh", result.Tags);

            var verifyContext = new LumenDbContext(options);

            var savedExistingTaggedPhoto = await verifyContext.Photos
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == existingTaggedPhoto.Id);
            var savedTargetPhoto = await verifyContext.Photos
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == targetPhoto.Id);

            Assert.NotNull(savedExistingTaggedPhoto);
            Assert.NotNull(savedTargetPhoto);
            Assert.Single(savedExistingTaggedPhoto!.Tags);
            Assert.Single(savedTargetPhoto!.Tags);
            Assert.Equal("edinburgh", savedExistingTaggedPhoto.Tags.First().Name);
            Assert.Equal("edinburgh", savedTargetPhoto.Tags.First().Name);
            Assert.Single(verifyContext.Tags);
        }

        [Fact]
        public async Task RemoveTagFromPhotoByIdAsync_WhenTagIsNoLongerUsed_DeletesTag()
        {
            DbContextOptions<LumenDbContext> options = new DbContextOptionsBuilder<LumenDbContext>()
                .UseInMemoryDatabase(databaseName: "RemoveTagFromPhotoByIdAsync_WhenTagIsNoLongerUsed_DeletesTag")
                .Options;
            var dbContext = new LumenDbContext(options);

            Tag tag = new Tag();
            tag.Name = "edinburgh";

            Photo photo = new Photo();
            photo.OriginalFileName = "test.jpg";
            photo.FileExtension = ".jpg";
            photo.MimeType = "image/jpeg";
            photo.StoredFilePath = "/photos/test.jpg";
            photo.FileHash = "abc123";
            photo.FileSizeBytes = 1024;
            photo.DateImported = DateTime.UtcNow;
            photo.Tags.Add(tag);

            dbContext.Photos.Add(photo);
            dbContext.Tags.Add(tag);
            await dbContext.SaveChangesAsync();

            DummyFileStorageService storageService = new DummyFileStorageService();
            DummyMetadataExtractor metadataExtractor = new DummyMetadataExtractor();
            DummyThumbnailService thumbnailService = new DummyThumbnailService();

            PhotoService service = new PhotoService(storageService, dbContext, metadataExtractor, thumbnailService);

            var result = await service.RemoveTagFromPhotoByIdAsync(photo.Id, "edinburgh");

            Assert.NotNull(result);
            Assert.Empty(result.Tags);

            var verifyContext = new LumenDbContext(options);

            var photoWithTags = await verifyContext.Photos
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == photo.Id);

            Assert.NotNull(photoWithTags);
            Assert.Empty(photoWithTags.Tags);
            Assert.Empty(verifyContext.Tags);
        }
    }

    public class DummyFileStorageService : IFileStorageService
    {
        public DummyFileStorageService() { }

        public Task<(string storedPath, string fileHash)> SavePhotoAsync(Stream fileStream, string fileName)
        {
            throw new NotImplementedException();
        }
    }

    public class DummyMetadataExtractor : IMetadataExtractor
    {
        public DummyMetadataExtractor() { }

        public PhotoMetadata ExtractMetadata(Stream fileStream)
        {
            throw new NotImplementedException();
        }
    }

    public class DummyThumbnailService : IThumbnailService
    {
        public DummyThumbnailService() { }

        public Task<string> GenerateThumbnailAsync(string sourceFilePath)
        {
            throw new NotImplementedException();
        }
    }
}
