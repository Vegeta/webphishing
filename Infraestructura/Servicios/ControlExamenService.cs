using Dapper;
using Domain;
using Domain.Entidades;
using Infraestructura.Examenes;
using Infraestructura.Examenes.Asignacion;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Servicios;

public class ControlExamenService {
	public const int PosCuestionario = 5;

	private AlgoritmoAsignacion _algo;

	private readonly AppDbContext _db;

	public ControlExamenService(AppDbContext db) {
		_db = db;
		_algo = new AlgoritmoAsignacion();
	}

	public EstadoExamen CrearEstado(IList<Pregunta> preguntas, int? cuestionarioPos) {
		var estado = new EstadoExamen();
		estado.CuestionarioPos = cuestionarioPos;
		estado.Lista = preguntas.Select(x => new PreguntaWebEstado {
			Id = x.Id,
			Dificultad = x.Dificultad ?? "facil"
		}).ToList();

		estado.DatosExamen ??= new DataExamen();
		foreach (var preg in preguntas) {
			var op = new OperacionExamen { Accion = "pregunta" };
			estado.Operaciones.Add(op);
			estado.DatosExamen.MaxScore += DificultadPregunta.ScoreRespuesta(preg.Dificultad ?? "facil");
		}
		estado.Operaciones.Add(new OperacionExamen { Accion = "fin" });

		if (cuestionarioPos.HasValue && cuestionarioPos.Value <= preguntas.Count) {
			estado.Operaciones.Insert(cuestionarioPos.Value, new OperacionExamen { Accion = "cuestionario" });
		}

		// inicio
		Step? paso;
		paso = _algo.SiguienteAsignacion(0, 0);
		if (paso != null) {
			var nuevas = NuevasPreguntas(paso.TomarPreguntas, estado.Lista);
			estado.AdicionarPreguntas(nuevas);
		}

		return estado;
	}

	public bool ResponderOperacionActual(EstadoExamen estado, SesionRespuesta resp, SesionPersona? sesion = null) {
		var op = estado.OperacionActual();
		if (op != null)
			return false;
		if (op.Accion != "pregunta" || !op.Asignada)
			return false;
		CalificarRespuesta(resp);
		resp.PreguntaId = op.PreguntaId.Value;
		op.Respuesta = resp.Respuesta;
		op.Score = resp.Score ?? 0;
		op.Tiempo = resp.Tiempo;
		estado.Score += op.Score;
		estado.Inicio ??= resp.Inicio; // si no hay
		ResolverAsignaciones(estado);

		if (sesion != null) {
			sesion.Score = estado.Score;
			sesion.FechaExamen ??= estado.Inicio;
			sesion.Estado = SesionPersona.EstadoEnCurso;
		}

		return true;
	}

	public void CalificarRespuesta(SesionRespuesta resp) {
		var dbPreg = _db.Pregunta.Select(x => new Pregunta {
			Id = x.Id,
			Dificultad = x.Dificultad,
			Legitimo = x.Legitimo
		}).FirstOrDefault(x => x.Id == resp.PreguntaId);

		if (dbPreg == null)
			return;

		//if (resp.Inicio.HasValue && resp.Fin.HasValue) {
		if (resp is { Inicio: not null, Fin: not null }) {
			var dif = resp.Fin - resp.Inicio;
			resp.Tiempo = (float)dif.Value.TotalSeconds;
		}

		var correcto = (resp.Respuesta == "legitimo" && dbPreg.Legitimo == 1)
					   || (resp.Respuesta == "phishing" && dbPreg.Legitimo is 0 or null);
		var puntos = 0;
		if (correcto) {
			puntos = DificultadPregunta.ScoreRespuesta(dbPreg.Dificultad ?? DificultadPregunta.Facil);
		}
		resp.Score = puntos;
		resp.Dificultad = dbPreg.Dificultad; // para el registro
	}

