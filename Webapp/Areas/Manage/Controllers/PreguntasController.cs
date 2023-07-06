using AutoMapper;
using Domain.Entidades;
using Domain.Transferencia;
using Infraestructura.Persistencia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Webapp.Controllers;
using Webapp.Models;
using Webapp.Web;

namespace Webapp.Areas.Manage.Controllers;

public class PreguntasController : BaseAdminController {
	AppDbContext _db;
	IImagenesEjercicios _imgService;

	private readonly IMapper _mapper;

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Preguntas", "/Manage/Preguntas");
		CurrentMenu("Contenido/Preguntas");
		Titulo("Lista de Preguntas");
	}

	public PreguntasController(AppDbContext db, IMapper mapper, IImagenesEjercicios imgService) {
		_db = db;
		_mapper = mapper;
		_imgService = imgService;
	}

	public IActionResult Index() {
		return View();
	}

	public async Task<IActionResult> Lista() {
		var lista = await _db.Pregunta
			.AsNoTracking()
			.OrderBy(x => x.Nombre)
			.Select(x => new {
				id = x.Id,
				nombre = x.Nombre,
				subject = x.Subject,
				legit = x.Legitimo == 0 ? "NO" : "SI",
				dif = x.Dificultad,
			}).ToListAsync();
		return Ok(lista);
	}

	public IActionResult Crear() {
		Breadcrumbs.Active("Crear");
		var model = new PreguntaModelWeb {
			Dificultades = PreguntaConfig.DificultadesCombo()
		};
		ViewBag.modelo = ToJson(model);
		ViewBag.banner = "Crear nueva Pregunta";
		return View("Edit", model);
	}

	public IActionResult Edit(int id) {
		Breadcrumbs.Active("Editar");
		var p = _db.Pregunta.Find(id);
		var model = _mapper.Map<PreguntaModelWeb>(p);
		if (!string.IsNullOrEmpty(p?.Adjuntos)) {
			model.ListaAdjuntos = FromJson<List<AdjuntoView>>(p.Adjuntos);
		}
		model.Dificultades = PreguntaConfig.DificultadesCombo();
		ViewBag.modelo = ToJson(model);
		ViewBag.banner = "Editar Pregunta";
		return View(model);
	}

	public IActionResult Guardar([FromBody] PreguntaModelWeb model) {
		var id = model.Id;
		var p = id.HasValue ? _db.Pregunta.Find(id.Value) : new Pregunta();

		if (p == null)
			return Problem("No encontrado");

		_mapper.Map(model, p);
		p.Adjuntos = ToJson(model.ListaAdjuntos);

		if (!id.HasValue)
			_db.Pregunta.Add(p);

		_db.SaveChanges();
		ConfirmaWeb("Datos actualizados");
		return Ok(new { id = p.Id });
	}

	[HttpPost]
	public async Task<IActionResult> Eliminar(int id) {
		// TODO checks
		await _db.Pregunta.Where(x => x.Id == id).ExecuteDeleteAsync();
		ConfirmaWeb("Pregunta Eliminada");
		return Ok();
	}

	// edicion del html

	public IActionResult EditHtml(int id) {
		Breadcrumbs
			.Add("Editar Pregunta", $"~/Manage/Preguntas/Edit/{id}")
			.Active("Editar HTML");
		var p = _db.Pregunta
			.AsNoTracking()
			.Single(x => x.Id == id);

		var model = new PreguntaContentModel {
			Id = id,
			Html = p.Html,
			Nombre = p.Nombre
		};
		ViewBag.modelo = JSON.Stringify(model);
		ViewBag.imagenes = ToJson(_imgService.GetFiles());
		return View(model);
	}

	[HttpPost]
	public IActionResult SaveHtml(int id, string html) {
		_db.Pregunta.Where(x => x.Id == id)
			.ExecuteUpdate(s =>
				s.SetProperty(b => b.Html, html)
			);
		return Ok();
	}

	[HttpGet]
	public IList<string> Imagenes() {
		return _imgService.GetFiles();
	}

	public IActionResult UploadImage(UploadModel model) {
		if(model.File != null)
			_imgService.SaveUpload(model.File);
		return Ok("OK");
	}
}