
using Lumen.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Lumen.Domain;
using Lumen.Application.Services;
using Lumen.Infrastructure.Services;
using Lumen.Application.Models;
using Lumen.Application.Dtos;


namespace Lumen.Tests
{
    public class PhotoServiceTests
    {

        private static Photo CreatePhoto(string originalFileName, string storedFilePath, string fileHash, long fileSizeBytes = 1024)
        {
            Photo photo = new Photo();
            photo.OriginalFileName = originalFileName;
            photo.FileExtension = ".jpg";
            photo.MimeType = "image/jpeg";
            photo.StoredFilePath = storedFilePath;
            photo.FileHash = fileHash;
            photo.FileSizeBytes = fileSizeBytes;
            photo.DateImported = DateTime.UtcNow;

            return photo;
        }

        private static PhotoService CreatePhotoService(LumenDbContext dbContext)
        {
            DummyFileStorageService storageService = new DummyFileStorageService();
            DummyMetadataExtractor metadataExtractor = new DummyMetadataExtractor();
            DummyThumbnailService thumbnailService = new DummyThumbnailService();

            return new PhotoService(storageService, dbContext, metadataExtractor, thumbnailService);
        }

        [Fact]
        public async Task AddTagToPhotoByIdAsync_WhenTagDoesNotExist_CreatesAndAssociatesTag()
        {
            DbContextOptions<LumenDbContext> options = new DbContextOptionsBuilder<LumenDbContext>()
                .UseInMemoryDatabase(databaseName: "AddTagToPhotoByIdAsync_WhenTagDoesNotExist_CreatesAndAssociatesTag")
                .Options;
            var dbContext = new LumenDbContext(options);

            Photo photo = CreatePhoto("test.jpg", "/photos/test.jpg", "abc123");

            dbContext.Photos.Add(photo);
            await dbContext.SaveChangesAsync();

            PhotoService service = CreatePhotoService(dbContext);

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

            Photo existingTaggedPhoto = CreatePhoto("existingTaggedPhoto.jpg", "/photos/existingTaggedPhoto.jpg", "abc123");
            existingTaggedPhoto.Tags.Add(tag);

            Photo targetPhoto = CreatePhoto("targetPhoto.jpg", "/photos/targetPhoto.jpg", "def456", 2048);


            dbContext.Photos.Add(existingTaggedPhoto);
            dbContext.Photos.Add(targetPhoto);
            dbContext.Tags.Add(tag);
            await dbContext.SaveChangesAsync();

            PhotoService service = CreatePhotoService(dbContext);

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

            Photo photo = CreatePhoto("test.jpg", "/photos/test.jpg", "abc123");
            photo.Tags.Add(tag);

            dbContext.Photos.Add(photo);
            dbContext.Tags.Add(tag);
            await dbContext.SaveChangesAsync();

            PhotoService service = CreatePhotoService(dbContext);

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

        [Fact]
        public async Task GetPhotosAsync_WhenTagIsProvided_ReturnsOnlyMatchingPhotos()
        {
            DbContextOptions<LumenDbContext> options = new DbContextOptionsBuilder<LumenDbContext>()
                .UseInMemoryDatabase(databaseName: "GetPhotosAsync_WhenTagIsProvided_ReturnsOnlyMatchingPhotos")
                .Options;
            var dbContext = new LumenDbContext(options);

            Tag tag = new Tag();
            tag.Name = "edinburgh";

            Photo matchingPhoto = CreatePhoto("matchingPhoto.jpg", "/photos/matchingPhoto.jpg", "abc123");
            matchingPhoto.Tags.Add(tag);

            Photo nonMatchingPhoto = CreatePhoto("nonMatchingPhoto.jpg", "/photos/nonMatchingPhoto.jpg", "def456", 2048);

            dbContext.Photos.Add(matchingPhoto);
            dbContext.Photos.Add(nonMatchingPhoto);
            dbContext.Tags.Add(tag);
            await dbContext.SaveChangesAsync();

            PhotoService service = CreatePhotoService(dbContext);
            var result = await service.GetPhotosAsync(1, 20, "edinburgh");

            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);

            var returnedPhoto = Assert.Single(result.Items);
            Assert.Equal(matchingPhoto.Id, returnedPhoto.Id);
            Assert.Contains("edinburgh", returnedPhoto.Tags);
        }

        [Fact]
        public async Task GetPhotosAsync_WhenQueryMatchesFileName_ReturnsOnlyMatchingPhotos()
        {
            DbContextOptions<LumenDbContext> options = new DbContextOptionsBuilder<LumenDbContext>()
                .UseInMemoryDatabase(databaseName: "GetPhotosAsync_WhenQueryMatchesFileName_ReturnsOnlyMatchingPhotos")
                .Options;
            var dbContext = new LumenDbContext(options);

            Photo matchingPhoto = CreatePhoto("edinburgh-castle.jpg", "/photos/edinburgh-castle.jpg", "abc123");
            Photo nonMatchingPhoto = CreatePhoto("london-bridge.jpg", "/photos/london-bridge.jpg", "def456", 2048);

            dbContext.Photos.Add(matchingPhoto);
            dbContext.Photos.Add(nonMatchingPhoto);
            await dbContext.SaveChangesAsync();

            PhotoService service = CreatePhotoService(dbContext);

            var result = await service.GetPhotosAsync(1, 20, q: "edinburgh");

            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);

            var returnedPhoto = Assert.Single(result.Items);
            Assert.Equal(matchingPhoto.Id, returnedPhoto.Id);
            Assert.Equal("edinburgh-castle.jpg", returnedPhoto.OriginalFileName);
        }

        [Fact]
        public async Task GetPhotosAsync_WhenQueryMatchesTagName_ReturnsOnlyMatchingPhotos()
        {
            DbContextOptions<LumenDbContext> options = new DbContextOptionsBuilder<LumenDbContext>()
                .UseInMemoryDatabase(databaseName: "GetPhotosAsync_WhenQueryMatchesTagName_ReturnsOnlyMatchingPhotos")
                .Options;
            var dbContext = new LumenDbContext(options);

            Tag tag = new Tag();
            tag.Name = "edinburgh";

            Photo matchingPhoto = CreatePhoto("castle.jpg", "/photos/castle.jpg", "abc123");
            matchingPhoto.Tags.Add(tag);

            Photo nonMatchingPhoto = CreatePhoto("bridge.jpg", "/photos/bridge.jpg", "def456", 2048);

            dbContext.Photos.Add(matchingPhoto);
            dbContext.Photos.Add(nonMatchingPhoto);
            dbContext.Tags.Add(tag);
            await dbContext.SaveChangesAsync();

            PhotoService service = CreatePhotoService(dbContext);

            var result = await service.GetPhotosAsync(1, 20, q: "edinburgh");

            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);

            var returnedPhoto = Assert.Single(result.Items);
            Assert.Equal(matchingPhoto.Id, returnedPhoto.Id);
            Assert.Contains("edinburgh", returnedPhoto.Tags);
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
