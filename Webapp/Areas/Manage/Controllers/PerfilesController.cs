﻿using AutoMapper;
using Domain.Entidades;
using Infraestructura;
using Infraestructura.Filtros;
using Infraestructura.Logging;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Webapp.Controllers;
using Webapp.Models;
using Webapp.Models.Datatables;

namespace Webapp.Areas.Manage.Controllers;

public class PerfilesController : BaseAdminController {
	UsuariosService _usuarios;
	AppDbContext _db;
	private readonly IMapper _mapper;
	private readonly IAuditor<PerfilesController> _logger;

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Perfiles", "/Manage/Perfiles");
		CurrentMenu("Administración/Perfiles");
		Titulo("Perfiles de usuario");
	}


	public PerfilesController(AppDbContext db, IMapper mapper, UsuariosService usuarios, IAuditor<PerfilesController> logger) {
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
		var filtros = model.GetFiltros(new FiltrosPerfiles());

		var orden = model.FirstOrder();
		filtros.OrdenCampo = orden.Campo;
		filtros.OrdenDir = orden.Dir;

		var q = _usuarios.ListaPerfiles(filtros);

		var proj = q.Select(x => new {
			id = x.Id,
			nombre = x.Nombre,
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
		var opciones = model.ObtenerOpciones(p ?? new Perfil());
		ViewBag.opciones = ToJson(opciones);
		ViewBag.modelo = ToJson(model);
		ViewBag.banner = "Editar Perfil";
		return View(model);
	}

	[HttpPost]
	public async Task<IActionResult> Delete(int id) {
		// TODO checks
		await _db.Perfil.Where(x => x.Id == id).ExecuteDeleteAsync();
		_logger.Info("Perfil eliminado", new { id });
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

		var ac = "actualizado";
		if (!id.HasValue) {
			_db.Perfil.Add(perfil);
			ac = "creado";
		}

		_db.SaveChanges();
		_logger.Info($"Perfil {perfil.Nombre} {ac}");

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