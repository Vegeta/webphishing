using AutoMapper;
using Domain;
using Infraestructura;
using Infraestructura.Filtros;
using Infraestructura.Persistencia;
using Infraestructura.Reportes;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Webapp.Controllers;
using Webapp.Models;
using Webapp.Models.Datatables;

namespace Webapp.Areas.Manage.Controllers;

public class EvaluacionesController : BaseAdminController {
	private readonly AppDbContext _db;
	private readonly IMapper _mapper;
	private readonly ConsultasService _consultas;

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Evaluaciones", "/Manage/Evaluaciones");
		CurrentMenu("Reportes/Evaluaciones");
		Titulo("Resultados evaluaciones");
	}

	public EvaluacionesController(AppDbContext db, IMapper mapper, ConsultasService consultas) {
		_db = db;
		_mapper = mapper;
		_consultas = consultas;
	}

	public IActionResult Index() {
		var vm = new FiltroCatalogoVm {
			Meses = OpcionesConfig.MesesWeb()
		};
		var mapaActividad = new CatalogoGeneral(_db).Carreras()
			.ToDictionary(keySelector: m => m, elementSelector: m => m);
		vm.Actividades = OpcionesConfig.ComboDict(mapaActividad);
		vm.Generos = OpcionesConfig.ComboDict(Generos.Mapa(), "");
		vm.Ocupaciones = OpcionesConfig.ComboDict(Ocupaciones.Mapa(), "");

		return View(vm);
	}

	[HttpPost]
	public IActionResult Lista(DatatablesModel model) {
		var filtros = model.GetFiltros(new FiltroEvaluacion());

		var orden = model.FirstOrder();
		filtros.OrdenCampo = orden.Campo;
		filtros.OrdenDir = orden.Dir;

		var q = _consultas.Sesiones(filtros);

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
			x.Estado,
			x.Percepcion
		});

		var paged = proj.GetPaged(model.Page, model.Length);

		var res = ResultPagedHelpers.FromPaged(paged, model.Draw);
		return Ok(res);
	}

	public IActionResult Detalle(int id) {
		Titulo("Resultado Evaluación");
		var ses = _db.VSesiones
			.First(x => x.Id == id);

		var con = new ConsultaEvaluacion(_db);
		var respuestas = con.RespuestasWeb(ses.Id);

		var cuest = _db.CuestionarioRespuesta
			.Where(x => x.SesionId == ses.Id)
			.OrderBy(x => x.Id)
			.ToList();

		ViewBag.modelo = JSON.Stringify(ses);
		ViewBag.respuestas = JSON.Stringify(respuestas);
		ViewBag.percepcion = ses.RespuestaCuestionario ?? "[]";
		ViewBag.cuest = JSON.Stringify(cuest);

		return View();
	}

	public IActionResult preguntaDet(int id) {
		return Ok();
	}

	[HttpPost]
	public IActionResult Exportar(string filtros) {
		var filObj = JSON.Parse<FiltroEvaluacion>(filtros);

		var query = _consultas.Sesiones(filObj);
		var exportador = new ExportarSesiones();
		using var wb = exportador.Exportar(query);
		using var stream = new MemoryStream();
		wb.SaveAs(stream);
		var content = stream.ToArray();
		return File(
			content,
			ExcelUtils.TipoMime,
			"sesiones.xlsx"
		);
	}
}

public class FiltroCatalogoVm {
	public List<SelectListItem> Meses { get; set; } = new();
	public List<SelectListItem> Generos { get; set; } = new();
	public List<SelectListItem> Ocupaciones { get; set; } = new();
	public List<SelectListItem> Actividades { get; set; } = new();
}