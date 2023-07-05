using Microsoft.AspNetCore.Mvc;

namespace Webapp.Controllers;

public class IngresoController : BaseController {
	private readonly ILogger<HomeController> _logger;

	public IngresoController(ILogger<HomeController> logger) {
		_logger = logger;
	}

	public IActionResult Index() {
		return View();
	}
}