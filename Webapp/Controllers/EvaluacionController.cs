using AutoMapper;
using Domain;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Webapp.Models;

namespace Webapp.Controllers;

public class EvaluacionController : BaseController {
	private readonly ILogger<HomeController> _logger;
	private readonly AppDbContext _db;
	private readonly IMapper _mapper;
	private readonly CatalogoGeneral _cat;

	public EvaluacionController(ILogger<HomeController> logger, AppDbContext db, IMapper mapper, CatalogoGeneral cat) {
		_logger = logger;
		_db = db;
		_mapper = mapper;
		_cat = cat;
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
		vm.Anios.Add(new SelectListItem("+20", "20"));

		ViewBag.registro = ToJson(vmReg);
		return View(vm);
	}
}

public class InicioModel {
	public List<SelectListItem> Generos { get; set; } = new();
	public List<SelectListItem> Ocupaciones { get; set; } = new();
	public List<SelectListItem> Actividades { get; set; } = new();
	public List<SelectListItem> Anios { get; set; } = new();
}

public class RegistroModel {
	public string Nombre { get; set; } = "";
	public string Apellido { get; set; } = "";
	public string Ocupacion { get; set; } = "";
	public string Genero { get; set; } = "";
	public short? Edad { get; set; }
	public short? ExperienciaSeguridad { get; set; }
	public string Actividad { get; set; } = "";
	public string? Email { get; set; }
}
