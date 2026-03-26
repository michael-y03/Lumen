using Lumen.Application.Dtos;
using Lumen.Application.Services;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<List<PhotoDto>>> GetPhotos()
        {
            var photos = await _photoService.GetPhotosAsync();
            return Ok(photos);
        }
    }
}