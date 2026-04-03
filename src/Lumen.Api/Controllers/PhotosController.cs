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
        public async Task<ActionResult<PagedResult<PhotoDto>>> GetPhotos(int page = 1, int pageSize = 20, string? tag = null)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page and page size must be greater than zero.");
            }
            var photos = await _photoService.GetPhotosAsync(page, pageSize, tag);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            bool success = await _photoService.DeletePhotoByIdAsync(id);
            if (success)
                return NoContent();
            return NotFound("Photo not found.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PhotoDto>> UpdatePhoto(int id, [FromBody] PhotoUpdateRequest request)
        {
            var updatedPhoto = await _photoService.UpdatePhotoByIdAsync(id, request);
            if (updatedPhoto is null)
                return NotFound("Photo not found.");
            return Ok(updatedPhoto);
        }

        [HttpPost("{id}/tags")]
        public async Task<ActionResult<PhotoDto>> AddTagToPhoto(int id, [FromBody] AddTagRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TagName))
                return BadRequest("Tag name cannot be empty.");

            var updatedPhoto = await _photoService.AddTagToPhotoByIdAsync(id, request);
            if (updatedPhoto is null)
                return NotFound("Photo not found.");

            return Ok(updatedPhoto);
        }

        [HttpDelete("{id}/tags/{tagName}")]
        public async Task<ActionResult<PhotoDto>> RemoveTagFromPhoto(int id, string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return BadRequest("Tag name cannot be empty.");
            var updatedPhoto = await _photoService.RemoveTagFromPhotoByIdAsync(id, tagName);
            if (updatedPhoto is null)
                return NotFound("Photo not found.");
            return Ok(updatedPhoto);
        }

        [HttpGet("tags")]
        public async Task<ActionResult<List<TagDto>>> GetTags()
        {
            var tags = await _photoService.GetTagsAsync();
            return Ok(tags);
        }
    }
}