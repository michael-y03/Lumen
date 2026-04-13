using Lumen.Application.Dtos;
using Lumen.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lumen.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly IPhotoService _photoService;

        public TagsController(IPhotoService photoService)
        {
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TagDto>>> GetTags()
        {
            var tags = await _photoService.GetTagsAsync();
            return Ok(tags);
        }
    }
}
