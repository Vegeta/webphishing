using Domain.Entidades;
using Infraestructura;
using Infraestructura.Filtros;
using Infraestructura.Persistencia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Webapp.Controllers;
using Webapp.Models;
using Webapp.Models.Datatables;

namespace Webapp.Areas.Manage.Controllers;

public class ContactosController : BaseAdminController {
	private readonly AppDbContext _db;

	public ContactosController(AppDbContext db) {
		_db = db;
	}

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Contactos", "/Manage/Contactos");
		CurrentMenu("Reportes/Contactos");
		Titulo("Mensajes de contacto recibidos");
	}

	public IActionResult Index() {
		var model = new ContactoFiltroVm {
			Meses = OpcionesConfig.MesesWeb(" ")
		};
		return View(model);
	}

	[HttpPost]
	public IActionResult Lista(DatatablesModel model) {
		var filtros = model.GetFiltros(new ContactoFiltroVm());

		var orden = model.FirstOrder();
		filtros.OrdenCampo = orden.Campo;
		filtros.OrdenDir = orden.Dir;

		IQueryable<Contacto> q = _db.Contacto;

		if (!string.IsNullOrEmpty(filtros.Email))
			q = q.Where(x => x.Email!.ToUpper().Contains(filtros.Email.ToUpper()));

		if (filtros.Anio.HasValue)
			q = q.Where(x => x.Fecha.Year == filtros.Anio);
		if (filtros.Mes.HasValue)
			q = q.Where(x => x.Fecha.Month == filtros.Mes);

		q = q.OrderByDescending(x => x.Fecha);
		var lista = q.Select(x => new {
			x.Id,
			x.Nombre,
			x.Email,
			x.Titulo,
			fecha = x.Fecha.ToString("yyyy-MM-dd hh:mm:ss")
		});

		var paged = lista.GetPaged(model.Page, model.Length);
		var res = ResultPagedHelpers.FromPaged(paged, model.Draw);
		return Ok(res);
	}

	public IActionResult Detalle(int id) {
		var log = _db.Contacto.First(x => x.Id == id);
		ViewBag.modelo = JSON.Stringify(log);
		return View();
	}
}

public class ContactoFiltroVm : Ordenable {
	public int? Mes { get; set; }
	public int? Anio { get; set; }
	public string? Email { get; set; }

	public List<SelectListItem> Meses { get; set; } = new();
}