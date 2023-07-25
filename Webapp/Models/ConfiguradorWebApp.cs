namespace Webapp.Models;
public static class ConfiguradorWebApp {

	/// <summary>
	/// Aca van los servicios del assembly de la aplicacion web, opcionales
	/// </summary>
	/// <param name="services"></param>
	/// <param name="config"></param>
	/// <returns></returns>
	public static IServiceCollection AddWebAppServices(this IServiceCollection services, IConfiguration config) {
		//services.AddScoped<ConsultasAuth>();

		//services.AddIdentity<IdentityUser, IdentityRole>();
		//services.UpgradePasswordSecurity()
		//	.UseBcrypt<IdentityUser>();

		return services;
	}

}
