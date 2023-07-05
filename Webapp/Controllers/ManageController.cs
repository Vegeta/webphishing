using Microsoft.AspNetCore.Mvc;

namespace Webapp.Controllers;

public class ManageController : BaseController {
	public IActionResult Index() {
		return RedirectToAction("Index", "Inicio", new { area = "manage" });
	}
}

