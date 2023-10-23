using System.Diagnostics;
using System.Globalization;
using Domain.Entidades;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Webapp.Models;
using Webapp.Web;

namespace Webapp.Controllers;

[AllowAnonymous]
public class HomeController : BaseController {
	private readonly IUserAccesor _users;
	private readonly AppDbContext _db;

	public HomeController(AppDbContext db, IUserAccesor users) {
		_db = db;
		_users = users;
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

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Mensaje([FromBody] MensajeModel model) {
		if (!CheckMessageLimit()) {
			return Ok(new { error = "Por favor espere unos momentos antes de enviar otro mensaje" });
		}

		var msg = new Contacto {
			Email = model.Email,
			Titulo = model.Titulo,
			Nombre = model.Nombre,
			Mensaje = model.Mensaje,
			Fecha = DateTime.Now,
			IpAddress = _users.IpAddress(),
		};

		_db.Contacto.Add(msg);
		_db.SaveChanges();
		return Ok(new { msg = "OK" });
	}

	// simple rate limiter por tiempo
	private bool CheckMessageLimit() {
		const string key = "__lastComment";
		var ts = HttpContext.Session.GetString(key);
		if (string.IsNullOrEmpty(ts)) {
			HttpContext.Session.SetString(key, DateTime.Now.ToString(CultureInfo.InvariantCulture));
			return true;
		}
		if (!DateTime.TryParse(ts, out var dt))
			return false;
		var diff = DateTime.Now - dt;
		if (diff.Minutes < 30)
			return false;
		return true;
	}
}

public class MensajeModel {
	public string? Nombre { get; set; }
	public string? Email { get; set; }
	public string? Titulo { get; set; }
	public string? Mensaje { get; set; }
}