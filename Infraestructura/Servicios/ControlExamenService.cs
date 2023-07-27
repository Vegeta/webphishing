using Domain;
using Domain.Entidades;
using Infraestructura.Persistencia;

namespace Infraestructura.Servicios;

public class ControlExamenService {
	private AlgoritmoAsignacion _algo;

	private readonly AppDbContext _db;

	public ControlExamenService(AppDbContext db) {
		_db = db;
		_algo = new AlgoritmoAsignacion();
	}

	public bool Finalizar(EstadoExamen estado, SesionPersona sesion) {
		var respuestas = _db.SesionRespuesta
			.Where(x => x.SesionId == sesion.Id)
			.OrderBy(x => x.Inicio);
		var scores = respuestas.Select(x => x.Score).ToList();
		var tiempos = respuestas.Select(x => x.Tiempo).ToList();
		sesion.AvgScore = (float?)scores.Average();
		sesion.AvgTiempo = tiempos.Average();

		var exitos = respuestas.Count(x => x.Score > 0);

		var inicio = respuestas.First();
		var fin = respuestas.Last();
		if (inicio.Inicio.HasValue && fin.Fin.HasValue) {
			var ts = fin.Fin - inicio.Inicio;
			sesion.TiempoTotal = (float)ts.Value.TotalSeconds;
		}

		sesion.FechaExamen = inicio.Inicio;
		sesion.FechaFin = inicio.Fin;

		if (exitos == 0)
			sesion.Exito = 0;
		else {
			sesion.Exito = (respuestas.Count() / exitos) * 100;
		}
		
		return true;
	}
	
	public void EvaluarCuestionario(SesionPersona sesion, IList<CuestionarioRespuesta> respuestas) {
		var calif = RespuestaCuestionario.Calificacion();

		foreach (var item in respuestas) {
			item.SesionId = sesion.Id;
			var resp = item.Respuesta ?? "";
			if (calif.TryGetValue(resp, out var value))
				item.Puntaje = value;
		}

		var pordim = new Dictionary<string, StatsDimension>();
		// agrupar por dimensiones, sumar cada una y promedio de las sumas
		var grupos = respuestas.GroupBy(x => x.Dimension ?? "");
		foreach (var grupo in grupos) {
			var t = new StatsDimension {
				Dimension = grupo.Key,
				Suma = grupo.Select(x => x.Puntaje).Sum(),
				Prom = grupo.Select(x => x.Puntaje).Average()
			};
			pordim[grupo.Key] = t;
		}
		var avg = pordim.Values.Select(x => x.Suma).Average();
		sesion.ScoreCuestionario = (float?)avg;
		sesion.Percepcion = RespuestaCuestionario.Percepcion(Convert.ToDecimal(avg));
		sesion.RespuestaCuestionario = JSON.Stringify(pordim.Values);
	}
	
	public AccionExamen ResponderPregunta(EstadoExamen estado, SesionRespuesta resp) {
		var p = estado.PreguntaActual();

		// tiempo
		if (resp.Inicio.HasValue && resp.Fin.HasValue) {
			var dif = resp.Fin - resp.Inicio;
			resp.Tiempo = (float)dif.Value.TotalSeconds;
		}

		var dbPreg = _db.Pregunta.Select(x => new Pregunta {
			Id = x.Id,
			Dificultad = x.Dificultad,
			Legitimo = x.Legitimo
		}).FirstOrDefault(x => x.Id == p.Id);

		p.Respuesta = true;
		p.Score = EvaluarRespuesta(dbPreg, resp);
		resp.Score = resp.Score;
		estado.Score += p.Score;

		return SiguientePaso(estado);
	}

	public int EvaluarRespuesta(Pregunta preg, SesionRespuesta resp) {
		var correcto = (resp.Respuesta == "legitimo" && preg.Legitimo == 1)
		               || (resp.Respuesta == "phish" && preg.Legitimo is 0 or null);
		var puntos = 0;
		if (correcto) {
			puntos = DificultadPregunta.ScoreRespuesta(preg.Dificultad ?? DificultadPregunta.Facil);
		}
		resp.Score = puntos;
		return puntos;
	}


	public EstadoExamen CrearEstado(IList<Pregunta> preguntas) {
		var estado = new EstadoExamen();
		estado.Lista = preguntas.Select(x => new PreguntaWebEstado {
			Id = x.Id,
			Dificultad = x.Dificultad ?? "facil"
		}).ToList();

		// inicio
		Step? paso;
		paso = _algo.Siguiente(0, 0);
		if (paso != null) {
			var asignadas = Asignar(paso.TomarPreguntas, estado.Lista);
			estado.Cola.AddRange(asignadas);
		}

		return estado;
	}

	public AccionExamen SiguientePaso(EstadoExamen estado) {
		var respuestas = estado.IndiceRespuesta;
		var score = estado.Score;
		var next = respuestas + 1;
		var mod = next % AlgoritmoAsignacion.MaxPreguntas;

		if (next >= AlgoritmoAsignacion.MaxPreguntas && mod == 0) {
			//Console.WriteLine("reiniciar");
			var inicio = _algo.Siguiente(0, 0);
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

		var step = _algo.Siguiente(next, score);
		if (step != null) {
			var asignadas = Asignar(step.TomarPreguntas, estado.Lista);
			estado.Cola.AddRange(asignadas);
		}
		estado.IndiceRespuesta++;

		var preg = estado.PreguntaActual();
		return new AccionExamen {
			Accion = preg == null ? "fin" : "pregunta",
			Pregunta = preg
		};
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
}

public class AccionExamen {
	public string Accion { get; set; } = "";
	public string Mensaje { get; set; } = "";
	public PreguntaWebEstado? Pregunta { get; set; }
}