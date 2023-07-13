using Domain;
using Domain.Entidades;
using Infraestructura.Persistencia;

namespace Infraestructura.Servicios;

public class FlujoExamen {

	private readonly AppDbContext _db;

	private static Random rng = new Random();

	public FlujoExamen(AppDbContext db) {
		_db = db;
	}

	public static SesionFlujoWeb InitFlujo(Examen examen) {
		var config = new SesionFlujoWeb();
		config.Lista = examen.ExamenPregunta.Select(x => new PreguntaWeb {
			Id = x.PreguntaId,
			Dificultad = x.Pregunta.Dificultad ?? DificultadPregunta.Facil
		}).ToList(); // sacar info preguntas
		config.CuestionarioPos = examen.CuestionarioPos;

		// shuffle
		config.Lista = config.Lista.OrderBy(_ => rng.Next()).ToList();

		return config;
	}

	public SesionFlujoWeb ResponderPregunta(Pregunta preg, SesionRespuesta resp, SesionPersona sesion) {
		var flujo = sesion.GetSesionFlujo();

		resp.PreguntaId = preg.Id;
		resp.Fin = DateTime.Now;
		// evaluar puntaje

		var actual = sesion.Score ?? 0;
		var correcto = (resp.Respuesta == "legitimo" && preg.Legitimo == 1)
					   || (resp.Respuesta == "phish" && preg.Legitimo is 0 or null);
		var puntos = 0;
		if (correcto) {
			puntos = DificultadPregunta.ScoreRespuesta(preg.Dificultad ?? DificultadPregunta.Facil);
			actual += puntos;
		}
		sesion.Score = actual;
		resp.Score = puntos;
		flujo.Respuestas++;
		return flujo;
	}

	public AccionFlujo Avance(SesionPersona sesion, SesionFlujoWeb flujoWeb) {
		var algo = Algoritmo();
		var paso = algo.Pasos[flujoWeb.Decision];

		if (paso == null)
			throw new ArgumentException(nameof(paso));

		var res = EvaluarPaso(paso, flujoWeb, sesion);
		if (res.Accion == "next")
			return Avance(sesion, flujoWeb);
		return res;
	}

	public AccionFlujo EvaluarPaso(Step paso, SesionFlujoWeb config, SesionPersona sesion) {
		int score = sesion.Score ?? 0;

		if (config.Respuestas > 0 && config.Respuestas == config.Lista.Count)
			return new AccionFlujo("fin");

		// cuestionario
		if (sesion.CuestionarioId.HasValue
			&& config.Respuestas == config.CuestionarioPos
			&& string.IsNullOrEmpty(sesion.RespuestaCuestionario))
			return new AccionFlujo("cuestionario");

		if (paso.Condicion == "fin")
			return new AccionFlujo("fin");


		if (paso.DebeAsignar) {
			Asignar(paso.Tomar, config);
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

		// switch
		Step? paraAsignar = null;
		foreach (var p in paso.Deciciones) {
			var pasa = EvaluarScore(p.Condicion, score);
			if (pasa) {
				paraAsignar = p;
				break;
			}
		}

		if (paraAsignar == null)
			return new AccionFlujo("error"); ;
		Asignar(paraAsignar.Tomar, config);
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
		lista = lista.OrderBy(x => rng.Next()).ToList();
		conf.EnCola.AddRange(lista);
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
			case "default": return true;
			default: return false;
		}
	}

	public StepBuilder Algoritmo() {
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

public class AccionFlujo {
	public string Accion { get; set; }
	public int? PreguntaId { get; set; }

	public AccionFlujo(string accion, int? preguntaId = null) {
		Accion = accion;
		PreguntaId = preguntaId;
	}
}

public class Step {
	public int? CheckNum { get; set; }
	public string Condicion { get; set; } = "";
	public List<string> Tomar { get; set; } = new();
	public List<Step> Deciciones { get; set; } = new();

	public bool DebeAsignar => Tomar.Count > 0;
	public bool EsSwitch => Deciciones.Count > 0;
	public bool Contar => CheckNum.HasValue && CheckNum > 0;

}

public class StepBuilder {
	public List<Step> Pasos { get; set; } = new List<Step>();

	public StepBuilder Asignar(params string[] dificultad) {
		Pasos.Add(new Step {
			Tomar = dificultad.ToList()
		});
		return this;
	}

	public StepBuilder ContarRespuestas(int num) {
		Pasos.Add(new Step { CheckNum = num });
		return this;
	}

	public StepBuilder Condicion(string cond, params string[] dificultad) {
		Pasos.Add(new Step {
			Condicion = cond,
			Tomar = dificultad.ToList()
		});
		return this;
	}

	public StepBuilder Switch(Action<StepBuilder> opciones) {
		if (opciones == null)
			throw new ArgumentNullException();
		var inner = new StepBuilder();
		opciones(inner);
		Pasos.Add(new Step {
			Deciciones = inner.Pasos
		});
		return this;
	}

}