	public void EvaluarCuestionario(EstadoExamen estado, IList<CuestionarioRespuesta> respuestas, SesionPersona? sesion = null) {
		var calif = RespuestaCuestionario.Calificacion();

		foreach (var item in respuestas) {
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
		estado.DatosCuestionario ??= new DataCuestionario();
		estado.DatosCuestionario.ScoreCuestionario = (float?)avg;
		estado.DatosCuestionario.Percepcion = RespuestaCuestionario.Percepcion(Convert.ToDecimal(avg));
		estado.DatosCuestionario.RespuestaCuestionario = JSON.Stringify(pordim.Values);

		if (sesion != null) {
			sesion.ScoreCuestionario = estado.DatosCuestionario.ScoreCuestionario;
			sesion.Percepcion = estado.DatosCuestionario.Percepcion;
			sesion.RespuestaCuestionario = estado.DatosCuestionario.RespuestaCuestionario;
			sesion.Estado = SesionPersona.EstadoEnCurso;
		}

	}

	public bool FinalizarSesion(EstadoExamen estado, SesionPersona? sesion = null) {
		var respuestas = estado.Operaciones.Where(x => x.HayRespuesta && x.Asignada).ToList();
		var scores = respuestas.Select(x => x.Score).ToList();
		var tiempos = respuestas.Select(x => x.Tiempo).ToList();

		estado.DatosExamen ??= new DataExamen();

		estado.DatosExamen.AvgScore = (float?)scores.Average();
		estado.DatosExamen.AvgTiempo = tiempos.Average();

		var exitos = respuestas.Count(x => x.Score > 0);

		if (estado.Inicio.HasValue && estado.Fin.HasValue) {
			var ts = estado.Fin - estado.Inicio;
			estado.DatosExamen.TiempoTotal = (float)ts.Value.TotalSeconds;
		}

		var tasa = ((float)exitos / (float)respuestas.Count) * 100;
		estado.DatosExamen.Exito = Convert.ToInt32(tasa);

		estado.DatosExamen.Estado = SesionPersona.EstadoTerminado;

		if (sesion != null) {
			sesion.AvgScore = estado.DatosExamen.AvgScore;
			sesion.AvgTiempo = estado.DatosExamen.AvgTiempo;
			sesion.TiempoTotal = estado.DatosExamen.TiempoTotal;
			sesion.Exito = estado.DatosExamen.Exito;
			sesion.Estado = estado.DatosExamen.Estado;
			sesion.FechaFin ??= estado.Fin;
		}

		return true;
	}

	public IList<PreguntaWebEstado> NuevasPreguntas(IList<string> dificultades, IList<PreguntaWebEstado> pool) {
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

	public void ResolverAsignaciones(EstadoExamen estado) {
		var respuestas = estado.Operaciones.Count(x => x is { Asignada: true, HayRespuesta: true });
		var score = estado.Score;
		var next = respuestas + 1;
		var mod = next % AlgoritmoAsignacion.MaxPreguntas;

		if (next >= AlgoritmoAsignacion.MaxPreguntas && mod == 0) {
			Console.WriteLine("reiniciar");
			var inicio = _algo.SiguienteAsignacion(0, 0);
			if (inicio != null) {
				var nuevas = NuevasPreguntas(inicio.TomarPreguntas, estado.Lista);
				estado.AdicionarPreguntas(nuevas);
				return;
			}
		}

		if (respuestas >= AlgoritmoAsignacion.MaxPreguntas) {
			score = estado.ScoreOffset(mod);
			next = mod;
		}
		Console.WriteLine("real {0}, virt {1}", estado.IndiceRespuesta, next);
		var step = _algo.SiguienteAsignacion(next, score);
		if (step != null) {
			var nuevas = NuevasPreguntas(step.TomarPreguntas, estado.Lista);
			estado.AdicionarPreguntas(nuevas);
		}
	}

	// persistencia, db

	public ConsultasExamen Consultas() {
		return new ConsultasExamen(_db);

	}

}

public class ConsultasExamen {
	private readonly AppDbContext _db;

	public ConsultasExamen(AppDbContext db) {
		_db = db;
	}

	public SesionPersona? SesionPorToken(string token) {
		return _db.SesionPersona.FirstOrDefault(x => x.Token == token);
	}

	public SesionPersona? SesionActualPersona(int personaId) {
		return _db.SesionPersona.FirstOrDefault(x => x.PersonaId == personaId
													 && (x.Estado == SesionPersona.EstadoPendiente || x.Estado == SesionPersona.EstadoEnCurso));
	}

	public IList<Pregunta> PreguntasExamen(int idExamen) {
		return _db.Pregunta
			.Where(x => x.ExamenPregunta.Any(y => y.ExamenId == idExamen))
			.Select(x => new Pregunta {
				Id = x.Id,
				Dificultad = x.Dificultad ?? "facil"
			}).ToList();
	}

	public IList<Pregunta> PreguntasRandom(int numero = 10) {
		return _db.Database.GetDbConnection()
			.Query<Pregunta>("select id, dificultad from pregunta order by RANDOM() limit " + numero)
			.ToList();
	}

	public SesionPersona CrearSesion(Persona per, string tipo) {
		var token = PasswordHash.CreateSecureRandomString();
		return new SesionPersona {
			Estado = SesionPersona.EstadoPendiente,
			Token = token,
			Nombre = per.NombreCompleto,
			PersonaId = per.Id,
			FechaActividad = DateTime.Now,
			Tipo = tipo,
		};
	}

}

public class EstadoExamen {
	public int IndiceRespuesta { get; set; } = 0;
	public int Score { get; set; } = 0;
	public int? CuestionarioPos { get; set; }
	public bool CuestionarioHecho { get; set; }
	public DateTime? Inicio { get; set; }
	public DateTime? Fin { get; set; }
	public DataExamen? DatosExamen { get; set; }
	public DataCuestionario? DatosCuestionario { get; set; }

	public List<PreguntaWebEstado> Lista { get; set; } = new();
	public List<OperacionExamen> Operaciones { get; set; } = new();

	public OperacionExamen? OperacionActual() {
		if (IndiceRespuesta >= Operaciones.Count)
			return null;
		return Operaciones[IndiceRespuesta];
	}

	public int Respondidas => Operaciones.Count(x => x.Asignada && x.HayRespuesta);

	/// <summary>
	/// Esto calcula el score desde la ultima pregunta contestada X elementos atras
	/// Para cuando el examen tiene mas de 10 elementos (algoritmo)
	/// </summary>
	/// <param name="tomar"></param>
	/// <returns></returns>
	public int ScoreOffset(int tomar) {
		return Operaciones.Where(x => x.Asignada && x.HayRespuesta)
			.Reverse()
			.Take(tomar)
			.Sum(x => x.Score);
	}

	public void AdicionarPreguntas(IList<PreguntaWebEstado> preguntas) {
		// orden es importante
		var ix = 0;
		foreach (var op in Operaciones) {
			if (op.Accion != "pregunta")
				continue;
			if (op.Asignada)
				continue;
			if (ix < preguntas.Count) {
				var p = preguntas[ix];
				op.PreguntaId = p.Id;
				ix++;
			}
		}
	}
}

public class PreguntaWebEstado {
	public int Id { get; set; }
	public string Dificultad { get; set; } = "";
	public bool Usada { get; set; } = false;
}

public class OperacionExamen {
	public int? PreguntaId { get; set; }
	public string Accion { get; set; } = "";
	public int Score { get; set; } = 0;
	public float? Tiempo { get; set; }
	public string? Respuesta { get; set; }
	public bool Asignada => PreguntaId != null;
	public bool HayRespuesta => Respuesta != null;
}