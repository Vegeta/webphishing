using Domain;
using Infraestructura;
using Infraestructura.Filtros;
using Infraestructura.Persistencia;
using Infraestructura.Reportes;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Webapp.Controllers;
using Webapp.Models;
using Webapp.Models.Datatables;

namespace Webapp.Areas.Manage.Controllers;

public class PersonasController : BaseAdminController {
	private readonly AppDbContext _db;
	private readonly ConsultasService _consultas;


	public PersonasController(AppDbContext db, ConsultasService consultas) {
		_db = db;
		_consultas = consultas;
	}

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Personas", "/Manage/Personas");
		CurrentMenu("Reportes/Personas");
		Titulo("Personas registradas");
	}

	public IActionResult Index() {
		var vm = new FiltroCatalogoVm();

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

		var q = _consultas.Personas(filtros);

		var proj = q.Select(x => new {
			x.Id,
			x.Apellido,
			x.Nombre,
			nombreCompleto = $"{x.Apellido} {x.Nombre}",
			x.Genero,
			x.Ocupacion,
			x.Actividad,
			x.Edad,
			x.Email,
			experiencia = x.ExperienciaSeguridad,
			creacion = x.Creacion!.Value.ToString("yyyy-MM-dd")
		});

		var paged = proj.GetPaged(model.Page, model.Length);

		var res = ResultPagedHelpers.FromPaged(paged, model.Draw);
		return Ok(res);
	}

	public IActionResult Detalle(int id) {
		var p = _db.Persona.First(x => x.Id == id);
		var detalle = new List<Tuple<string, object>> {
			new("Apellidos", p.Apellido!),
			new("Nombres", p.Nombre!),
			new("Genero", p.Genero!),
			new("Edad", p.Edad!),
			new("Ocupacion", p.Ocupacion!),
			new("Actividad", p.Actividad!),
			new("Experiencia Seguridad", p.ExperienciaSeguridad!),
			new("Fecha creación", p.Creacion!.Value.ToString("yyyy-MM-dd hh:mm:ss")),
			new("Fecha modificación", p.Modificacion!.Value.ToString("yyyy-MM-dd hh:mm:ss"))
		};

		var actividad = _consultas.ActividadPersona(id);
		
		ViewBag.modelo = JSON.Stringify(new {
			detalle,
			actividad,
		});

		return View();
	}

	[HttpPost]
	public IActionResult Exportar(string filtros) {
		var filObj = JSON.Parse<FiltroEvaluacion>(filtros);

		var query = _consultas.Personas(filObj);
		var exportador = new ExportarPersonas();
		using var wb = exportador.Exportar(query);
		using var stream = new MemoryStream();
		wb.SaveAs(stream);
		var content = stream.ToArray();
		return File(
			content,
			ExcelUtils.TipoMime,
			"personas.xlsx"
		);
	}
}