using Domain.Entidades;
using Infraestructura;
using Infraestructura.Filtros;
using Infraestructura.Logging;
using Infraestructura.Persistencia;
using Infraestructura.Reportes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Webapp.Controllers;
using Webapp.Models.Datatables;

namespace Webapp.Areas.Manage.Controllers;

public class AuditoriasController : BaseAdminController {
	private readonly AppDbContext _db;

	public AuditoriasController(AppDbContext db) {
		_db = db;
	}

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Auditorias", "/Manage/Auditorias");
		CurrentMenu("Administración/Auditorias");
		Titulo("Auditorias del sistema");
	}

	public IActionResult Index() {
		var niveles = NivelAuditoria.Niveles()
			.Select(x => new SelectListItem(x, x)).ToList();

		var model = new AuditoriaVm {
			Niveles = niveles
		};
		return View(model);
	}

	[HttpPost]
	public IActionResult Lista(DatatablesModel model) {
		var filtros = model.GetFiltros(new AuditoriaVm());

		var orden = model.FirstOrder();
		filtros.OrdenCampo = orden.Campo;
		filtros.OrdenDir = orden.Dir;

		IQueryable<Auditoria> q = _db.Auditoria;

		if (!string.IsNullOrEmpty(filtros.Nivel))
			q = q.Where(x => x.Nivel == filtros.Nivel);
		if (!string.IsNullOrEmpty(filtros.Mensaje))
			q = q.Where(x => x.Mensaje!.Contains(filtros.Mensaje.ToUpper()));

		if (!string.IsNullOrEmpty(filtros.Modulo))
			q = q.Where(x => x.Modulo!.Contains(filtros.Modulo.ToUpper()));

		q = q.OrderByDescending(x => x.Fecha);
		var lista = q.Select(x => new {
			x.Id,
			x.Nivel,
			x.Modulo,
			x.Mensaje,
			fecha = x.Fecha.ToString("yyyy-MM-dd hh:mm:ss"),
			x.Usuario
		});

		var paged = lista.GetPaged(model.Page, model.Length);
		var res = ResultPagedHelpers.FromPaged(paged, model.Draw);
		return Ok(res);
	}

	public IActionResult Detalle(int id) {
		var log = _db.Auditoria.First(x => x.Id == id);
		ViewBag.contenido = log.Datos ?? "";
		log.Datos = "";
		ViewBag.modelo = JSON.Stringify(log);
		return View();
	}
}

public class AuditoriaVm : Ordenable {
	public string? Nivel { get; set; }
	public string? Mensaje { get; set; }
	public string? Modulo { get; set; }
	public List<SelectListItem> Niveles { get; set; } = new();
}