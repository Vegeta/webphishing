using Microsoft.EntityFrameworkCore;
using WebUI.Data;
using WebUI.Servicios;

namespace WebUI.Utils {
	public class Configurador {

		public static void configureDatabase(IServiceCollection services, IConfigurationRoot config) {
			// https://github.com/skoruba/IdentityServer4.Admin/issues/963
			AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
			AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

			var connString = config.GetConnectionString(DbPhishing.connStringName);
			services.AddDbContext<DbPhishing>(options => {
				options.UseNpgsql(connString)
					.UseSnakeCaseNamingConvention();
			});

			// https://stackoverflow.com/questions/67307830/how-to-create-index-on-json-field-in-postgres
			// https://blog.devart.com/best-practices-in-using-the-dbcontext-in-ef-core.html
		}

		public static void configureServices(IServiceCollection services, IConfigurationRoot config) {
			services.AddTransient<Transformacion, Transformacion>();
		}

	}
}