using Foxhole_Web.Models;

namespace Foxhole_Web
{
	public class Startup
	{
		public static void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();
			services.AddControllersWithViews();
		}


		public static void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
		{
			environment.ApplicationName = "Foxhole Server";

			app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
				Path.Combine(Directory.GetCurrentDirectory())),
				RequestPath = "/wwwroot"
			});

			app.Use(async (context, next) =>
			{
				await next();

				if (context.Response.StatusCode == 404)
				{
					context.Request.Path = "/Exception/404";
					await next();
				}
			});

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
		}
	}
}
