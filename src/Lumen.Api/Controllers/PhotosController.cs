using Lumen.Application.Dtos;
using Lumen.Application.Models;
using Lumen.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Lumen.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly IPhotoService _photoService;

        public PhotosController(IPhotoService photoService)
        {
            _photoService = photoService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<PhotoDto>> UploadPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            await using Stream fileStream = file.OpenReadStream();
            PhotoDto photo = await _photoService.UploadPhotoAsync(fileStream, file.FileName, file.Length);

            return StatusCode(StatusCodes.Status201Created, photo);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PhotoDto>> GetPhotoById(int id)
        {
            var photo = await _photoService.GetPhotoByIdAsync(id);
            if (photo is null)
                return NotFound("Photo not found.");
            return Ok(photo);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<PhotoDto>>> GetPhotos(int page = 1, int pageSize = 20)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page and page size must be greater than zero.");
            }
            var photos = await _photoService.GetPhotosAsync(page, pageSize);
            return Ok(photos);
        }

        [HttpGet("{id}/file")]
        public async Task<IActionResult> GetPhotoFile(int id)
        {
            var filePath = await _photoService.GetPhotoFilePathByIdAsync(id);
            if (filePath is null)
                return NotFound("Photo not found.");
            string absoluteFilePath = Path.GetFullPath(filePath);
            if (!System.IO.File.Exists(absoluteFilePath))
                return NotFound("Photo file not found on disk.");
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(absoluteFilePath, out string? contentType))
                contentType = "application/octet-stream";
            return PhysicalFile(absoluteFilePath, contentType);
        }

        [HttpGet("{id}/thumb")]
        public async Task<IActionResult> GetPhotoThumb(int id)
        {
            var filePath = await _photoService.GetPhotoThumbnailPathByIdAsync(id);
            if (filePath is null)
                return NotFound("Photo not found.");
            string absoluteFilePath = Path.GetFullPath(filePath);
            if (!System.IO.File.Exists(absoluteFilePath))
                return NotFound("Photo file not found on disk.");
            string contentType = "image/jpeg";
            return PhysicalFile(absoluteFilePath, contentType);
        }
    }
}