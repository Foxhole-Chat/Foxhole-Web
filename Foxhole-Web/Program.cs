using Foxhole_Web;
using Foxhole_Web.Models;

Config_Builder.Read_Config_File("Config.toml");

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);



builder.WebHost.UseUrls(Config.Host_URL);

Startup.ConfigureServices(builder.Services);

WebApplication app = builder.Build();

Startup.Configure(app, app.Environment);

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/",
	defaults: new { controller = "Home", action = "Index" });

app.Run();