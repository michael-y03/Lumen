using Lumen.Application.Models;

namespace Lumen.Application.Services
{
    public interface IMetadataExtractor
    {
        PhotoMetadata ExtractMetadata(Stream fileStream);
    }
}