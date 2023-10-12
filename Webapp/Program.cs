using System.Text.Json;
using System.Text.Json.Serialization;
using Infraestructura;
using Infraestructura.Servicios;
using Webapp.Models;
using Webapp.Web;

namespace Webapp {
	public class Program {
		public static void Main(string[] args) {
			var builder = WebApplication.CreateBuilder(args);
			var services = builder.Services;

			services.AddDatabase(builder.Configuration);
			services.AddServices(builder.Configuration);
			services.AddScoped<IUserAccesor, SessionUserAccesor>();
			services.AddTransient<MenuConfig>();

			// Add services to the container.
			builder.Services.AddControllersWithViews()
				.AddJsonOptions(options => {
					options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
					options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
					options.JsonSerializerOptions.Converters.Add(new JsonConverterNullableInt());
					options.JsonSerializerOptions.Converters.Add(new JsonConverterDoubleInt());
				})
				.AddSessionStateTempDataProvider();

			builder.Services.AddSession(options => {
				options.Cookie.Name = AuthConstants.SessionName;
				//options.Cookie.Name = ".Phishing.Session";
				options.IdleTimeout = TimeSpan.FromHours(2);
				options.Cookie.IsEssential = true;
			});

			builder.Services.AddWebAppServices(builder.Configuration);

			builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

			builder.Services.AddHttpContextAccessor();

			builder.Services.AddRazorPages()
				.AddRazorRuntimeCompilation()
				.AddSessionStateTempDataProvider();

			services.AddTransient<IImagenesWeb, ImagenesWeb>(opt => {
				var env = opt.GetRequiredService<IWebHostEnvironment>();
				var path = Path.Combine(env.WebRootPath, "ejercicios");
				return new ImagenesWeb(path);
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment()) {
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			} else {
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseSession();

			app.UseAuthentication();
			app.UseAuthorization();

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