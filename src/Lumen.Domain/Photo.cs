
namespace Lumen.Domain
{
    public class Photo
    {
        public int Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string StoredFilePath { get; set; } = string.Empty;
        public string? ThumbnailPath { get; set; }
        public string? Description { get; set; }
        public string FileHash { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public int WidthPx { get; set; }
        public int HeightPx { get; set; }
        public DateTime? DateTaken { get; set; }
        public DateTime DateImported { get; set; }

        // EXIF metadata
        public string? CameraMake { get; set; }
        public string? CameraModel { get; set; }
        public string? LensModel { get; set; }
        public int? Iso { get; set; }
        public string? ShutterSpeed { get; set; }
        public string? Aperture { get; set; }
        public string? FocalLength { get; set; }
        public double? GpsLatitude { get; set; }
        public double? GpsLongitude { get; set; }
        public int? Orientation { get; set; }

        // Navigation properties
        public ICollection<Tag> Tags { get; set; } = [];
        public ICollection<Album> Albums { get; set; } = [];
    }
}