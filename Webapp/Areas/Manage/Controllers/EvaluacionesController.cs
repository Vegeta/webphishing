using AutoMapper;
using Domain;
using Domain.Entidades;
using Infraestructura;
using Infraestructura.Examenes;
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
	private readonly EvaluacionesService _servicio;
	private readonly IUserAccesor _users;

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Evaluaciones", "/Manage/Evaluaciones");
		CurrentMenu("Reportes/Evaluaciones");
		Titulo("Resultados evaluaciones");
	}

	public EvaluacionesController(AppDbContext db, IMapper mapper, EvaluacionesService servicio, IUserAccesor users) {
		_db = db;
		_mapper = mapper;
		_servicio = servicio;
		_users = users;
	}

	public IActionResult Index() {
		var vm = new PantallaManageVm {
			Meses = OpcionesConfig.MesesWeb(" ")
		};
		var mapaActividad = new CatalogoGeneral(_db).Carreras()
			.ToDictionary(keySelector: m => m, elementSelector: m => m);
		vm.Actividades = OpcionesConfig.ComboDict(mapaActividad);
		vm.Generos = OpcionesConfig.ComboDict(Generos.Mapa(), "");
		vm.Ocupaciones = OpcionesConfig.ComboDict(Ocupaciones.Mapa(), "");

		vm.Estados = SesionPersona.ListaEstados()
			.Select(x => new SelectListItem(x.ToUpper(), x)).ToList();
		vm.Estados.Insert(0, new SelectListItem());

		vm.Admin = _users.CurrentUser().TienePermisos("admin");

		return View(vm);
	}

	[HttpPost]
	public IActionResult Lista(DatatablesModel model) {
		var filtros = model.GetFiltros(new FiltroEvaluacion());

		var orden = model.FirstOrder();
		filtros.OrdenCampo = orden.Campo;
		filtros.OrdenDir = orden.Dir;

		var q = _servicio.Sesiones(filtros);

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
		var dtos = respuestas.Select(x => x.interacciones).Cast<InteraccionesDto>();

		var stats = new InteraccionesStats();
		var interStats = stats.TotalesLista(dtos.ToList());

		var cuest = _db.CuestionarioRespuesta
			.Where(x => x.SesionId == ses.Id)
			.OrderBy(x => x.Id)
			.ToList();

		ViewBag.modelo = JSON.Stringify(ses);
		ViewBag.respuestas = JSON.Stringify(respuestas);
		ViewBag.percepcion = ses.RespuestaCuestionario ?? "[]";
		ViewBag.cuest = JSON.Stringify(cuest);
		ViewBag.interStats = JSON.Stringify(interStats);
		ViewBag.admin = _users.CurrentUser().TienePermisos("admin");

		return View();
	}

	[HttpPost]
	public IActionResult Exportar(string filtros) {
		var filObj = JSON.Parse<FiltroEvaluacion>(filtros);

		var query = _servicio.Sesiones(filObj);
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

	public IActionResult Eliminar(int id) {
		var res = _servicio.EliminarEvaluacion(id);
		if (res.Valido)
			ConfirmaWeb("Evaluación eliminada");
		else
			ErrorWeb("Error eliminando evaluación");
		return RedirectToAction("Index");
	}

	[HttpPost]
	public IActionResult ExportarRespuestas() {
		var exportador = new ExportarRespuestas(_db);
		using var wb = exportador.Exportar();
		using var stream = new MemoryStream();
		wb.SaveAs(stream);
		var content = stream.ToArray();
		return File(
			content,
			ExcelUtils.TipoMime,
			"respuestas.xlsx"
		);
	}
	
	[HttpPost]
	public IActionResult ExportarCuestionario() {
		var exportador = new ExportarCuestionarioRespuestas(_db);
		using var wb = exportador.Exportar();
		using var stream = new MemoryStream();
		wb.SaveAs(stream);
		var content = stream.ToArray();
		return File(
			content,
			ExcelUtils.TipoMime,
			"respuestas_cuestionario.xlsx"
		);
	}
	
}

public class PantallaManageVm {
	public List<SelectListItem> Meses { get; set; } = new();
	public List<SelectListItem> Generos { get; set; } = new();
	public List<SelectListItem> Ocupaciones { get; set; } = new();
	public List<SelectListItem> Actividades { get; set; } = new();
	public List<SelectListItem> Estados { get; set; } = new();

	public bool Admin { get; set; }
}