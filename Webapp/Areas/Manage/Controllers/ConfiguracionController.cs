using Dapper;
using Infraestructura;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Webapp.Controllers;

namespace Webapp.Areas.Manage.Controllers;

public class ConfiguracionController : BaseAdminController {
	private readonly AppDbContext _db;
	private readonly CatalogoGeneral _cat;

	public ConfiguracionController(AppDbContext db, CatalogoGeneral cat) {
		_db = db;
		_cat = cat;
	}

	public override void OnActionExecuting(ActionExecutingContext context) {
		base.OnActionExecuting(context);
		BreadcrumbsAdmin.Add("Configuración", "/Manage/Configuracion");
		CurrentMenu("Administración/Configuración");
		Titulo("Configuración del sistema");
	}

	public IActionResult Index() {
		Titulo("Configuracion Aplicación");
		ViewBag.carreras = JSON.Stringify(_cat.Carreras());
		ViewBag.general = JSON.Stringify(_cat.ConfigGeneral());
		ViewBag.examenes = JSON.Stringify(Examenes());
		return View();
	}

	public IActionResult Carreras() {
		var lista = _cat.Carreras();
		return Ok(lista);
	}

	public IActionResult ConfigGeneral() {
		var data = _cat.ConfigGeneral();
		return Ok(data);
	}

	public IActionResult Guardar([FromBody] ModeloDatos model) {
		_cat.GuardarParametro(model.Nombre, model.Data);
		return Ok();
	}

	private IList<ExamenNum> Examenes() {
		var sql = @"select e.id, e.titulo, e.activo, e.cuestionario_pos as pos, c.num
		from examen e
		LEFT JOIN (
		select examen_id, count(*) num
		from examen_pregunta
		group by examen_id
		) c on c.examen_id = e.id
		order by e.titulo";

		return _db.Database.GetDbConnection()
			.Query<ExamenNum>(sql)
			.ToList();
	}

	public class ExamenNum {
		public int Id { get; set; }
		public string? Titulo { get; set; }
		public bool? Activo { get; set; }
		public int Num { get; set; }
		public int? Pos { get; set; }

		public string Nombre {
			get {
				var post = Activo ?? false ? "" : " (N/A)";
				return (Titulo ?? "S/N") + post;
			}
		}
	}
}

public class ModeloDatos {
	public string Nombre { get; set; } = "";
	public object Data { get; set; } = "";
}