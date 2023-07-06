using AutoMapper;
using Infraestructura.Persistencia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Webapp.Controllers;
using Webapp.Models;

namespace Webapp.Areas.Manage.Controllers;

class CuestionarioController : BaseAdminController {

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
		return View();
	}

	public IActionResult Guardar([FromBody] ExamenModelWeb model) {
		//var res = _service.GuardarExamen(model, model.Deleted);
		//ConfirmaWeb("Datos actualizados");
		//return Ok(new { error = res.Error, id = res.Data?.Id });
	}

}