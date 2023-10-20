using Infraestructura;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Tests;

/// <summary>
/// Esta cuestion utiliza DI para utilizar en tests unitarios y de integracion, una maravilla
/// Para este caso usamos la base de datos por defecto
/// https://www.linkedin.com/pulse/xunit-dependency-injection-framework-arash-a-sabet/
/// </summary>
public class TestFixture : TestBedFixture {
	protected override void AddServices(IServiceCollection services, IConfiguration? configuration) {
		services.AddDatabase(configuration);
		services.AddServices(configuration);
		services.AddLogging(builder => { builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning); });
	}

	protected override IEnumerable<TestAppSettings> GetTestAppSettings() {
		// NOTA: Es necesario configurar que el archivo appsettings.json se copie Siempre al output en la IDE
		yield return new TestAppSettings {
			Filename = "appsettings.json",
			IsOptional = false
		};
	}

	protected override ValueTask DisposeAsyncCore() => new();
}