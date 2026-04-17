using System.Net;

namespace Lumen.Tests
{
    public class PhotosApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public PhotosApiTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetPhotos_WhenFromIsAfterTo_ReturnsBadRequest()
        {
            HttpClient client = _factory.CreateClient();

            var response = await client.GetAsync("/api/photos?from=2025-01-02&to=2025-01-01");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetPhotoById_WhenPhotoDoesNotExist_ReturnsNotFound()
        {
            HttpClient client = _factory.CreateClient();
            
            var response = await client.GetAsync("/api/photos/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetPhotos_WhenRequestIsValid_ReturnsOk()
        {
            HttpClient client = _factory.CreateClient();

            var response = await client.GetAsync("/api/photos");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetTags_WhenRequestIsValid_ReturnsOk()
        {
            HttpClient client = _factory.CreateClient();

            var response = await client.GetAsync("/api/tags");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


    }
}
