using AutoMapper;
using Domain;
using Domain.Entidades;
using Infraestructura;
using Infraestructura.Persistencia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Webapp.Controllers;
using Webapp.Models;
using Webapp.Web;

namespace Webapp.Areas.Manage.Controllers;

public class CuestionarioController : BaseAdminController {

	private readonly AppDbContext _db;
	private readonly IMapper _mapper;

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Cuestionario", "/Manage/Cuestionario");
		CurrentMenu("Contenido/Cuestionario");
		Titulo("Cuestionario");
	}

	public CuestionarioController(AppDbContext db, IMapper mapper) {
		_db = db;
		_mapper = mapper;
	}

	public IActionResult Index() {
		// por ahora uno solo
		var cuest = _db.Cuestionario.FirstOrDefault() ?? new Cuestionario {
			Creacion = DateTime.Now
		};
		var vm = new CuestionarioModelWeb {
			Id = cuest.Id,
			Titulo = cuest.Titulo,
			Instrucciones = cuest.Instrucciones,
		};
		if (cuest is { Id: > 0, Preguntas: not null }) {
			vm.Preguntas = JSON.Parse<List<CuestPreguntaModel>>(cuest.Preguntas);
		}

		var respuestas = RespuestaCuestionario.Mapa().Values;
		ViewBag.respuestas = string.Join(", ", respuestas);

		ViewBag.modelo = ToJson(vm);
		return View();
	}

	public IActionResult Guardar([FromBody] CuestionarioModelWeb model) {
		var cuest = _db.Cuestionario.FirstOrDefault() ?? new Cuestionario {
			Creacion = DateTime.Now
		};
		cuest.Modificacion = DateTime.Now;
		cuest.Preguntas = JSON.Stringify(model.Preguntas);
		cuest.Titulo = model.Titulo;
		cuest.Instrucciones = model.Instrucciones;
		if (cuest.Id == 0)
			_db.Cuestionario.Add(cuest);
		_db.SaveChanges();
		ConfirmaWeb("Datos actualizados");
		return Ok();
	}

}