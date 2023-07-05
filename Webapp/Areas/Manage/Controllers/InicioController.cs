using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Webapp.Controllers;

namespace Webapp.Areas.Manage.Controllers {
	public class InicioController : BaseAdminController {
		public InicioController() {
		}

		public override void OnActionExecuting(ActionExecutingContext context) {
			base.OnActionExecuting(context);
			CurrentMenu("Inicio");
		}

		public IActionResult Index() {
			return View();
		}
	}
}