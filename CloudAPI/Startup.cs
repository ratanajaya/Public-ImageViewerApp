using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CloudAPI.AL;
using SharedLibrary;
using Serilog;
using CloudAPI.CustomMiddlewares;
using CloudAPI.AL.Models;
using CloudAPI.AL.DataAccess;
using CloudAPI.AL.Services;

namespace CloudAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers();

            services.AddCors(o => o.AddPolicy("MyPolicy", builder => {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            #region Services
            services.AddSingleton<AuthorizationService>();
            services.AddSingleton<FileRepository>();
            services.AddSingleton<LibraryRepository>();
            services.AddSingleton<ISystemIOAbstraction, SystemIOAbstraction>();
            services.AddSingleton<IDbContext, JsonDbContext>();
            services.AddSingleton(new Random());

            var config = new ConfigurationModel {
                LibraryPath = Configuration.GetSection("LibraryPath").Value,
                FullPageCachePath = Path.Combine(Configuration.GetSection("LibraryPath").Value, Configuration.GetSection("LibRelPageCachePath").Value),
                FullAlbumDbPath = Path.Combine(Configuration.GetSection("LibraryPath").Value, Configuration.GetSection("LibRelAlbumDbPath").Value),
                FullArtistDbPath = Path.Combine(Configuration.GetSection("LibraryPath").Value, Configuration.GetSection("LibRelArtistDbPath").Value),
                AlbumMetadataFileName = Configuration.GetSection("AlbumMetadataFileName").Value,
                BuildType = Configuration.GetSection("BuildType").Value
            };
            services.AddSingleton(config);
            if(config.IsPublicBuild) {
                services.AddSingleton<IAlbumInfoProvider, FakeAlbumInfoProvider>();
            }
            else {
                services.AddSingleton<IAlbumInfoProvider, AlbumInfoProvider>();
            }


            var logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serilog/log-.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();
            services.AddSingleton<Serilog.ILogger>(logger);
            #endregion
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            app.UseCors("MyPolicy");

            app.UseRouting();

            app.UseAuthorization();

            app.UseCustomExceptionMiddleware();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
