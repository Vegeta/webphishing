using Domain;
using Domain.Entidades;
using Infraestructura;
using Infraestructura.Examenes;
using Infraestructura.Persistencia;
using Infraestructura.Reportes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Webapp.Controllers;
using Webapp.Models;
using Webapp.Models.Evaluacion;


namespace Webapp.Areas.Manage.Controllers;

public class SimuladorController : BaseAdminController {
	readonly AppDbContext _db;
	private readonly ManagerExamen _manager;

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		Titulo("Simulador examenes");
	}

	public SimuladorController(AppDbContext db, ManagerExamen manager) {
		_db = db;
		_manager = manager;
	}

	public IActionResult Examen(int id) {
		var examen = _db.Examen.FirstOrDefault(x => x.Id == id);
		if (examen == null) {
			ErrorWeb("Examen no encontrado");
			return RedirectToAction("Index", "Examenes");
		}

		var config = new ConfigExamen {
			CuestionarioPos = examen.CuestionarioPos,
			Tipo = TipoExamen.Personalizado,
			Aleatorio = false,
			IdExamen = examen.Id,
		};

		var flujo = _manager.CrearFlujo(config);
		flujo.Inicio = DateTime.Now;

		var model = new {
			id,
			nombre = "Simulación",
			email = "prueba@prueba.com",
			total = flujo.NumPreguntas,
			indice = 0, // respondidas?
			token = "----",
			estado = flujo,
		};
		ViewBag.model = JSON.Stringify(model);
		ViewBag.cuest = "null";
		ViewBag.pregunta = "null";

		var cons = new ConsultaEvaluacion(_db);

		var paso = flujo.PasoActual();

		if (paso.Accion == "pregunta") {
			var data = cons.PreguntaData(paso.EntidadId);
			ViewBag.pregunta = JSON.Stringify(data);
		}

		if (paso.Accion == "cuestionario") {
			var cuest = cons.CuestionarioData();
			ViewBag.cuest = cuest == null ? "null" : JSON.Stringify(cuest);
		}

		return View();
	}

	[HttpPost]
	public IActionResult Respuesta([FromBody] RespuestaSim resp) {
		var tiempo = DbHelpers.DiferenciaSegundos(resp.Inicio, resp.Fin);
		var flujo = resp.Estado ?? new FlujoExamenDto();

		var config = new ConfigExamen {
			CuestionarioPos = flujo.CuestionarioPos,
			Tipo = TipoExamen.Personalizado,
			Aleatorio = false,
			IdExamen = flujo.ExamenId
		};

		// parse interacciones
		var inter = InteraccionesDto.Parse(resp.Interaccion);
		var mapa = JSON.Parse<MapaInteracciones>(flujo.ExtraData ?? "{}");
		mapa[resp.PreguntaId] = inter;
		flujo.ExtraData = JSON.Stringify(mapa);

		_manager.RespuestaActual(config, flujo, resp.Respuesta ?? "", tiempo);

		return ContinuaFlujo(flujo);
	}

	protected IActionResult ContinuaFlujo(FlujoExamenDto flujo) {
		var paso = flujo.PasoActual();
		var res = new AccionExamenWeb {
			Accion = paso.Accion,
			Indice = flujo.Respondidas,
			Estado = flujo
		};

		if (flujo.EsFin) {
			res.Accion = "fin";
			var sesion = new SesionPersona();
			_manager.FinalizarSesion(flujo, sesion);
			res.Data = DatosResultado(flujo, sesion);
			return Ok(res);
		}

		var cons = new ConsultaEvaluacion(_db);
		if (paso.Accion == "pregunta") {
			res.Data = cons.PreguntaData(paso.EntidadId);
		}

		if (paso.Accion == "cuestionario") {
			res.Data = cons.CuestionarioData();
		}

		return Ok(res);
	}

	public IActionResult ResponderCuestionario([FromBody] RespuestaCuest data) {
		var respuestas = data.Respuestas
			.Select(x => new CuestionarioRespuesta {
					Dimension = x.Dimension,
					Pregunta = x.Texto,
					Respuesta = x.Respuesta,
				}
			).ToList();

		var flujo = data.Estado ??= new FlujoExamenDto();

		var evaluador = new EvaluadorCuestionario();
		flujo.DatosCuestionario = evaluador.EvaluarCuestionario(respuestas);

		var config = new ConfigExamen {
			CuestionarioPos = flujo.CuestionarioPos,
			Tipo = TipoExamen.Personalizado,
			Aleatorio = false,
			IdExamen = flujo.ExamenId
		};

		_manager.RespuestaActual(config, flujo, "OK", data.TiempoCuest ?? 0);
		flujo.DatosCuestionario.Respuestas = respuestas;
		flujo.DatosCuestionario.TiempoCuestionario = data.TiempoCuest;

		return ContinuaFlujo(flujo);
	}

	protected object DatosResultado(FlujoExamenDto flujo, SesionPersona sesion) {
		flujo.DatosCuestionario ??= new DataCuestionario();
		var vses = new VSesionPersona {
			Nombre = "Simulación",
			Nombres = "prueba",
			Apellidos = "prueba",
			Score = flujo.Score,
			Estado = "terminado",
			AvgScore = sesion.AvgScore,
			AvgTiempo = sesion.AvgTiempo,
			FechaExamen = sesion.FechaExamen,
			FechaFin = sesion.FechaFin,
			Exito = sesion.Exito,
			TiempoCuestionario = flujo.DatosCuestionario.TiempoCuestionario,
			TiempoTotal = sesion.TiempoTotal,
			MaxScore = sesion.MaxScore,
			ScoreCuestionario = flujo.DatosCuestionario.ScoreCuestionario,
			Percepcion = flujo.DatosCuestionario.Percepcion,
		};

		var mapaInter = JSON.Parse<MapaInteracciones>(flujo.ExtraData ?? "{}");

		var mapaRespuestas = flujo.Pasos.Where(x => x.Ejecutado && x.Accion == "pregunta")
			.ToDictionary(x => x.EntidadId);
		var ids = mapaRespuestas.Keys.ToList();

		var dbResp = _db.Pregunta.Where(x => ids.Contains(x.Id)).ToList();
		var respuestas = dbResp.Select(x => MapeoPregunta(mapaRespuestas, x, mapaInter)).ToList();

		var respCuest = flujo.DatosCuestionario.RespuestaCuestionario;

		var interMan = new InteraccionesStats();
		var interStats = interMan.Calcular(mapaInter.Values.ToList());

		var res = new {
			modelo = vses,
			respuestas,
			percepcion = respCuest == null ? new List<string>() : JSON.Parse<object>(respCuest),
			cuest = flujo.DatosCuestionario.Respuestas,
			interStats
		};
		return res;
	}

	protected dynamic MapeoPregunta(IDictionary<int, PasoExamen> mapa, Pregunta x, MapaInteracciones interacciones) {
		var res = mapa[x.Id];

		var tablaInter = new List<FilaInter>();
		if (interacciones.ContainsKey(res.EntidadId)) {
			tablaInter = InteraccionesStats.TablaInter(interacciones[res.EntidadId]);
		}

		var item = new {
			x.ImagenRetro,
			x.Dificultad,
			x.Explicacion,
			x.Email,
			adjuntos = JSON.Parse<dynamic>(x.Adjuntos ?? "[]"),
			res.Respuesta,
			tipo = x.Legitimo == 0 ? "PHISHING" : "LEGITIMO",
			res.Score,
			PreguntaId = res.EntidadId,
			res.Tiempo,
			comentario = "Test comentario",
			interacciones = tablaInter
		};
		return item;
	}
}

public class MapaInteracciones : Dictionary<int, InteraccionesDto> {
}