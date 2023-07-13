using Domain;
using Domain.Entidades;
using Infraestructura;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Probador;

public class Program {
	private static void Main(string[] args) {
		Host.CreateDefaultBuilder(args)
		   .ConfigureServices((hostContext, services) => {
			   var config = hostContext.Configuration;
			   services.AddDatabase(config);
			   services.AddServices(config);
			   services.AddHostedService<ConsoleService>();
		   }).RunConsoleAsync();
	}

}

public class ConsoleService : IHostedService {
	private IServiceProvider services;
	public ConsoleService(IServiceProvider services) {
		this.services = services;
	}

	public Task StartAsync(CancellationToken cancellationToken) {

		using (var scope = services.CreateScope()) {
			var db = services.GetRequiredService<AppDbContext>();
			var flujo = services.GetRequiredService<FlujoExamen>();

			var examen = db.Examen
					.AsNoTracking()
					.Include(x => x.ExamenPregunta)
					.ThenInclude(x => x.Pregunta)
					.First(x => x.Tipo == TipoExamen.Predeterminado);

			var flujoWeb = FlujoExamen.InitFlujo(examen);
			Dump(flujoWeb);

			var ses = new SesionPersona();
			var cues = db.Cuestionario.Select(x => new {
				id = x.Id
			}).FirstOrDefault();
			ses.CuestionarioId = cues?.id;

			var accion = flujo.Avance(ses, flujoWeb);
			Dump(accion);
			ses.Flujo = JSON.Stringify(flujoWeb);
			// fin init


			foreach (var preg in examen.ExamenPregunta) {
				Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++");
				var acc = Responder(flujo, db, ses);
				if (acc.Accion == "cuestionario") {
					ses.RespuestaCuestionario = "{}";
					flujoWeb = ses.GetSesionFlujo();
					Console.WriteLine("//////////////////// \\\\\\\\\\\\\\\\\\\\\\\\");
					accion = flujo.Avance(ses, flujoWeb);
					ses.Flujo = JSON.Stringify(flujoWeb);
					Dump(flujoWeb);
					Dump(accion);
				}

				if (acc.Accion == "fin") {
					Console.WriteLine("************* FIN **************");
				}
			}

			Console.WriteLine(ses.Score);

		}

		return Task.CompletedTask;
	}

	AccionFlujo Responder(FlujoExamen flujo, AppDbContext db, SesionPersona ses) {
		var rng = new Random();
		var flujoWeb = ses.GetSesionFlujo();
		var next = flujoWeb.Siguiente();


		var preg = db.Pregunta.Find(next.Id);

		var r = new SesionRespuesta();
		//r.Respuesta = rng.Next() % 2 == 0 ? "phish" : "legitimo";
		r.Respuesta = preg.Legitimo.HasValue && preg.Legitimo > 0 ? "legitimo" : "phish";

		flujoWeb = flujo.ResponderPregunta(preg, r, ses);
		var accion = flujo.Avance(ses, flujoWeb);
		ses.Flujo = JSON.Stringify(flujoWeb);
		Dump(flujoWeb);
		Dump(accion);
		return accion;
	}

	void Dump(object? obj) {
		var s = ObjectDumper.Dump(obj, new DumpOptions { MaxLevel = 3, DumpStyle = DumpStyle.CSharp });
		Console.WriteLine(s);
	}

	void TestPermisos() {
		var permisos = new List<string> { "preguntas", "chorro" };
		var check = "preguntas|examenes, chorro";

		var vale = UsuariosService.AutorizarPermisos(permisos, check);
		Console.WriteLine(vale);
	}

	public Task StopAsync(CancellationToken cancellationToken) {
		return Task.CompletedTask;
	}
}