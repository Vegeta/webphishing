using WebUI.Utils;

namespace WebUI {
	public class Program {
		public static void Main(string[] args) {
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
			builder.Configuration.AddEnvironmentVariables();

			Configurador.configureDatabase(builder.Services, builder.Configuration);
			Configurador.configureServices(builder.Services, builder.Configuration);

			builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			// https://stackoverflow.com/questions/70511588/how-to-enable-cors-in-asp-net-core-6-0-web-api-project
			app.UseCors(builder => builder
				.AllowAnyHeader()
				.AllowAnyMethod()
				.SetIsOriginAllowed((host) => true)
				.AllowCredentials()
			);

			app.UseHttpsRedirection();

			app.UseAuthorization();

			app.MapControllers();

			app.Run();
		}

		public static IHost config() {
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables();
			var config = builder.Build();

			var host = Host.CreateDefaultBuilder()
				.ConfigureServices(services => {
					Configurador.configureDatabase(services, config);
					services.AddSingleton<IConfigurationRoot>(config);
				}).Build();
			return host;
		}
	}
}