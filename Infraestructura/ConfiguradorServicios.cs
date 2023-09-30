using Infraestructura.Examenes;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infraestructura;

public static class ConfiguradorServicios {
	public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config) {
		// https://github.com/skoruba/IdentityServer4.Admin/issues/963
		AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
		AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

		var connString = config.GetConnectionString(AppDbContext.ConnStringName);
		services.AddDbContext<AppDbContext>(options => {
			options.UseNpgsql(connString)
				.UseSnakeCaseNamingConvention();
		});
		// https://stackoverflow.com/questions/67307830/how-to-create-index-on-json-field-in-postgres
		// https://blog.devart.com/best-practices-in-using-the-dbcontext-in-ef-core.html

		return services;
	}

	public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config) {
		services.AddScoped<ExamenService>();
		services.AddScoped<CatalogoGeneral>();
		services.AddScoped<RegistroService>();
		services.AddScoped<UsuariosService>();
		services.AddScoped<FlujoExamen>();
		services.AddScoped<EvaluacionesService>();
		services.AddScoped<ManagerExamen>();
		return services;
	}
}