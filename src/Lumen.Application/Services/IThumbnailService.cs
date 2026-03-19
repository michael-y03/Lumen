
namespace Lumen.Application.Services
{
    public interface IThumbnailService
    {
        Task<string> GenerateThumbnailAsync(string sourceFilePath);
    }
}