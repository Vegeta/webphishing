using AutoMapper;
using Infraestructura;
using Infraestructura.Filtros;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Webapp.Controllers;
using Webapp.Models.Datatables;

namespace Webapp.Areas.Manage.Controllers;

public class EvaluacionesController : BaseAdminController {
	AppDbContext _db;
	private readonly IMapper _mapper;
	private readonly EvaluacionesService _sesiones;

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Evaluaciones", "/Manage/Evaluaciones");
		CurrentMenu("Reportes/Evaluaciones");
		Titulo("Resultados evaluaciones");
	}

	public EvaluacionesController(AppDbContext db, IMapper mapper, EvaluacionesService sesiones) {
		_db = db;
		_mapper = mapper;
		_sesiones = sesiones;
	}

	public IActionResult Index() {
		return View();
	}


	[HttpPost]
	public IActionResult Lista(DatatablesModel model) {
		var filtros = model.GetFiltros(new FiltroEvaluacion());

		var orden = model.FirstOrder();
		filtros.OrdenCampo = orden.Campo;
		filtros.OrdenDir = orden.Dir;


		var q = _sesiones.Sesiones(filtros);

		var proj = q.Select(x => new {
			id = x.Id,
			tipo = x.Tipo,
			nombreCompleto = $"{x.Apellidos} {x.Nombres}",
			email = x.Email,
			fechaExamen = x.FechaExamen.HasValue ? x.FechaExamen.Value.ToString("yyyy-MM-dd hh:mm") : "",
			tiempo = x.TiempoTotal,
			score = x.Score,
			exito = x.Exito,
			avgTiempo = x.AvgTiempo,
			avgScore = x.AvgScore,
		});

		var paged = proj.GetPaged(model.Page, model.Length);

		var res = ResultPagedHelpers.FromPaged(paged, model.Draw);
		return Ok(res);
	}

	public IActionResult Detalle(int id) {
		Titulo("Resultado Evaluación");
		var ses = _db.VSesiones
			.First(x => x.Id == id);
		var respuestas = _db.SesionRespuesta
			.Where(x => x.SesionId == ses.Id)
			.OrderBy(x => x.Inicio)
			.ToList();
		var flujo = ses.GetSesionFlujo();

		ViewBag.modelo = JSON.Stringify(ses);
		ViewBag.respuestas = JSON.Stringify(respuestas);
		ViewBag.percepcion = ses.RespuestaCuestionario ?? "null";
		ViewBag.flujo = JSON.Stringify(flujo);
		return View();
	}

}