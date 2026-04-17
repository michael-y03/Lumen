using Lumen.Api;
using Lumen.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;


namespace Lumen.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<LumenDbContext>));
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<LumenDbContext>));

                services.AddDbContext<LumenDbContext>(options =>
                {
                    options.UseInMemoryDatabase("LumenApiTests");
                });
            });
        }
    }
}
