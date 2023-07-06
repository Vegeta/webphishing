using AutoMapper;
using Dapper;
using Domain;
using Domain.Entidades;
using Domain.Transferencia;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Webapp.Controllers;
using Webapp.Models;

namespace Webapp.Areas.Manage.Controllers;

public class ExamenesController : BaseAdminController {
	private readonly AppDbContext _db;
	private readonly IMapper _mapper;
	private readonly ExamenService _service;

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Exámenes", "/Manage/Examenes");
		CurrentMenu("Contenido/Examenes");
		Titulo("Plantillas de Exámenes");
	}

	public ExamenesController(AppDbContext db, IMapper mapper, ExamenService service) {
		_db = db;
		_mapper = mapper;
		_service = service;
	}

	public IActionResult Index() {
		return View();
	}

	[HttpPost]
	public IActionResult Lista() {
		// usando dapper extensions
		var lista = _db.Database.GetDbConnection()
			.Query<dynamic>("select * from examen e order by e.titulo")
			.AsList();

		lista.ForEach(x => {
			x.lblActivo = x.activo ? "SI" : "NO";
			x.creacion = x.creacion.ToString("yyyy-MM-dd");
			x.modificacion = x.modificacion.ToString("yyyy-MM-dd");
		});
		return Ok(lista);
	}

	public IActionResult Crear() {
		Breadcrumbs.Active("Crear");
		var model = new ExamenModelWeb {
			Tipos = OpcionesConfig.TipoExamenCombo(),
			Tipo = TipoExamen.Personalizado
		};
		ViewBag.modelo = ToJson(model);
		ViewBag.banner = "Crear nuevo Examen";
		return View("Edit", model);
	}

	public IActionResult Preguntas() {
		var preguntas = _db.Pregunta
			.OrderBy(x => x.Nombre)
			.Select(x => new {
				id = x.Id,
				nombre = x.Nombre,
				dificultad = x.Dificultad,
				legitimo = x.Legitimo == 0 ? "NO" : "SI",
			});
		return Ok(preguntas);
	}

	public IActionResult Edit(int id) {
		Breadcrumbs.Active("Editar");
		var p = _db.Examen
			.AsNoTracking().Include(x => x.ExamenPregunta)
			.ThenInclude(y => y.Pregunta)
			.First(x => x.Id == id);
		var model = _mapper.Map<ExamenModelWeb>(p);
		model.Preguntas = p.ExamenPregunta.OrderBy(x => x.Orden)
			.Select(x => new PreguntaExModel {
				Id = x.Id,
				PreguntaId = x.PreguntaId,
				Nombre = x.Pregunta.Nombre,
				Dificultad = x.Pregunta.Dificultad,
				Legitimo = x.Pregunta.Legitimo == 0 ? "NO" : "SI",
				Orden = x.Orden ?? 0,
			}).ToList();
		model.Tipos = OpcionesConfig.TipoExamenCombo();

		ViewBag.modelo = ToJson(model);
		ViewBag.banner = "Editar Examen";
		return View(model);
	}

	public IActionResult Guardar([FromBody] ExamenModelWeb model) {
		var res = _service.GuardarExamen(model, model.Deleted);
		ConfirmaWeb("Datos actualizados");
		return Ok(new { error = res.Error, id = res.Data?.Id });
	}
	
}