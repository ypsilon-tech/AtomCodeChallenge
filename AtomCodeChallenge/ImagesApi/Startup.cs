using ImagesApi.Model;
using ImagesApi.Model.Caching;
using ImagesApi.Model.ImageHandling;
using ImagesApi.Model.ImageHandling.IO;
using ImagesApi.Model.ImageHandling.IO.LocalFileSystem;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImagesApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = Configuration["RedisCache:EndpointAddress"];
                options.InstanceName = Configuration["RedisCache:InstanceName"];

            });

            services.AddCors(cors =>
            {
                cors.AddPolicy("AnyConsumer", options =>
                {
                    // In practice this should be locked down to at least known origins
                    options.AllowAnyHeader();
                    options.AllowAnyMethod();
                    options.AllowAnyOrigin();
                });
            });

            RegisterApplicationServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseCors("AnyConsumer");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void RegisterApplicationServices(IServiceCollection services)
        {
            services.AddTransient<IImagesLibrary, ImagesLibrary>();
            services.AddTransient<IImagesCache, ImagesCache>();
            services.AddTransient<IImageLoader, ImageLoader>();
            services.AddTransient<IGdiImageLoader, LocalFileSystemGdiImageLoader>();
            services.AddSingleton<ILocalFileSystemConfiguration, LocalFileSystemConfiguration>();
            services.AddTransient<IImageTransformer, ImageTransformer>();
        }
    }
}
