using Lumen.Application.Models;
using Lumen.Application.Services;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace Lumen.Infrastructure.Metadata
{
    public class ExifMetadataExtractor : IMetadataExtractor
    {
        public PhotoMetadata ExtractMetadata(Stream fileStream)
        {
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            var directories = ImageMetadataReader.ReadMetadata(fileStream);

            var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            var gps = directories.OfType<GpsDirectory>().FirstOrDefault();

            var metadata = new PhotoMetadata();

            if (subIfd is not null)
            {
                if (subIfd.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTaken))
                {
                    metadata.DateTaken = dateTaken;
                }

                metadata.LensModel = subIfd.GetDescription(ExifDirectoryBase.TagLensModel);
                metadata.ShutterSpeed = subIfd.GetDescription(ExifDirectoryBase.TagExposureTime);
                metadata.Aperture = subIfd.GetDescription(ExifDirectoryBase.TagFNumber);
                metadata.FocalLength = subIfd.GetDescription(ExifDirectoryBase.TagFocalLength);

                if (subIfd.TryGetInt32(ExifDirectoryBase.TagIsoEquivalent, out var iso))
                {
                    metadata.Iso = iso;
                }

                if (subIfd.TryGetInt32(ExifDirectoryBase.TagExifImageWidth, out var exifWidth))
                {
                    metadata.WidthPx = exifWidth;
                }

                if (subIfd.TryGetInt32(ExifDirectoryBase.TagExifImageHeight, out var exifHeight))
                {
                    metadata.HeightPx = exifHeight;
                }
            }

            if (ifd0 is not null)
            {
                metadata.CameraMake = ifd0.GetDescription(ExifDirectoryBase.TagMake);
                metadata.CameraModel = ifd0.GetDescription(ExifDirectoryBase.TagModel);

                if (ifd0.TryGetInt32(ExifDirectoryBase.TagOrientation, out var orientation))
                {
                    metadata.Orientation = orientation;
                }

                if (metadata.WidthPx is null &&
                    ifd0.TryGetInt32(ExifDirectoryBase.TagImageWidth, out var imageWidth))
                {
                    metadata.WidthPx = imageWidth;
                }

                if (metadata.HeightPx is null &&
                    ifd0.TryGetInt32(ExifDirectoryBase.TagImageHeight, out var imageHeight))
                {
                    metadata.HeightPx = imageHeight;
                }
            }

            if (gps is not null && gps.TryGetGeoLocation(out var location))
            {
                metadata.GpsLatitude = location.Latitude;
                metadata.GpsLongitude = location.Longitude;
            }

            return metadata;
        }
    }
}