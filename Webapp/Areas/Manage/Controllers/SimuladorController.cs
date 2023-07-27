using AutoMapper;
using Domain;
using Domain.Entidades;
using Domain.Transferencia;
using Infraestructura;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Webapp.Controllers;
using Webapp.Models;

namespace Webapp.Areas.Manage.Controllers;

public class SimuladorController : BaseAdminController {
	AppDbContext _db;
	private readonly IMapper _mapper;
	private readonly ControlExamenService _control;

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		Titulo("Simulador examenes");
	}

	public SimuladorController(AppDbContext db, IMapper mapper, ControlExamenService control) {
		_db = db;
		_mapper = mapper;
		_control = control;
	}

	public IActionResult Examen(int id) {
		var rng = new Random();
		var examen = _db.Examen.FirstOrDefault(x => x.Id == id);
		var pregs = _db.Pregunta
			.Where(x => x.ExamenPregunta.Any(y => y.ExamenId == id))
			.Select(x => new Pregunta {
				Id = x.Id,
				Dificultad = x.Dificultad ?? "facil"
			}).ToList();

		pregs = pregs.OrderBy(_ => rng.Next()).ToList();
		var con = new ControlExamenService(_db);
		var estado = con.CrearEstado(pregs);
		estado.CuestionarioPos = examen.CuestionarioPos;

		var model = new {
			id,
			nombre = "Usuario",
			email = "prueba@prueba.com",
			total = estado.Lista.Count,
			indice = estado.IndiceRespuesta,
			token = "----",
			estado,
		};
		ViewBag.model = JSON.Stringify(model);
		ViewBag.cuest = "null";
		ViewBag.pregunta = "null";

		if (estado.TocaCuestionario()) {
			var cuest = CuestionarioData();
			ViewBag.cuest = cuest == null ? "null" : JSON.Stringify(cuest);
		} else {
			var inicio = estado.PreguntaActual();
			var data = PreguntaData(inicio.Id);
			ViewBag.pregunta = JSON.Stringify(data);
		}
		return View();
	}

	protected object? PreguntaData(int id) {
		return _db.Pregunta
			.Where(x => x.Id == id)
			.Select(vm => new {
				vm.Id,
				vm.Subject,
				vm.Email,
				vm.Html,
				vm.Sender,
				adjuntos = JSON.Parse<List<AdjuntoView>>(vm.Adjuntos ?? "[]"),
			})
			.FirstOrDefault();
	}

	protected object? CuestionarioData() {
		var cues = _db.Cuestionario
			.AsNoTracking()
			.FirstOrDefault();

		return new {
			preguntas = JSON.Parse<List<CuestRespuestaModel>>(cues.Preguntas ?? "[]"),
			cues.Titulo,
			cues.Instrucciones,
			opciones = OpcionesConfig.ComboDict(RespuestaCuestionario.Mapa()),
		};
	}

	[HttpPost]
	public IActionResult Respuesta([FromBody] RespuestaWebSim resp) {
		var r = new SesionRespuesta {
			Clicks = resp.Interaccion,
			PreguntaId = resp.PreguntaId,
			Respuesta = resp.Respuesta,
			Comentario = resp.Comentario,
			Inicio = resp.Inicio,
			Fin = resp.Fin,
		};

		var estado = resp.Estado;
		var accion = _control.ResponderPregunta(resp.Estado, r);
		var res = new AccionWeb {
			Accion = accion.Accion,
			Estado = resp.Estado,
			Indice = resp.Estado.IndiceRespuesta
		};

		if (estado.TocaCuestionario()) {
			res.Accion = "cuestionario";
			res.Data = CuestionarioData();
			return Ok(res);
		}
		if (accion is { Accion: "pregunta", Pregunta: not null }) {
			res.Data = PreguntaData(accion.Pregunta.Id);
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

		var ses = new SesionPersona();
		_control.EvaluarCuestionario(ses, respuestas);
		data.Estado.CuestionarioHecho = true;

		var preg = data.Estado.PreguntaActual();
		if (preg == null) {
			return Ok(new AccionWeb { Accion = "fin", 
				Estado = data.Estado,
				Indice = data.Estado.IndiceRespuesta,
			});
		}

		var res = new AccionWeb {
			Accion = "pregunta",
			Data = PreguntaData(preg.Id),
			Indice = data.Estado.IndiceRespuesta,
			Estado = data.Estado
		};

		return Ok(res);
	}

	protected void Resultados(EstadoExamen estado) {
		
	}

}

public class RespuestaWebSim : RespuestaWeb {
	public EstadoExamen? Estado { get; set; }
}

public class RespuestaCuest {
	public List<CuestRespuestaModel> Respuestas { get; set; } = new();
	public EstadoExamen? Estado { get; set; }
}

public class AccionWeb {
	public object? Data { get; set; }
	public string Accion { get; set; } = "";

	public object? Estado { get; set; }
	public string? Error { get; set; }
	public int Indice { get; set; }
}