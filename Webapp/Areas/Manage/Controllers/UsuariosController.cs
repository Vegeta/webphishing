using AutoMapper;
using Domain;
using Domain.Entidades;
using Infraestructura;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Webapp.Controllers;
using Webapp.Models;
using Webapp.Models.Consultas;
using Webapp.Models.Datatables;
using Webapp.Models.Formularios;

namespace Webapp.Areas.Manage.Controllers;

public class UsuariosController : BaseAdminController {
	AppDbContext _db;
	private readonly IMapper _mapper;
	private readonly ConsultasAuth _consultas;

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Usuarios", "/Manage/Usuarios");
		CurrentMenu("Administración/Usuarios");
		Titulo("Usuarios registrados");
	}

	public UsuariosController(AppDbContext db, IMapper mapper, ConsultasAuth consultas) {
		_db = db;
		_mapper = mapper;
		_consultas = consultas;
	}

	public IActionResult Index() {
		return View();
	}

	[HttpPost]
	public IActionResult Lista(DatatablesModel model) {
		var filtros = model.GetFiltros(new FiltrosUsuario());

		var orden = model.FirstOrder();
		filtros.OrdenCampo = orden.Campo;
		filtros.OrdenDir = orden.Dir;

		var q = _consultas.ListaUsuarios(filtros);

		var proj = q.Select(x => new {
			id = x.Id,
			email = x.Email,
			activo = !x.Activo ?? false ? "NO" : "SI",
			tipo = x.Tipo,
			nombres = $"{x.Apellidos} {x.Nombres}",
			creacion = x.Creacion.ToString("yyyy-MM-dd"),
			modificacion = x.Modificacion.ToString("yyyy-MM-dd")
		});

		var paged = proj.GetPaged(model.Page, model.Length);

		var res = ResultPagedHelpers.FromPaged(paged, model.Draw);
		return Ok(res);
	}

	public IActionResult Crear() {
		Breadcrumbs.Active("Crear");
		var model = new UsuarioModel {
			Perfiles = _consultas.ComboPerfiles(),
			Tipos = OpcionesConfig.ComboDict(TipoUsuario.Mapa()),
			Tipo = TipoUsuario.Normal,
		};
		ViewBag.modelo = ToJson(model);
		ViewBag.banner = "Crear Usuario";
		return View("Edit", model);
	}

	public IActionResult Edit(int id) {
		Breadcrumbs.Active("Editar");
		var p = _db.Usuario.Find(id);
		var model = _mapper.Map<UsuarioModel>(p);

		model.Perfiles = _consultas.ComboPerfiles();
		model.Tipos = OpcionesConfig.ComboDict(TipoUsuario.Mapa());
		ViewBag.modelo = ToJson(model);
		ViewBag.banner = "Editar Usuario";
		return View(model);
	}

	[HttpPost]
	public async Task<IActionResult> Delete(int id) {
		// TODO checks
		await _db.Usuario.Where(x => x.Id == id).ExecuteDeleteAsync();
		ConfirmaWeb("Usuario Eliminado");
		return Ok();
	}

	public IActionResult Guardar([FromBody] UsuarioModel model) {
		var id = model.Id;
		var user = id.HasValue ? _db.Usuario.Find(id.Value) : new Usuario();

		if (user == null)
			return Problem("No encontrado");

		_mapper.Map(model, user);

		if (!id.HasValue) {
			UsuariosService.UpdatePassword(user, model.Password);
			user.Creacion = DateTime.Now;
			_db.Usuario.Add(user);
		}
		user.Modificacion = DateTime.Now;

		_db.SaveChanges();
		ConfirmaWeb("Datos actualizados");
		return Ok(new { id = user.Id });
	}


	[HttpPost]
	public async Task<IActionResult> CambiarPassword(int id, string pass) {
		// TODO checks
		var user = await _db.Usuario.FindAsync(id);
		if (user == null) {
			return Problem();
		}

		UsuariosService.UpdatePassword(user, pass);
		user.Modificacion = DateTime.Now;
		await _db.SaveChangesAsync();

		return Ok(new { msg = "Password cambiado" });
	}



}

