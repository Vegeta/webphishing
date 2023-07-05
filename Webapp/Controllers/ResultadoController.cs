using Microsoft.AspNetCore.Mvc;

namespace Webapp.Controllers;

public class ResultadoController : BaseController {
	private readonly ILogger<HomeController> _logger;

	public ResultadoController(ILogger<HomeController> logger) {
		_logger = logger;
	}

	public IActionResult Index() {
		return View();
	}
}
