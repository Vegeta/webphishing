using Domain;
using Domain.Entidades;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Servicios;

public class FlujoExamen {
	public const string EstadoPendiente = "pendiente";
	public const string EstadoEnCurso = "en_curso";
	public const string EstadoTerminado = "terminado";

	private readonly AppDbContext _db;

	private static readonly Random Rng = new();

	public FlujoExamen(AppDbContext db) {
		_db = db;
	}

	public SesionPersona? SesionPorToken(string token) {
		return _db.SesionPersona.FirstOrDefault(x => x.Token == token);
	}

	public SesionPersona? SesionActualPersona(int personaId) {
		return _db.SesionPersona.FirstOrDefault(x => x.PersonaId == personaId
		                                             && (x.Estado == EstadoPendiente || x.Estado == EstadoEnCurso));
	}


	public SesionCreada IniciarSesionIndividual(Persona per, bool guardar = true) {
		// TODO sacar de aca el examen y poner todo el banco de preguntas
		var examen = _db.Examen
			.AsNoTracking()
			.Include(x => x.ExamenPregunta)
			.ThenInclude(x => x.Pregunta)
			.FirstOrDefault(x => x.Tipo == TipoExamen.Predeterminado);
		if (examen == null) {
			return new SesionCreada(null, null, new AccionFlujo("sin_datos"));
		}

		var token = PasswordHash.CreateSecureRandomString();
		var ses = new SesionPersona {
			Estado = EstadoPendiente,
			Token = token,
			Nombre = per.NombreCompleto,
			ExamenId = examen.Id,
			PersonaId = per.Id,
			FechaActividad = DateTime.Now,
			Tipo = examen.Tipo,
		};

		var config = InitFlujo(examen);

		if (config.CuestionarioPos.HasValue) {
			var cues = _db.Cuestionario.Select(x => new {
				id = x.Id
			}).FirstOrDefault();
			ses.CuestionarioId = cues?.id;
		}

		var accion = Avance(ses, config);

		ses.Flujo = JSON.Stringify(config);
		ses.Score = 0;
		ses.MaxScore = config.MaxScore;

		if (guardar) {
			_db.SesionPersona.Add(ses);
			_db.SaveChanges();
		}

		return new SesionCreada(ses, config, accion);
	}

	public static SesionFlujoWeb InitFlujo(Examen examen) {
		// TODO recibir lista preguntas simple, no examen
		var config = new SesionFlujoWeb();
		config.Lista = examen.ExamenPregunta
			.Select(x => new PreguntaWeb {
				Id = x.PreguntaId,
				Dificultad = x.Pregunta.Dificultad ?? DificultadPregunta.Facil
			}).ToList(); // sacar info preguntas
		config.CuestionarioPos = examen.CuestionarioPos;
		// shuffle
		config.Lista = config.Lista.OrderBy(_ => Rng.Next()).ToList();

		foreach (var item in config.Lista) {
			config.MaxScore += DificultadPregunta.ScoreRespuesta(item.Dificultad);
		}

		return config;
	}

	public bool FinalizarSesion(SesionPersona sesion) {
		if (sesion.Estado == EstadoTerminado)
			return false;

		// calculos

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

		sesion.Estado = EstadoTerminado;
		return true;
	}

