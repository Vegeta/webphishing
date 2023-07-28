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
		var pregs = _control.Consultas().PreguntasExamen(id);
		pregs = pregs.OrderBy(_ => rng.Next()).ToList();
		var con = new ControlExamenService(_db);
		var estado = con.CrearEstado(pregs, examen.CuestionarioPos);

		var model = new {
			id,
			nombre = "Simulacion",
			email = "prueba@prueba.com",
			total = estado.Lista.Count,
			indice = estado.Respondidas,
			token = "----",
			estado,
		};
		ViewBag.model = JSON.Stringify(model);
		ViewBag.cuest = "null";
		ViewBag.pregunta = "null";

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

		return View();
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
		_control.ResponderOperacionActual(estado, r);
		estado.IndiceRespuesta++;

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
			estado.Fin = r.Fin;
			_control.FinalizarSesion(estado);
			res.Data = Resultados(estado);
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

		var estado = data.Estado;

		_control.EvaluarCuestionario(estado, respuestas);
		estado.CuestionarioHecho = true;
		estado.IndiceRespuesta++;
		estado.Inicio ??= DateTime.Now;
		estado.DatosCuestionario.Respuestas = respuestas;
		estado.DatosCuestionario.TiempoCuestionario = data.TiempoCuest;

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
			_control.FinalizarSesion(estado);
			res.Data = Resultados(estado);
		}

		return Ok(res);
	}

	protected object? Resultados(EstadoExamen estado) {
		estado.DatosCuestionario ??= new DataCuestionario();
		estado.DatosExamen ??= new DataExamen();
		var vses = new VSesionPersona {
			Nombre = "Simulacion",
			Nombres = "prueba",
			Apellidos = "prueba",
			Score = estado.Score,
			Estado = "terminado",
			AvgScore = estado.DatosExamen.AvgScore,
			AvgTiempo = estado.DatosExamen.AvgTiempo,
			FechaExamen = estado.Inicio,
			FechaFin = estado.Fin,
			Exito = estado.DatosExamen.Exito,
			TiempoCuestionario = estado.DatosCuestionario.TiempoCuestionario,
			TiempoTotal = estado.DatosExamen.TiempoTotal,
			MaxScore = estado.DatosExamen.MaxScore,
			ScoreCuestionario = estado.DatosCuestionario.ScoreCuestionario,
			Percepcion = estado.DatosCuestionario.Percepcion,
		};

		var mapaRespuestas = estado.Operaciones.Where(x => x.HayRespuesta)
			.ToDictionary(x => x.PreguntaId);
		var ids = mapaRespuestas.Keys.ToList();

		var dbResp = _db.Pregunta.Where(x => ids.Contains(x.Id)).ToList();
		var respuestas = dbResp.Select(x => MapeoPregunta(mapaRespuestas, x)).ToList();

		estado.DatosCuestionario ??= new DataCuestionario();

		var respCuest = estado.DatosCuestionario.RespuestaCuestionario;

		var res = new {
			modelo = vses,
			respuestas,
			percepcion = respCuest == null ? new List<string>() : JSON.Parse<object>(respCuest),
			cuest = estado.DatosCuestionario.Respuestas,
		};
		return res;
	}

	protected dynamic MapeoPregunta(IDictionary<int?, OperacionExamen> mapa, Pregunta x) {
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
			res.PreguntaId,
			res.Tiempo,
			comentario = "Test comentario",
		};
		return item;
	}

}

public class RespuestaWebSim : RespuestaWeb {
	public EstadoExamen? Estado { get; set; }
}

public class RespuestaCuest {
	public List<CuestRespuestaModel> Respuestas { get; set; } = new();
	public EstadoExamen? Estado { get; set; }
	public float? TiempoCuest { get; set; }
}

public class AccionWeb {
	public object? Data { get; set; }
	public string Accion { get; set; } = "";

	public object? Estado { get; set; }
	public string? Error { get; set; }
	public int Indice { get; set; }
	public string? Url { get; set; }
}