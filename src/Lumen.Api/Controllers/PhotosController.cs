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
    }
}