	public AccionFlujo ResponderCuestionario(SesionPersona sesion, IList<CuestionarioRespuesta> respuestas) {
		var flujoWeb = sesion.GetSesionFlujo();

		var calif = RespuestaCuestionario.Calificacion();

		foreach (var item in respuestas) {
			item.SesionId = sesion.Id;
			var resp = item.Respuesta ?? "";
			if (calif.TryGetValue(resp, out var value))
				item.Puntaje = value;
			_db.CuestionarioRespuesta.Add(item);
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

		var accion = Avance(sesion, flujoWeb);
		sesion.Flujo = JSON.Stringify(flujoWeb);
		sesion.Estado = EstadoEnCurso;
		_db.SaveChanges();
		return accion;
	}

	public AccionFlujo RegistrarRespuesta(SesionPersona sesion, SesionRespuesta resp) {
		var preg = _db.Pregunta
			.AsNoTracking()
			.Select(x => new Pregunta { Id = x.Id, Dificultad = x.Dificultad, Legitimo = x.Legitimo })
			.FirstOrDefault(x => x.Id == resp.PreguntaId);
		var flujo = sesion.GetSesionFlujo();
		var score = EvaluarRespuesta(preg, resp);
		sesion.Score ??= 0; // da fuq
		sesion.Score += score;
		flujo.Respuestas++;
		var accion = Avance(sesion, flujo);
		sesion.Flujo = JSON.Stringify(flujo);
		sesion.FechaActividad = DateTime.Now;
		sesion.Estado = EstadoEnCurso;

		var tx = _db.Database.BeginTransaction();
		try {
			_db.SesionRespuesta.Add(resp);
			_db.SaveChanges();

			if (accion.Accion == "fin") {
				FinalizarSesion(sesion);
			}
			_db.SaveChanges();
			tx.Commit();
		} catch (Exception ex) {
			//TODO log esto
			tx.Rollback();
		}

		return accion;
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

	public AccionFlujo Avance(SesionPersona sesion, SesionFlujoWeb flujoWeb) {
		// TODO simplificar algoritmo a paso definido, ver definicion abajo
		var algo = Algoritmo();
		// convertida recursion en iteracion por la herramienta
		while (true) {
			var paso = algo.Pasos[flujoWeb.Decision];

			if (paso == null) throw new ArgumentException(nameof(paso));

			var res = EvaluarPaso(paso, flujoWeb, sesion);
			// si dice siga al siguiente, continua el flujo
			if (res.Accion == "next")
				continue;
			return res;
			//break;
		}
	}

	public AccionFlujo EvaluarPaso(Step paso, SesionFlujoWeb config, SesionPersona sesion) {
		int score = sesion.Score ?? 0;

		if (config.Respuestas > 0 && config.Respuestas == config.Lista.Count)
			return new AccionFlujo("fin");

		// cuestionario
		if (config.TocaCuestionario(sesion))
			return new AccionFlujo("cuestionario");

		if (paso.Condicion == "fin")
			return new AccionFlujo("fin");


		if (paso.DebeAsignar) {
			Asignar(paso.TomarPreguntas, config);
			config.Decision++;
			var next = config.Siguiente();
			return new AccionFlujo("pregunta", next?.Id);
		}

		if (paso.Contar) {
			if (config.Respuestas == paso.CheckNum) {
				config.Decision++;
				return new AccionFlujo("next");
			}
			return new AccionFlujo("pregunta", config.Siguiente()?.Id);
		}

		// switch score
		Step? paraAsignar = null;
		foreach (var p in paso.Deciciones) {
			var pasa = EvaluarScore(p.Condicion, score);
			if (pasa) {
				paraAsignar = p;
				break;
			}
		}

		if (paraAsignar == null)
			return new AccionFlujo("error");
		;
		Asignar(paraAsignar.TomarPreguntas, config);
		config.Decision++;
		return new AccionFlujo("pregunta", config.Siguiente()?.Id);
	}

	public int Asignar(IList<string> dificultades, SesionFlujoWeb conf) {
		int asignadas = 0;
		var lista = new List<PreguntaWeb>();
		foreach (var d in dificultades) {
			var preg = conf.Lista.FirstOrDefault(x => !x.Usada && x.Dificultad == d);
			if (preg == null)
				preg = conf.Lista.FirstOrDefault(x => !x.Usada);
			if (preg == null)
				break;
			preg.Usada = true;
			lista.Add(preg);
			asignadas++;
		}
		lista = lista.OrderBy(x => Rng.Next()).ToList();
		conf.Cola.AddRange(lista);
		return asignadas;
	}

	public bool EvaluarScore(string cond, int score) {
		if (string.IsNullOrEmpty(cond))
			return false;
		switch (cond) {
			case ">=5": return score >= 5;
			case ">=3 y <5": return score >= 3 && score < 5;
			case ">=6": return score >= 6;
			case ">=3 y <6": return score >= 3 && score < 6;
			case "default": return true; // OJO
			default: return false;
		}
	}

	public StepBuilder Algoritmo() {
		// TODO simplificar, poner conteo de preguntas en el mismo paso de asignar
		var que = new StepBuilder();
		que.Asignar(DificultadPregunta.Facil, DificultadPregunta.Medio, DificultadPregunta.Dificil)
			.ContarRespuestas(3)
			.Switch(op => {
				op.Condicion(">=5", DificultadPregunta.Medio, DificultadPregunta.Dificil, DificultadPregunta.Dificil);
				op.Condicion(">=3 y <5", DificultadPregunta.Medio, DificultadPregunta.Medio,
					DificultadPregunta.Dificil);
				op.Condicion("default", DificultadPregunta.Facil, DificultadPregunta.Facil, DificultadPregunta.Medio);
			})
			.ContarRespuestas(6)
			.Switch(op => {
				op.Condicion(">=6", DificultadPregunta.Medio, DificultadPregunta.Medio, DificultadPregunta.Dificil,
					DificultadPregunta.Dificil);
				op.Condicion(">=3 y <6", DificultadPregunta.Medio, DificultadPregunta.Medio, DificultadPregunta.Dificil,
					DificultadPregunta.Facil);
				op.Condicion("default", DificultadPregunta.Medio, DificultadPregunta.Medio, DificultadPregunta.Facil,
					DificultadPregunta.Facil);
			})
			.ContarRespuestas(10)
			.Condicion("fin");
		return que;
	}
}

public class SesionCreada {
	public SesionPersona? Sesion { get; set; }
	public SesionFlujoWeb? Flujo { get; set; }
	public AccionFlujo Accion { get; set; }

	public SesionCreada(SesionPersona? sesion, SesionFlujoWeb? flujo, AccionFlujo accion) {
		Sesion = sesion;
		Flujo = flujo;
		Accion = accion;
	}
}

public class EstadoExamen {
	public int IndiceRespuesta { get; set; } = 0;
	public int Score { get; set; } = 0;
	public int? CuestionarioPos { get; set; }

	public List<PreguntaWebEstado> Lista { get; set; } = new();
	public List<PreguntaWebEstado> Cola { get; set; } = new();

	public PreguntaWebEstado? PreguntaActual() {
		if (Cola.Count == 0)
			return null;
		// siempre deberia ser +1
		return IndiceRespuesta >= Cola.Count ? null : Cola[IndiceRespuesta];
	}

	public bool TocaCuestionario(SesionPersona ses) {
		return IndiceRespuesta == CuestionarioPos && string.IsNullOrEmpty(ses.RespuestaCuestionario);
	}

	public int ScoreOffset(int tomar) {
		var cuenta = Cola.Count;
		if (cuenta < tomar)
			return 0;
		var score = 0;
		for (var i = 1; i <= tomar; i++) {
			var ix = IndiceRespuesta + 1 - i;
			score += Cola[ix].Score;
		}
		return score;
	}
}

public class PreguntaWebEstado {
	public int Id { get; set; }
	public string Dificultad { get; set; } = "";
	public bool Usada { get; set; } = false;
	public bool Respuesta { get; set; } = false;
	public int Score { get; set; } = 0;
}

public class SesionFlujoWeb {
	public int Respuestas { get; set; } = 0;
	public int? CuestionarioPos { get; set; }
	public int Decision { get; set; } = 0;
	public int MaxScore { get; set; } = 0;
	public List<PreguntaWeb> Lista { get; set; } = new();
	public List<PreguntaWeb> Cola { get; set; } = new();

	public PreguntaWeb? Siguiente() {
		if (Cola.Count == 0)
			return null;
		return Respuestas >= Cola.Count ? null : Cola[Respuestas];
	}

	public bool TocaCuestionario(SesionPersona sesion) {
		return sesion.CuestionarioId.HasValue
		       && Respuestas == CuestionarioPos
		       && string.IsNullOrEmpty(sesion.RespuestaCuestionario);
	}
}

// para el cuestionario
public class StatsDimension {
	public string Dimension { get; set; } = string.Empty;
	public double Suma { get; set; } = 0;
	public double Prom { get; set; } = 0;
}

public class PreguntaWeb {
	public int Id { get; set; }
	public string Dificultad { get; set; } = "";
	public bool Usada { get; set; } = false;
}

public class AccionFlujo {
	public string Accion { get; set; }
	public int? PreguntaId { get; set; }
	public string Mensaje { get; set; } = "";

	public AccionFlujo(string accion, int? preguntaId = null, string? mensaje = null) {
		Accion = accion;
		PreguntaId = preguntaId;
		Mensaje = mensaje ?? "";
	}
}