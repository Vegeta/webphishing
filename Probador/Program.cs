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
using System.Threading.RateLimiting;
using Dapper;

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
		var random = new Random();
		using (var scope = services.CreateScope()) {
			var db = services.GetRequiredService<AppDbContext>();
			var control = services.GetRequiredService<ControlExamenService>();

			//var examen = db.Examen.Find(6);
			//var pregs = control.PreguntasExamen(examen.Id);

			var examen = new Examen { CuestionarioPos = 5 };

			var pregs = db.Database.GetDbConnection()
				.Query<Pregunta>("select id, dificultad from pregunta order by RANDOM() limit 15")
				.ToList();

			var estado = control.CrearEstado(pregs, examen.CuestionarioPos);

			var done = false;
			while (!done) {
				var op = estado.OperacionActual();
				if (op == null) {
					Console.WriteLine("Error, null");
					done = true;
					break;
				}

				if (op.Accion == "fin") {
					done = true;
					break;
				}

				if (op.Accion == "cuestionario") {
					estado.CuestionarioHecho = true;
				}

				if (op.Accion == "pregunta") {
					if (!op.Asignada) {
						Console.WriteLine("Error no asignada");
						break;
					}
					var res = new SesionRespuesta {
						Inicio = DateTime.Now,
						Fin = DateTime.Now.AddSeconds(7),
						PreguntaId = op.PreguntaId.Value,
						Respuesta = "legitimo",
						Comentario = "bababoey",
					};
					//control.ResponderOperacion(estado, estado.IndiceRespuesta, res);
					control.ResponderOperacionActual(estado, res);
				}
				estado.IndiceRespuesta++;
			}

			Dump(estado);
		}

		return Task.CompletedTask;
	}


	void PruebaFlujonuevo() {
		var random = new Random();
		var algo = new AlgoritmoAsignacion();
		var score = 0;
		var numActual = 0;
		var posibles = AlgoritmoAsignacion.MaxPreguntas;
		foreach (var respuesta in Enumerable.Range(0, 16)) {
			if (respuesta % (posibles + 1) == 0) {
				numActual = 0;
				score = 0;
				Console.WriteLine("## tomar {0}", respuesta - posibles);
			}
			if (numActual > 0)
				score += random.Next(0, 3);
			var step = algo.SiguienteAsignacion(numActual, score);
			Console.WriteLine("Resp: {0}, Vuelta: {1}, Score: {2}", respuesta, numActual, score);
			if (step == null)
				Console.WriteLine("-- NADA");
			else {
				if (step.DebeAsignar) {
					Console.WriteLine("-- ASIGNAR");
					Dump(step.TomarPreguntas);
				}
			}
			numActual++;
		}
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