
namespace Lumen.Application.Models
{
    public class PhotoMetadata
    {
        public int? WidthPx { get; set; }
        public int? HeightPx { get; set; }
        public DateTime? DateTaken { get; set; }
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
    }
}
