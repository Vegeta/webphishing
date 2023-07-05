using Microsoft.AspNetCore.Mvc;

namespace Webapp.Controllers;

public class RegistroController : BaseController {
	private readonly ILogger<HomeController> _logger;

	public RegistroController(ILogger<HomeController> logger) {
		_logger = logger;
	}

	public IActionResult Index() {
		return View();
	}
}
