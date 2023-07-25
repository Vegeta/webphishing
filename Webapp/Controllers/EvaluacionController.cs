using AutoMapper;
using Domain;
using Domain.Entidades;
using Domain.Transferencia;
using Infraestructura;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Webapp.Models;

namespace Webapp.Controllers;

public class EvaluacionController : BaseController {
	private readonly ILogger<HomeController> _logger;
	private readonly AppDbContext _db;
	private readonly IMapper _mapper;
	private readonly CatalogoGeneral _cat;
	private readonly RegistroService _registro;
	private readonly FlujoExamen _flujo;

	public EvaluacionController(ILogger<HomeController> logger, AppDbContext db,
		IMapper mapper, CatalogoGeneral cat, RegistroService registro, FlujoExamen flujo) {
		_logger = logger;
		_db = db;
		_mapper = mapper;
		_cat = cat;
		_registro = registro;
		_flujo = flujo;
	}

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		Breadcrumbs.Add("Empezar", "/Empezar");
		Titulo("Resultados tests");
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

		var check = _flujo.SesionActualPersona(res.Data.Id);
		if (check != null) {
			HttpContext.Session.SetString("token_examen", check.Token ?? "");
			return Ok(new { url, error = "" });
		}

		var creacion = _flujo.IniciarSesionIndividual(res.Data);
		if (creacion.Sesion == null) {
			return Problem("Error creando sesion");
		}

