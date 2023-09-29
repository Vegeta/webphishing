using AutoMapper;
using Domain;
using Domain.Entidades;
using Infraestructura;
using Infraestructura.Examenes;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Webapp.Controllers;
using Webapp.Models;
using Webapp.Models.Evaluacion;


namespace Webapp.Areas.Manage.Controllers;

public class SimuladorController : BaseAdminController {
	AppDbContext _db;
	private readonly IMapper _mapper;
	private readonly ControlExamenService _control;
	private readonly ManagerExamen _manager;

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		Titulo("Simulador examenes");
	}

	public SimuladorController(AppDbContext db, IMapper mapper, ControlExamenService control, ManagerExamen manager) {
		_db = db;
		_mapper = mapper;
		_control = control;
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

		var model = new {
			id,
			nombre = "Simulacion",
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
		var r = new SesionRespuesta {
			Clicks = resp.Interaccion,
			PreguntaId = resp.PreguntaId,
			Respuesta = resp.Respuesta ?? "",
			Comentario = resp.Comentario,
			Inicio = resp.Inicio,
			Fin = resp.Fin,
			Tiempo = DbHelpers.DiferenciaSegundos(resp.Inicio, resp.Fin)
		};

		var flujo = resp.Estado ?? new FlujoExamenDto();

		var config = new ConfigExamen {
			CuestionarioPos = flujo.CuestionarioPos,
			Tipo = TipoExamen.Personalizado,
			Aleatorio = false,
			IdExamen = flujo.ExamenId
		};

		_manager.RespuestaActual(config, flujo, r.Respuesta, r.Tiempo ?? 0);

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

		var evaluador = new EvaluadorCuestionario(_db);
		flujo.DatosCuestionario = evaluador.EvaluarCuestionario(respuestas);

		var config = new ConfigExamen {
			CuestionarioPos = flujo.CuestionarioPos,
			Tipo = TipoExamen.Personalizado,
			Aleatorio = false,
			IdExamen = flujo.ExamenId
		};

		_manager.RespuestaActual(config, flujo, "OK", data.TiempoCuest ?? 0);
		flujo.DatosCuestionario.Respuestas = respuestas;

		return ContinuaFlujo(flujo);
	}

	protected object? DatosResultado(FlujoExamenDto flujo, SesionPersona sesion) {
		flujo.DatosCuestionario ??= new DataCuestionario();
		var vses = new VSesionPersona {
			Nombre = "Simulacion",
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

		var mapaRespuestas = flujo.Pasos.Where(x => x.Ejecutado && x.Accion == "pregunta")
			.ToDictionary(x => x.EntidadId);
		var ids = mapaRespuestas.Keys.ToList();

		var dbResp = _db.Pregunta.Where(x => ids.Contains(x.Id)).ToList();
		var respuestas = dbResp.Select(x => MapeoPregunta(mapaRespuestas, x)).ToList();

		var respCuest = flujo.DatosCuestionario.RespuestaCuestionario;

		var res = new {
			modelo = vses,
			respuestas,
			percepcion = respCuest == null ? new List<string>() : JSON.Parse<object>(respCuest),
			cuest = flujo.DatosCuestionario.Respuestas,
		};
		return res;
	}

	protected dynamic MapeoPregunta(IDictionary<int, PasoExamen> mapa, Pregunta x) {
		var res = mapa[x.Id];
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
		};
		return item;
	}
}