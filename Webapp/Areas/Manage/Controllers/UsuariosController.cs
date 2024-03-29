using AutoMapper;
using Domain;
using Domain.Entidades;
using Infraestructura.Filtros;
using Infraestructura.Logging;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Webapp.Controllers;
using Webapp.Models;
using Webapp.Models.Datatables;

namespace Webapp.Areas.Manage.Controllers;

public class UsuariosController : BaseAdminController {
	private readonly AppDbContext _db;
	private readonly IMapper _mapper;
	private readonly UsuariosService _usuarios;
	private readonly IAuditor<UsuariosController> _logger;

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Usuarios", "/Manage/Usuarios");
		CurrentMenu("Administración/Usuarios");
		Titulo("Usuarios registrados");
	}

	public UsuariosController(AppDbContext db, IMapper mapper, UsuariosService usuarios, IAuditor<UsuariosController> logger) {
		_db = db;
		_mapper = mapper;
		_usuarios = usuarios;
		_logger = logger;
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

		var q = _usuarios.ListaUsuarios(filtros);

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
			Perfiles = ComboPerfiles(),
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

		model.Perfiles = ComboPerfiles();
		model.Tipos = OpcionesConfig.ComboDict(TipoUsuario.Mapa());
		ViewBag.modelo = ToJson(model);
		ViewBag.banner = "Editar Usuario";
		return View(model);
	}

	[HttpPost]
	public async Task<IActionResult> Delete(int id) {
		// TODO checks
		await _db.Usuario.Where(x => x.Id == id).ExecuteDeleteAsync();
		_logger.Info("Usuario eliminado: " + id, new { id });
		ConfirmaWeb("Usuario Eliminado");
		return Ok();
	}

	public IActionResult Guardar([FromBody] UsuarioModel model) {
		var id = model.Id;
		var user = id.HasValue ? _db.Usuario.Find(id.Value) : new Usuario();

		if (user == null)
			return Problem("No encontrado");

		_mapper.Map(model, user);

		var ac = "actualizado";
		if (!id.HasValue) {
			UsuariosService.UpdatePassword(user, model.Password);
			user.Creacion = DateTime.Now;
			_db.Usuario.Add(user);
			ac = "creado";
		}
		user.Modificacion = DateTime.Now;

		_db.SaveChanges();
		_logger.Info($"Usuario {ac}: {user.Username}", new { user.Username, user.Id });
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
		_logger.Info("Password cambiado para usuario: " + user.Username);

		return Ok(new { msg = "Password cambiado" });
	}

	protected List<SelectListItem> ComboPerfiles() {
		var lista = _db.Perfil
			.OrderBy(x => x.Nombre)
			.Select(x => new { nombre = x.Nombre, id = x.Id })
			.ToList()
			.Select(x => new SelectListItem(x.nombre, x.id.ToString()))
			.ToList();
		lista.Insert(0, new SelectListItem());
		return lista;
	}
}