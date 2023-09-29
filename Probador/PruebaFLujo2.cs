using Domain;
using Domain.Entidades;
using Infraestructura.Examenes;
using Infraestructura.Persistencia;

namespace Probador;

public class PruebaFLujo2 {
	public void ProbarAleatorio(AppDbContext db) {
		var conf = new Configurador(db);

		var config = new ConfigExamen {
			NumPreguntas = 10,
			CuestionarioPos = 5,
			Aleatorio = true,
			Tipo = TipoExamen.Predeterminado,
		};

		var sesion = new SesionPersona {
			FechaExamen = DateTime.Now
		};

		var flujo = conf.CrearFlujo(config);
		flujo.Inicio = DateTime.Now;

		for (var i = 0; i < config.NumPreguntas + 1; i++) {
			var paso = conf.RespuestaActual(config, flujo, "phishing", 0.5f);
			Console.WriteLine(paso.Dump());
			Console.WriteLine(flujo.EsFin);
			if (flujo.EsFin)
				break;
		}
		flujo.Fin = DateTime.Now;

		conf.CalculoFinal(flujo, sesion);

		Console.WriteLine("---------------------------------");
		Console.WriteLine(flujo.Dump());
		Console.WriteLine(sesion.Dump());
	}

	public void PruebaParticular(AppDbContext db) {
		var conf = new Configurador(db);

		var examen = db.Examen.First(x => x.Id == 5);

		var config = new ConfigExamen {
			CuestionarioPos = examen.CuestionarioPos,
			Aleatorio = true,
			Tipo = TipoExamen.Personalizado,
			IdExamen = examen.Id,
		};

		var sesion = new SesionPersona {
			FechaExamen = DateTime.Now
		};

		var flujo = conf.CrearFlujo(config);
		flujo.Inicio = DateTime.Now;

		for (var i = 0; i < flujo.NumPreguntas + 1; i++) {
			var paso = conf.RespuestaActual(config, flujo, "legitimo", 0.5f);
			Console.WriteLine(paso.Dump());
			Console.WriteLine(flujo.EsFin);
			if (flujo.EsFin)
				break;
		}
		flujo.Fin = DateTime.Now;

		conf.CalculoFinal(flujo, sesion);

		Console.WriteLine("---------------------------------");
		Console.WriteLine(flujo.Dump());
		Console.WriteLine(sesion.Dump());
	}
}