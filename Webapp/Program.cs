using System.Text.Json;
using Infraestructura;
using Microsoft.AspNetCore.Identity;
using Webapp.Models;
using Webapp.Web;

namespace Webapp {
	public class Program {
		public static void Main(string[] args) {
			var builder = WebApplication.CreateBuilder(args);
			var services = builder.Services;

			services.AddDatabase(builder.Configuration);
			services.AddServices(builder.Configuration);

			// Add services to the container.
			builder.Services.AddControllersWithViews()
				.AddJsonOptions(options => {
					options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				})
				.AddSessionStateTempDataProvider();

			builder.Services.AddSession(options => {
				options.Cookie.Name = ".Phishing.Session";
				//options.IdleTimeout = TimeSpan.FromHours(2);
				//options.Cookie.IsEssential = true;
			});

			builder.Services.AddWebAppServices(builder.Configuration);

			builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

			builder.Services.AddHttpContextAccessor();

			builder.Services.AddRazorPages()
				.AddRazorRuntimeCompilation()
				.AddSessionStateTempDataProvider();

			services.AddTransient<IImagenesEjercicios, ImagenesEjercicios>(opt => {
				var env = opt.GetRequiredService<IWebHostEnvironment>();
				var path = Path.Combine(env.WebRootPath, "ejercicios");
				return new ImagenesEjercicios(path);
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment()) {
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			app.UseSession();

			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.Run();
		}
	}
}