using AutoMapper;
using Domain;
using Domain.Entidades;
using Infraestructura;
using Infraestructura.Examenes;
using Infraestructura.Persistencia;
using Infraestructura.Reportes;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Webapp.Models;
using Webapp.Models.Evaluacion;

namespace Webapp.Controllers;

public class EvaluacionController : BaseController {
	private readonly AppDbContext _db;
	private readonly IMapper _mapper;
	private readonly CatalogoGeneral _cat;
	private readonly RegistroService _registro;
	private readonly ManagerExamen _manager;

	public EvaluacionController(AppDbContext db,
		IMapper mapper, CatalogoGeneral cat, RegistroService registro, ManagerExamen manager) {
		_db = db;
		_mapper = mapper;
		_cat = cat;
		_registro = registro;
		_manager = manager;
	}

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		Breadcrumbs.Add("Empezar", "/Empezar");
		Titulo("Evaluación");
	}

	public IActionResult Index() {
		var local = SesionActual();
		if (!local.HasError)
			return RedirectToAction("Proceso");

		var vm = new InicioModel();

		var vmReg = new RegistroModel();
		var mapaActividad = _cat.Carreras()
			.ToDictionary(keySelector: m => m, elementSelector: m => m);

		vm.Actividades = OpcionesConfig.ComboDict(mapaActividad, "");
		vm.Generos = OpcionesConfig.ComboDict(Generos.Mapa(), "");
		vm.Ocupaciones = OpcionesConfig.ComboDict(Ocupaciones.Mapa(), "");

		vm.Anios.Add(new SelectListItem());
		for (var i = 1; i < 20; i++) {
			vm.Anios.Add(new SelectListItem(i.ToString(), i.ToString()));
		}
		vm.Anios.Add(new SelectListItem("20 o más", "20"));

		HttpContext.Session.Remove("token_examen");

		ViewBag.registro = ToJson(vmReg);
		return View(vm);
	}

	[HttpPost]
	public IActionResult Registrar([FromBody] RegistroModel model) {
		var local = SesionActual();
		var url = Url.Content("~/Evaluacion/Proceso");
		if (!local.HasError) {
			return Ok(new {
				mensaje = "Tiene una sesión de evaluación en curso",
				url
			});
		}

		var per = _mapper.Map(model, new Persona());
		per.Nombre = per.Nombre?.ToUpperInvariant();
		per.Apellido = per.Apellido?.ToUpperInvariant();
		var res = _registro.RegistrarPersona(per);

		if (res.Data == null)
			return Problem("Error creando persona");

		per = res.Data;

		var consultas = new ConsultasExamen(_db);

		// si existe una sesion en curso, continue sin crear una nueva
		var check = consultas.SesionActualPersona(per.Id);
		if (check != null) {
			HttpContext.Session.SetString("token_examen", check.Token ?? "");
			return Ok(new { url, error = "" });
		}

		// nueva sesion rapida
		var sesion = consultas.CrearSesion(res.Data, "rapida");

		// TODO esto deberia salir de alguna configuracion del sistema mismo	
		var config = new ConfigExamen {
			NumPreguntas = 3,
			CuestionarioPos = 2,
			Aleatorio = true,
			Tipo = TipoExamen.Predeterminado,
		};

		// crear el flujo y la sesion y persistir
		var flujo = _manager.CrearFlujo(config);
		sesion.Flujo = JSON.Stringify(flujo);
		_db.SesionPersona.Add(sesion);
		SaveSession(sesion, flujo);

		HttpContext.Session.SetString("token_examen", sesion.Token ?? "");
		return Ok(new { url, error = "" });
	}

	public IActionResult Proceso() {
		var local = SesionActual();
		if (local.HasError) {
			ErrorWeb(local.Error);
			return RedirectToAction("Index");
		}

		var nombre = "SIN PERSONA";
		var per = _db.Persona.FirstOrDefault(x => x.Id == local.Sesion.PersonaId);
		if (per != null) {
			nombre = $"{per.Nombre} {per.Apellido}";
		}

		var flujo = local.Sesion.GetSesionFlujo();

		var model = new {
			nombre,
			email = per?.Email,
			total = flujo.NumPreguntas,
			indice = flujo.Respondidas,
			token = local.Sesion.Token
		};

		ViewBag.model = JSON.Stringify(model);
		ViewBag.cuest = "null";
		ViewBag.pregunta = "{}";
		ViewBag.reporte = "null";

		ContinuaFlujo(flujo, local.Sesion, true);
		return View();
	}

	protected void SaveSession(SesionPersona ses, FlujoExamenDto flujo) {
		ses.FechaActividad = DateTime.Now;
		ses.Flujo = JSON.Stringify(flujo);
		_db.SaveChanges();
	}

	protected AccionExamenWeb ContinuaFlujo(FlujoExamenDto flujo, SesionPersona sesion, bool setViewbag = false) {
		var paso = flujo.PasoActual();
		var res = new AccionExamenWeb {
			Accion = paso.Accion,
			Indice = flujo.Respondidas,
		};

		if (flujo.EsFin) {
			res.Accion = "fin";
			if (_manager.FinalizarSesion(flujo, sesion))
				SaveSession(sesion, flujo);
			res.Data = Resultados(sesion);
			if (setViewbag)
				ViewBag.reporte = JSON.Stringify(res.Data);
			return res;
		}

		var cons = new ConsultaEvaluacion(_db);
		if (paso.Accion == "pregunta") {
			res.Data = cons.PreguntaData(paso.EntidadId);
			if (setViewbag)
				ViewBag.pregunta = JSON.Stringify(res.Data);
		}

		if (paso.Accion == "cuestionario") {
			res.Data = cons.CuestionarioData();
			if (setViewbag)
				ViewBag.cuest = JSON.Stringify(res.Data);
		}

		return res;
	}

	[HttpPost]
	public IActionResult Respuesta([FromBody] RespuestaWeb resp) {
		var local = SesionActual();
		if (local.HasError) {
			ErrorWeb(local.Error);
			return Problem();
		}

		var flujo = local.Sesion.GetSesionFlujo();

		var r = new SesionRespuesta {
			Interacciones = resp.Interaccion,
			PreguntaId = resp.PreguntaId,
			Respuesta = resp.Respuesta,
			Comentario = resp.Comentario,
			Inicio = resp.Inicio,
			Fin = resp.Fin,
			Tiempo = DbHelpers.DiferenciaSegundos(resp.Inicio, resp.Fin)
		};

		var config = new ConfigExamen {
			CuestionarioPos = flujo.CuestionarioPos,
			Tipo = flujo.Tipo,
			Aleatorio = flujo.Aleatorio,
			IdExamen = local.Sesion.ExamenId ?? 0
		};

		var paso = _manager.RespuestaActual(config, flujo, resp.Respuesta ?? "", r.Tiempo ?? 0);
		r.SesionId = local.Sesion.Id;
		r.Dificultad = paso.Dificultad;
		r.Score = paso.Score;
		local.Sesion.Score = flujo.Score;
		local.Sesion.MaxScore = flujo.MaxScore; // un tipo es adaptativo

		// parse interacciones
		if (!string.IsNullOrEmpty(resp.Interaccion)) {
			var inter = InteraccionesDto.Parse(resp.Interaccion);
			var man = new InteraccionesStats();
			man.CompletarRespuesta(r, inter);
		}

		// persist
		_db.SesionRespuesta.Add(r);
		SaveSession(local.Sesion, flujo);

		return Ok(ContinuaFlujo(flujo, local.Sesion));
	}

	public IActionResult ResponderCuestionario([FromBody] RespuestaCuest data) {
		var local = SesionActual();
		if (local.HasError) {
			ErrorWeb(local.Error);
			return Problem();
		}

		var respuestas = data.Respuestas
			.Select(x => new CuestionarioRespuesta {
					Dimension = x.Dimension,
					Pregunta = x.Texto,
					Respuesta = x.Respuesta,
				}
			).ToList();

		var flujo = local.Sesion.GetSesionFlujo();

		var evaluador = new EvaluadorCuestionario();
		flujo.DatosCuestionario = evaluador.EvaluarCuestionario(respuestas, local.Sesion);

		var config = new ConfigExamen {
			CuestionarioPos = flujo.CuestionarioPos,
			Tipo = flujo.Tipo,
			Aleatorio = flujo.Aleatorio,
			IdExamen = local.Sesion.ExamenId ?? 0
		};
		_manager.RespuestaActual(config, flujo, "OK", data.TiempoCuest ?? 0);

		// persist
		local.Sesion.TiempoCuestionario = data.TiempoCuest;
		local.Sesion.FechaExamen ??= flujo.Inicio;
		foreach (var respuesta in respuestas) {
			respuesta.SesionId = local.Sesion.Id;
			_db.CuestionarioRespuesta.Add(respuesta);
		}
		SaveSession(local.Sesion, flujo);

		return Ok(ContinuaFlujo(flujo, local.Sesion));
	}

	private dynamic Resultados(SesionPersona sesion) {
		var ses = _db.VSesiones
			.First(x => x.Id == sesion.Id);

		var con = new ConsultaEvaluacion(_db);
		var respuestas = con.RespuestasWeb(ses.Id);

		var data = new {
			modelo = ses,
			respuestas,
			percepcion = ses.RespuestaCuestionario != null ? JSON.Parse<object>(ses.RespuestaCuestionario) : new List<string>(),
			cuest = new List<string>(), // placeholder
		};
		return data;
	}

	public IActionResult Print() {
		var local = SesionActual();
		if (local.HasError) {
			ErrorWeb(local.Error);
			return Problem();
		}
		Titulo("Resultados");
		var res = Resultados(local.Sesion);
		ViewBag.modelo = JSON.Stringify(res.modelo);
		ViewBag.respuestas = JSON.Stringify(res.respuestas);
		ViewBag.percepcion = JSON.Stringify(res.percepcion);
		return View();
	}

	[HttpPost]
	public IActionResult Terminar() {
		var local = SesionActual();
		if (local.HasError) {
			ErrorWeb(local.Error);
			return Problem(local.Error);
		}

		HttpContext.Session.Remove("token_examen");

		var res = new {
			url = Url.Content("~/")
		};
		return Ok(res);
	}

	private InfoExamen SesionActual() {
		var res = new InfoExamen();
		var token = HttpContext.Session.GetString("token_examen");
		if (string.IsNullOrEmpty(token)) {
			res.Error = "No existe examen iniciado";
			return res;
		}

		res.Token = token;
		var ses = new ConsultasExamen(_db).SesionPorToken(token);
		if (ses == null) {
			res.Error = "No se encontró el examen requerido";
			return res;
		}

		res.Sesion = ses;
		return res;
	}
}

public class InicioModel {
	public List<SelectListItem> Generos { get; set; } = new();
	public List<SelectListItem> Ocupaciones { get; set; } = new();
	public List<SelectListItem> Actividades { get; set; } = new();
	public List<SelectListItem> Anios { get; set; } = new();
}