using Microsoft.AspNetCore.Mvc;

namespace Webapp.Controllers;

public class ExamenController : BaseController {
	private readonly ILogger<HomeController> _logger;

	public ExamenController(ILogger<HomeController> logger) {
		_logger = logger;
	}

	public IActionResult Index() {
		return View();
	}
}
