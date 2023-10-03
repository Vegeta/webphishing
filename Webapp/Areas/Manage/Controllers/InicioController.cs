using Infraestructura.Persistencia;
using Infraestructura.Reportes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Webapp.Controllers;

namespace Webapp.Areas.Manage.Controllers {
	public class InicioController : BaseAdminController {
		private readonly AppDbContext _db;

		public InicioController(AppDbContext db) {
			_db = db;
		}

		public override void OnActionExecuting(ActionExecutingContext context) {
			base.OnActionExecuting(context);
			CurrentMenu("Inicio");
		}

		public IActionResult Index() {
			var rep = new EstadisticasPrincipales(_db);
			var est = rep.HomeScreenStats();

			ViewBag.est = est;

			return View();
		}
	}
}