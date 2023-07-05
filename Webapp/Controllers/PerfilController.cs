using Microsoft.AspNetCore.Mvc;

namespace Webapp.Controllers;

public class PerfilController : BaseController {
	private readonly ILogger<HomeController> _logger;

	public PerfilController(ILogger<HomeController> logger) {
		_logger = logger;
	}

	public IActionResult Index() {
		return View();
	}
}