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
			var pregs = db.Database.GetDbConnection()
				.Query<Pregunta>("select id, dificultad from pregunta order by RANDOM() limit 15")
				.ToList();

			var estado = new EstadoExamen();
			estado.Lista = pregs.Select(x => new PreguntaWebEstado {
				Id = x.Id,
				Dificultad = x.Dificultad ?? "facil"
			}).ToList();

			var algo = new AlgoritmoAsignacion();

			// inicio
			Step? paso;
			paso = algo.Siguiente(0, 0);
			if (paso != null) {
				var asignadas = Asignar(paso.TomarPreguntas, estado.Lista);
				estado.Cola.AddRange(asignadas);
			}

			var hacer = estado.Lista.Count;
			foreach (var num in Enumerable.Range(1, hacer)) {
				var p = estado.PreguntaActual();
				if (p == null) {
					Console.WriteLine("ERROR no pregunta");
					break;
				}

				p.Respuesta = true;
				p.Score = random.Next(0, 3);
				estado.Score += p.Score;

				var respuestas = estado.IndiceRespuesta;
				var score = estado.Score;
				var next = respuestas + 1;
				var mod = next % AlgoritmoAsignacion.MaxPreguntas;

				if (next >= AlgoritmoAsignacion.MaxPreguntas && mod == 0) {
					Console.WriteLine("reiniciar");
					var inicio = algo.Siguiente(0, 0);
					if (inicio != null) {
						var asignadas = Asignar(inicio.TomarPreguntas, estado.Lista);
						estado.Cola.AddRange(asignadas);
					}
				}
				if (respuestas >= AlgoritmoAsignacion.MaxPreguntas) {
					score = estado.ScoreOffset(mod);
					next = mod;
				}
				Console.WriteLine("real {0}, virt {1}", estado.IndiceRespuesta, next);
				
				var step = algo.Siguiente(next, score);
				if (step != null) {
					var asignadas = Asignar(step.TomarPreguntas, estado.Lista);
					estado.Cola.AddRange(asignadas);
				}
				estado.IndiceRespuesta++;
			}
			estado.Lista.Clear();

			Dump(estado);
		}
		return Task.CompletedTask;
	}

	public IList<PreguntaWebEstado> Asignar(IList<string> dificultades, IList<PreguntaWebEstado> pool) {
		var lista = new List<PreguntaWebEstado>();
		var random = new Random();
		foreach (var d in dificultades) {
			var preg = pool.FirstOrDefault(x => !x.Usada && x.Dificultad == d);
			if (preg == null)
				preg = pool.FirstOrDefault(x => !x.Usada);
			if (preg == null)
				break;
			preg.Usada = true; // ojo, referencia
			lista.Add(preg);
		}
		lista = lista.OrderBy(x => random.Next()).ToList();
		return lista;
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
			var step = algo.Siguiente(numActual, score);
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

	void PruebaFlujo1() {
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
	}

	AccionFlujo Responder(FlujoExamen flujo, AppDbContext db, SesionPersona ses) {
		var rng = new Random();
		var flujoWeb = ses.GetSesionFlujo();
		var next = flujoWeb.Siguiente();


		var preg = db.Pregunta.Find(next.Id);

		var r = new SesionRespuesta();
		//r.Respuesta = rng.Next() % 2 == 0 ? "phish" : "legitimo";
		r.Respuesta = preg.Legitimo.HasValue && preg.Legitimo > 0 ? "legitimo" : "phish";

		var score = flujo.EvaluarRespuesta(preg, r);
		ses.Score += score;
		flujoWeb.Respuestas++;

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