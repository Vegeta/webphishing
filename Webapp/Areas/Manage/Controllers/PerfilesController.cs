using AutoMapper;
using Domain.Entidades;
using Infraestructura.Persistencia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Webapp.Controllers;
using Webapp.Models;
using Webapp.Models.Consultas;
using Webapp.Models.Datatables;
using Webapp.Models.Formularios;
using Webapp.Web;

namespace Webapp.Areas.Manage.Controllers;

public class PerfilesController : BaseAdminController {

	ConsultasAuth _consultas;
	AppDbContext _db;
	private readonly IMapper _mapper;

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Perfiles", "/Manage/Perfiles");
		CurrentMenu("Administración/Perfiles");
		Titulo("Perfiles de usuario");
	}


	public PerfilesController(AppDbContext db, IMapper mapper, ConsultasAuth consultas) {
		_db = db;
		_mapper = mapper;
		_consultas = consultas;
	}

	public IActionResult Index() {
		return View();
	}

	[HttpPost]
	public IActionResult Lista(DatatablesModel model) {
		var filtros = model.GetFiltros(new FiltrosPerfiles());

		var orden = model.FirstOrder();
		filtros.OrdenCampo = orden.Campo;
		filtros.OrdenDir = orden.Dir;

		var q = _consultas.ListaPerfiles(filtros);

		var proj = q.Select(x => new {
			id = x.Id,
			email = x.Nombre,
			identificador = x.Identificador,
		});

		var paged = proj.GetPaged(model.Page, model.Length);
		var res = ResultPagedHelpers.FromPaged(paged, model.Draw);
		return Ok(res);
	}

	public IActionResult Crear() {
		Breadcrumbs.Active("Crear");
		var model = new PerfilModelWeb();
		var opciones = model.ObtenerOpciones(new Perfil());
		ViewBag.opciones = ToJson(opciones);
		ViewBag.modelo = ToJson(model);
		ViewBag.banner = "Crear Usuario";
		return View("Edit", model);
	}

	public IActionResult Edit(int id) {
		Breadcrumbs.Active("Editar");
		var p = _db.Perfil.Find(id);
		var model = _mapper.Map<PerfilModelWeb>(p);
		var opciones = model.ObtenerOpciones(p);
		ViewBag.opciones = ToJson(opciones);
		ViewBag.modelo = ToJson(model);
		ViewBag.banner = "Editar Perfil";
		return View(model);
	}

	[HttpPost]
	public async Task<IActionResult> Delete(int id) {
		// TODO checks
		await _db.Perfil.Where(x => x.Id == id).ExecuteDeleteAsync();
		ConfirmaWeb("Perfil Eliminado");
		return Ok();
	}

	public IActionResult Guardar([FromBody] PerfilModelWeb modelWeb) {
		var id = modelWeb.Id;
		var perfil = id.HasValue ? _db.Perfil.Find(id.Value) : new Perfil();

		if (perfil == null)
			return Problem("No encontrado");

		_mapper.Map(modelWeb, perfil);
		perfil.Permisos = JSON.Stringify(modelWeb.Permisos);

		if (!id.HasValue) {
			_db.Perfil.Add(perfil);
		}

		_db.SaveChanges();
		ConfirmaWeb("Datos actualizados");
		return Ok(new { id = perfil.Id });
	}

	[HttpPost]
	public async Task<IActionResult> Usuarios(int id) {
		var lista = await _db.Usuario
			.Where(x => x.PerfilId == id)
			.Select(x => new {
				id = x.Id,
				label = $"{x.Apellidos} {x.Nombres} ({x.Username})",
				email = x.Email
			})
			.ToListAsync();
		return Ok(lista);
	}

}

