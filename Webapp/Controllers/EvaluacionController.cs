using AutoMapper;
using Domain;
using Domain.Entidades;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Webapp.Models;

namespace Webapp.Controllers;

public class EvaluacionController : BaseController {
	private readonly ILogger<HomeController> _logger;
	private readonly AppDbContext _db;
	private readonly IMapper _mapper;
	private readonly CatalogoGeneral _cat;
	private readonly RegistroService _registro;

	public EvaluacionController(ILogger<HomeController> logger, AppDbContext db,
		IMapper mapper, CatalogoGeneral cat, RegistroService registro) {
		_logger = logger;
		_db = db;
		_mapper = mapper;
		_cat = cat;
		_registro = registro;
	}

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		Breadcrumbs.Add("Empezar", "/Empezar");
		Titulo("Resultados tests");
	}

	public IActionResult Index() {
		var vm = new InicioModel();

		var vmReg = new RegistroModel();
		var mapaActividad = _cat.Carreras()
			.ToDictionary(keySelector: m => m, elementSelector: m => m);

		vm.Actividades = OpcionesConfig.ComboDict(mapaActividad, "");
		vm.Generos = OpcionesConfig.ComboDict(Generos.Mapa(), "");
		vm.Ocupaciones = OpcionesConfig.ComboDict(Ocupaciones.Mapa(), "");

		vm.Anios.Add(new SelectListItem());
		for (var i = 1; i < 20; i++) {
			vm.Anios.Add(new SelectListItem(i.ToString(), i.ToString()));
		}
		vm.Anios.Add(new SelectListItem("20 o más", "20"));

		ViewBag.registro = ToJson(vmReg);
		return View(vm);
	}

	[HttpPost]
	public IActionResult Registrar([FromBody] RegistroModel model) {
		var per = _mapper.Map(model, new Persona());
		per.Nombre = per.Nombre?.ToUpperInvariant();
		per.Apellido = per.Apellido?.ToUpperInvariant();
		var res = _registro.RegistrarPersona(per);

		if (res.Data == null)
			return Problem("Error creando persona");

		var creacion = _registro.IniciarSesionIndividual(res.Data);
		if (creacion.Sesion == null) {
			return Problem("Error creando sesion");
		}

		HttpContext.Session.SetString("token_examen", creacion.Sesion.Token ?? "");

		var url = Url.Content("~/Evaluacion");
		return Ok(new { url, error = "" });
	}

	public IActionResult Proceso() {


		return View();
	}




}

public class InicioModel {
	public List<SelectListItem> Generos { get; set; } = new();
	public List<SelectListItem> Ocupaciones { get; set; } = new();
	public List<SelectListItem> Actividades { get; set; } = new();
	public List<SelectListItem> Anios { get; set; } = new();
}