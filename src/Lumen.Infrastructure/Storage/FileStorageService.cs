
using Lumen.Application.Services;
using System.Security.Cryptography;

namespace Lumen.Infrastructure.Storage
{
    public class FileStorageService : IFileStorageService
    {
        public async Task<(string storedPath, string fileHash)> SavePhotoAsync(Stream fileStream, string fileName)
        {
            string extension = Path.GetExtension(fileName);

            DateTime now = DateTime.UtcNow;
            string year = now.Year.ToString();
            string month = now.Month.ToString("00");

            string directoryPath = Path.Combine("storage", "photos", year, month);
            Directory.CreateDirectory(directoryPath);

            string fileId = Guid.NewGuid().ToString() + extension;
            string relativeFilePath = Path.Combine(directoryPath, fileId);
            string absoluteFilePath = Path.GetFullPath(relativeFilePath);

            await using (Stream outputStream = new FileStream(absoluteFilePath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(outputStream);
            }

            using SHA256 sha256 = SHA256.Create();
            await using Stream savedFileStream = new FileStream(absoluteFilePath, FileMode.Open, FileAccess.Read);

            byte[] hashBytes = await sha256.ComputeHashAsync(savedFileStream);
            string fileHash = Convert.ToHexString(hashBytes);

            return (relativeFilePath, fileHash);
        }
    }
}