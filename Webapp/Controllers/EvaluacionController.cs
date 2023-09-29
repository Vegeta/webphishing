using AutoMapper;
using Domain;
using Domain.Entidades;
using Infraestructura;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Webapp.Areas.Manage.Controllers;
using Webapp.Models;

namespace Webapp.Controllers;

public class EvaluacionController : BaseController {
	private readonly AppDbContext _db;
	private readonly IMapper _mapper;
	private readonly CatalogoGeneral _cat;
	private readonly RegistroService _registro;
	private readonly FlujoExamen _flujo;
	private readonly ControlExamenService _control;

	public EvaluacionController(AppDbContext db,
		IMapper mapper, CatalogoGeneral cat, RegistroService registro, FlujoExamen flujo, ControlExamenService control) {
		_db = db;
		_mapper = mapper;
		_cat = cat;
		_registro = registro;
		_flujo = flujo;
		_control = control;
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

		var consultas = _control.Consultas();

		// si existe una sesion en curso, continue sin crear una nueva
		var check = consultas.SesionActualPersona(per.Id);
		if (check != null) {
			HttpContext.Session.SetString("token_examen", check.Token ?? "");
			return Ok(new { url, error = "" });
		}

		// nueva sesion rapida
		var con = new ConsultaEvaluacion(_db);
		var sesion = consultas.CrearSesion(res.Data, "rapida");
		sesion.CuestionarioId = con.IdPrimercuestionario();
		var pregs = consultas.PreguntasRandom(10);
		var estado = _control.CrearEstado(pregs, ControlExamenService.PosCuestionario);
		sesion.MaxScore = estado.DatosExamen?.MaxScore;
		_db.SesionPersona.Add(sesion);
		SaveSession(sesion, estado);

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

		var estado = local.Sesion.GetSesionFlujo();

		var model = new {
			nombre,
			email = per.Email,
			total = estado.Lista.Count,
			indice = estado.Respondidas,
			token = local.Sesion.Token
		};

		ViewBag.model = JSON.Stringify(model);
		ViewBag.cuest = "null";
		ViewBag.pregunta = "{}";
		ViewBag.reporte = "null";

		var cons = new ConsultaEvaluacion(_db);
		var op = estado.OperacionActual();

		if (op.Accion == "pregunta") {
			var data = cons.PreguntaData(op.PreguntaId.Value);
			ViewBag.pregunta = JSON.Stringify(data);
		}

		if (op.Accion == "cuestionario") {
			var cuest = cons.CuestionarioData();
			ViewBag.cuest = cuest == null ? "null" : JSON.Stringify(cuest);
		}

		if (op.Accion == "fin") {
			ViewBag.reporte = JSON.Stringify(Resultados(local.Sesion));
		}

		return View();
	}

	protected void SaveSession(SesionPersona ses, EstadoExamen estado) {
		ses.FechaActividad = DateTime.Now;
		estado.DatosCuestionario = null;
		estado.DatosExamen = null;
		ses.Flujo = JSON.Stringify(estado);
		_db.SaveChanges();
	}

	[HttpPost]
	public IActionResult Respuesta([FromBody] RespuestaWeb resp) {
		var local = SesionActual();
		if (local.HasError) {
			ErrorWeb(local.Error);
			return Problem();
		}

		var r = new SesionRespuesta {
			Clicks = resp.Interaccion,
			PreguntaId = resp.PreguntaId,
			Respuesta = resp.Respuesta,
			Comentario = resp.Comentario,
			Inicio = resp.Inicio,
			Fin = resp.Fin,
		};

		var estado = local.Sesion.GetSesionFlujo();
		_control.ResponderOperacionActual(estado, r, local.Sesion);
		estado.IndiceRespuesta++;

		// persist
		r.SesionId = local.Sesion.Id;
		_db.SesionRespuesta.Add(r);
		SaveSession(local.Sesion, estado);

		var op = estado.OperacionActual();
		var res = new AccionWeb {
			Accion = op.Accion,
			Indice = estado.Respondidas,
		};

		var cons = new ConsultaEvaluacion(_db);
		if (op.Accion == "pregunta") {
			res.Data = cons.PreguntaData(op.PreguntaId.Value);
		}

		if (op.Accion == "cuestionario") {
			res.Data = cons.CuestionarioData();
		}

		if (op.Accion == "fin") {
			estado.Fin = r.Fin;
			_control.FinalizarSesion(estado, local.Sesion);
			SaveSession(local.Sesion, estado);
			res.Data = Resultados(local.Sesion);
		}

		return Ok(res);
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

		var estado = local.Sesion.GetSesionFlujo();

		_control.EvaluarCuestionario(estado, respuestas, local.Sesion);
		estado.CuestionarioHecho = true;
		estado.IndiceRespuesta++;
		estado.Inicio ??= DateTime.Now;

		// persist
		local.Sesion.TiempoCuestionario = data.TiempoCuest;
		local.Sesion.FechaExamen ??= estado.Inicio;
		foreach (var respuesta in respuestas) {
			respuesta.SesionId = local.Sesion.Id;
			_db.CuestionarioRespuesta.Add(respuesta);
		}
		SaveSession(local.Sesion, estado);

		var op = estado.OperacionActual();

		var res = new AccionWeb {
			Accion = op.Accion,
			Indice = estado.Respondidas,
			Estado = estado,
		};

		var cons = new ConsultaEvaluacion(_db);
		if (op.Accion == "pregunta") {
			res.Data = cons.PreguntaData(op.PreguntaId.Value);
		}

		if (op.Accion == "cuestionario") {
			res.Data = cons.CuestionarioData();
		}

		if (op.Accion == "fin") {
			estado.Fin = DateTime.Now;
			_control.FinalizarSesion(estado, local.Sesion);
			SaveSession(local.Sesion, estado);
			res.Data = Resultados(local.Sesion);
		}

		return Ok(res);
	}

	protected object Resultados(SesionPersona sesion) {
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

	protected InfoExamen SesionActual() {
		var res = new InfoExamen();
		var token = HttpContext.Session.GetString("token_examen");
		if (string.IsNullOrEmpty(token)) {
			res.Error = "No existe examen iniciado";
			return res;
		}

		res.Token = token;
		var ses = _control.Consultas().SesionPorToken(token);
		if (ses == null) {
			res.Error = "No se encontró el examen requerido";
			return res;
		}

		res.Sesion = ses;
		return res;
	}
}

public class RespuestaWeb {
	public string Token { get; set; } = "";
	public int PreguntaId { get; set; }
	public DateTime? Inicio { get; set; }
	public DateTime? Fin { get; set; }
	public string? Respuesta { get; set; }
	public string? Comentario { get; set; }
	public string? Interaccion { get; set; } // esto es el JSON de clicks y focus
}

public class InfoExamen {
	public SesionPersona Sesion { get; set; } = new();
	public string Error { get; set; } = "";
	public string Token { get; set; } = "";
	public bool HasError => !string.IsNullOrEmpty(Error);
}

public class InicioModel {
	public List<SelectListItem> Generos { get; set; } = new();
	public List<SelectListItem> Ocupaciones { get; set; } = new();
	public List<SelectListItem> Actividades { get; set; } = new();
	public List<SelectListItem> Anios { get; set; } = new();
}