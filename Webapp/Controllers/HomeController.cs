using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Webapp.Models;
using Webapp.Web;

namespace Webapp.Controllers;

[AllowAnonymous]
public class HomeController : BaseController {
	private readonly ILogger<HomeController> _logger;

	public HomeController(ILogger<HomeController> logger) {
		_logger = logger;
	}

	public IActionResult Index() {
		ViewBag.claseHeader = "";
		return View();
	}

	public IActionResult Privacy() {
		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error() {
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}

	[Route("Logout")]
	public IActionResult Logout() {
		HttpContext.Session.Clear();
		Response.Cookies.Delete(AuthConstants.SessionName);
		return RedirectToAction("Index");
	}

}