		HttpContext.Session.SetString("token_examen", creacion.Sesion.Token ?? "");
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
			email = per?.Email ?? "prueba@prueba.com",
			total = flujo.Lista.Count,
			respuestas = flujo.Respuestas,
			cuestionario = flujo.TocaCuestionario(local.Sesion),
			token = local.Token,
		};
		ViewBag.modelo = JSON.Stringify(model);
		return View();
	}

	public IActionResult Pregunta(int id) {
		var p = _db.Pregunta.FirstOrDefault(x => x.Id == id);
		if (p != null)
			return Problem("pregunta no encontrada");

		var html = p.Html ?? "";
		var adjuntos = new List<AdjuntoView>();
		if (!string.IsNullOrEmpty(p.Adjuntos)) {
			adjuntos = FromJson<List<AdjuntoView>>(p.Adjuntos);
		}
		var model = new {
			p.Subject,
			p.Sender,
			p.Email,
			adjuntos,
		};
		return Ok(model);
	}

	protected PreguntaModelWeb DatosPregunta(int id) {
		var p = _db.Pregunta.Find(id);
		if (p == null)
			return new PreguntaModelWeb();
		var model = _mapper.Map<PreguntaModelWeb>(p);
		if (!string.IsNullOrEmpty(p?.Adjuntos)) {
			model.ListaAdjuntos = FromJson<List<AdjuntoView>>(p.Adjuntos);
		}
		return model;
	}

	[HttpPost]
	public IActionResult Avance() {
		var local = SesionActual();
		if (local.HasError) {
			return Ok(new { error = local.Error });
		}

		var flujo = local.Sesion.GetSesionFlujo();
		var pregunta = flujo.Siguiente();

		dynamic? vpregunta = null;

		if (pregunta != null) {
			var vm = DatosPregunta(pregunta.Id);
			vpregunta = new {
				vm.Subject,
				vm.Email,
				vm.Html,
				vm.Sender,
				adjuntos = vm.ListaAdjuntos,
			};
		}

		var model = new {
			preguntaId = pregunta?.Id,
			total = flujo.Lista.Count,
			respuestas = flujo.Respuestas,
			cuestionario = flujo.TocaCuestionario(local.Sesion),
			token = local.Token,
			mensaje = vpregunta,
			fin = flujo.Respuestas == flujo.Lista.Count,
		};

		return Ok(model);
	}

	public IActionResult Respuesta([FromBody] RespuestaWeb resWeb) {
		var sesion = _flujo.SesionPorToken(resWeb.Token);
		var resp = new SesionRespuesta();
		resp.SesionId = sesion.Id;
		resp.PreguntaId = resWeb.PreguntaId;
		resp.Inicio = resWeb.Inicio;
		resp.Fin = resWeb.Fin;
		resp.Comentario = resWeb.Comentario;
		resp.Respuesta = resWeb.Respuesta ?? "na";
		resp.Clicks = resWeb.Interaccion;

		if (resp.Inicio.HasValue && resp.Fin.HasValue) {
			var ts = resp.Fin - resp.Inicio;
			resp.Tiempo = (float)ts.Value.TotalSeconds;
		}

		var accion = _flujo.RegistrarRespuesta(sesion, resp);
		return Ok(accion);
	}

	public IActionResult Cuestionario() {
		var local = SesionActual();
		if (local.HasError)
			return Ok(new { error = local.Error });

		var cues = _db.Cuestionario
			.AsNoTracking()
			.FirstOrDefault(x => x.Id == local.Sesion.CuestionarioId);

		if (cues == null)
			return Ok(new { error = "No encontrado" });

		var data = new {
			preguntas = JSON.Parse<dynamic>(cues.Preguntas ?? "[]"),
			cues.Titulo,
			cues.Instrucciones,
			opciones = OpcionesConfig.ComboDict(RespuestaCuestionario.Mapa()),
		};
		return Ok(data);
	}

	public IActionResult ResponderCuestionario(string? data) {
		var local = SesionActual();
		if (local.HasError)
			return Ok(new { error = local.Error });
		
		var lista = JSON.Parse<List<CuestRespuestaModel>>(data ?? "[]");
		var respuestas = lista.Select(x => new CuestionarioRespuesta {
				SesionId = local.Sesion.Id,
				Dimension = x.Dimension,
				Pregunta = x.Texto,
				Respuesta = x.Respuesta,
			}
		).ToList();

		var accion = _flujo.ResponderCuestionario(local.Sesion, respuestas);
		return Ok(accion);
	}

	public IActionResult Resultados() {
		var local = SesionActual();
		if (local.HasError) {
			ErrorWeb(local.Error);
			return RedirectToAction("Index");
		}

		var detalle = _db.SesionRespuesta
			.Include(x => x.Pregunta)
			.Where(x => x.SesionId == local.Sesion.Id)
			.Select(x => new {
				x.Pregunta.ImagenRetro,
				x.Pregunta.Dificultad,
				x.Pregunta.Explicacion,
				x.Pregunta.Email,
				adjuntos = JSON.Parse<dynamic>(x.Pregunta.Adjuntos ?? "[]"),
				x.Respuesta,
				real = x.Pregunta.Legitimo == 0 ? "phish" : "legitimo",
				x.Score,
				x.Tiempo,
				x.Comentario,
				x.PreguntaId
			});
		var data = new {
			local.Sesion.Score,
			local.Sesion.AvgScore,
			local.Sesion.AvgTiempo,
			local.Sesion.TiempoTotal,
			TasaExito = local.Sesion.Exito,
			//cuestionario = local.Sesion.RespuestaCuestionario,
			cuestionario = JSON.Parse<dynamic>(local.Sesion.RespuestaCuestionario ?? "[]"),
			detalle,
		};


		ViewBag.model = JSON.Stringify(data);
		return View();
	}

	protected InfoExamen SesionActual() {
		var res = new InfoExamen();
		var token = HttpContext.Session.GetString("token_examen");
		if (string.IsNullOrEmpty(token)) {
			res.Error = "No existe examen iniciado";
			return res;
		}

		res.Token = token;
		var ses = _flujo.SesionPorToken(token);
		if (ses == null) {
			res.Error = "No se encontró el examen requerido";
			return res;
		}

		res.Sesion = ses;
		return res;
	}


}

public class RespuestaWeb {
	public string Token { get; set; }
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