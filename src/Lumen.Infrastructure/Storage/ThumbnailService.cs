using Lumen.Application.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Lumen.Infrastructure.Storage
{
    public class ThumbnailService : IThumbnailService
    {
        public async Task<string> GenerateThumbnailAsync(string sourceFilePath)
        {
            string year = DateTime.UtcNow.Year.ToString();
            string month = DateTime.UtcNow.Month.ToString("00");

            string thumbsDirectory = Path.Combine("storage", "thumbs", year, month);
            Directory.CreateDirectory(thumbsDirectory);

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
            string thumbFileName = fileNameWithoutExtension + "_thumb.jpg";

            string relativeThumbPath = Path.Combine(thumbsDirectory, thumbFileName);
            string absoluteThumbPath = Path.GetFullPath(relativeThumbPath);

            using Image image = await Image.LoadAsync(sourceFilePath);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(400, 400)
            }));

            await image.SaveAsJpegAsync(
                absoluteThumbPath,
                new JpegEncoder { Quality = 75 });

            return relativeThumbPath;
        }
    }
}