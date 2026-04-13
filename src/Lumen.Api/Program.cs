using Lumen.Application.Services;
using Lumen.Infrastructure;
using Lumen.Infrastructure.Services;
using Lumen.Infrastructure.Storage;
using Lumen.Infrastructure.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Lumen.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<LumenDbContext>(options =>
                options.UseSqlite("Data Source=lumen.db"));

            builder.Services.AddScoped<IPhotoService, PhotoService>();
            builder.Services.AddScoped<IFileStorageService, FileStorageService>();
            builder.Services.AddScoped<IMetadataExtractor, ExifMetadataExtractor>();
            builder.Services.AddScoped<IThumbnailService, ThumbnailService>();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Run(async context => { 
                    context.Response.StatusCode = 500; 
                    context.Response.ContentType = "application/json"; 
                    await context.Response.WriteAsync("{\"error\":\"An unexpected error occurred.\"}"); });
            });

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}