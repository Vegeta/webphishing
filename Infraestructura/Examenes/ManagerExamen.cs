using Domain;
using Domain.Entidades;
using Infraestructura.Persistencia;

namespace Infraestructura.Examenes;

/// <summary>
/// Obnjeto principal de control para el flujo de evaluacion de examenes
/// </summary>
public class ManagerExamen {
	private readonly AppDbContext _db;

	public ManagerExamen(AppDbContext db) {
		_db = db;
	}

	public IFlujoExamen GetAsignador(ConfigExamen config) {
		if (config.Tipo == TipoExamen.Personalizado) {
			if (config.IdExamen == 0)
				throw new ArgumentException("Se requiere el id del examen para procesar");
			return new FlujoParticular(_db, config);
		}
		// por defecto
		var obj = new FlujoAleatorio(_db, config);
		return obj;
	}

	public FlujoExamenDto CrearFlujo(ConfigExamen config) {
		var asigna = GetAsignador(config);
		var flujo = asigna.CrearFlujo();
		CheckCuestionario(flujo);
		return flujo;
	}

	public PasoExamen RespuestaActual(ConfigExamen config, FlujoExamenDto flujo, string respuesta, float tiempo) {
		var paso = flujo.PasoActual();
		paso.Ejecutado = true;
		paso.Tiempo = tiempo;
		if (paso.Accion == "pregunta") {
			paso.Respuesta = respuesta;
			if (paso.Real == respuesta) {
				paso.Score = DificultadPregunta.ScoreRespuesta(paso.Dificultad);
				flujo.Score += paso.Score;
			}
		}

		var asignador = GetAsignador(config);
		asignador.ResolverPreguntas(flujo);
		CheckCuestionario(flujo);

		flujo.Inicio ??= DateTime.Now; // check inicio

		//if (flujo.IndicePaso + 1 < flujo.Pasos.Count)
		flujo.IndicePaso++;
		return paso;
	}

	public void CheckCuestionario(FlujoExamenDto flujo) {
		var pos = flujo.CuestionarioPos;
		if (pos == null)
			return;
		var check = flujo.Pasos.FirstOrDefault(x => x.Accion == "cuestionario");
		if (check != null)
			return;
		// el indice del cuestionario es 1 -based
		pos--;
		if (pos < 0)
			return;
		var idCues = SeleccionarCuestionario();
		if (idCues == 0)
			return;
		var item = new PasoExamen {
			Accion = "cuestionario",
			EntidadId = idCues,
		};
		var cuenta = flujo.Pasos.Count;
		if (pos < cuenta)
			flujo.Pasos.Insert(pos.Value, item);
		if (pos == cuenta + 1)
			flujo.Pasos.Add(item);
	}

	public int SeleccionarCuestionario() {
		// intenta recuperar solo el id del primer cuestionario, si no 0 y algo mas arriba
		// se hara cargo. En el futuro se podria tomar un id por ejemplo
		var cues = _db.Cuestionario.Select(x => new { x.Id })
			.OrderBy(x => x.Id)
			.FirstOrDefault();
		return cues?.Id ?? 0;
	}

	public bool FinalizarSesion(FlujoExamenDto flujo, SesionPersona sesion) {
		if (sesion.Estado == SesionPersona.EstadoTerminado)
			return false;
		var fin = DateTime.Now;
		flujo.Fin = fin;
		sesion.FechaFin = fin;
		var respuestas = flujo.Pasos
			.Where(x => x is { Accion: "pregunta", Ejecutado: true })
			.ToList();
		var scores = respuestas.Select(x => x.Score).ToList();
		var tiempos = respuestas.Select(x => x.Tiempo).ToList();

		sesion.AvgScore = (float?)scores.Average();
		sesion.AvgTiempo = tiempos.Average();
		var exitos = respuestas.Count(x => x.Score > 0);

		sesion.TiempoTotal = DbHelpers.DiferenciaSegundos(sesion.FechaExamen, sesion.FechaFin);

		sesion.MaxScore = flujo.MaxScore;
		sesion.Score = flujo.Score;
		var tasa = ((float)exitos / (float)respuestas.Count) * 100;
		sesion.Exito = Convert.ToInt32(tasa);
		sesion.Estado = SesionPersona.EstadoTerminado;
		return true;
	}
}