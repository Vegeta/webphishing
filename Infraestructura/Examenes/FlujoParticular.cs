using Domain;
using Infraestructura.Examenes.Asignacion;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Examenes;

/// <summary>
/// Asignador para Examenes configuradose especificos
/// </summary>
public class FlujoParticular : IFlujoExamen {
	private readonly AppDbContext _db;
	private static readonly Random Rng = new();
	public AlgoritmoAsignacion Algoritmo { get; }
	private ConfigExamen _config;

	public FlujoParticular(AppDbContext db, ConfigExamen config) {
		_db = db;
		_config = config;
		Algoritmo = new AlgoritmoAsignacion();
	}

	public FlujoExamenDto CrearFlujo() {
		var flujo = new FlujoExamenDto {
			Aleatorio = _config.Aleatorio,
			CuestionarioPos = _config.CuestionarioPos,
			Tipo = TipoExamen.Personalizado,
			ExamenId = _config.IdExamen
		};

		var preguntas = Preguntas();
		if (_config.Aleatorio)
			preguntas = preguntas.OrderBy(_ => Rng.Next()).ToList();

		flujo.NumPreguntas = preguntas.Count;

		foreach (var item in preguntas) {
			flujo.MaxScore += DificultadPregunta.ScoreRespuesta(item.Dificultad);
			flujo.Pasos.Add(item);
		}
		return flujo;
	}

	private IList<PasoExamen> Preguntas() {
		return _db.ExamenPregunta
			.Include(x => x.Pregunta)
			.Where(x => x.ExamenId == _config.IdExamen)
			.OrderBy(x => x.Orden)
			.Select(x => new PasoExamen {
				Accion = "pregunta",
				EntidadId = x.PreguntaId,
				Dificultad = x.Pregunta.Dificultad ?? "facil",
				Real = x.Pregunta.Legitimo == 0 ? "phishing" : "legitimo",
			}).ToList();
	}

	public void ResolverPreguntas(FlujoExamenDto flujo) {
		// TODO tqal vez usar el algoritmo si se configura?
	}
}