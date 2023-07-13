using Infraestructura;
using Infraestructura.Identity;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Webapp.Web;

namespace Webapp.Controllers;

public class IngresoController : BaseController {
	private readonly ILogger<HomeController> _logger;
	private readonly AppDbContext _db;
	private readonly UsuariosService _usuarios;

	public IngresoController(ILogger<HomeController> logger, AppDbContext db, UsuariosService usuarios) {
		_logger = logger;
		_db = db;
		_usuarios = usuarios;
	}

	public IActionResult Index() {
		var url = HttpContext.Session.GetString("returnUrl");
		if (ValidRetUrl(url))
			ViewBag.returnUrl = url ?? "";
		return View();
	}

	public IActionResult Login([FromBody] LoginModel model) {
		var login = _usuarios.Login(model.Username, model.Password);
		if (login.HasError)
			return Ok(new LoginWeb { Error = login.Error });

		if (login.Usuario == null)
			return Ok(new LoginWeb { Error = "Error recuperando datos" });

		var ses = new SessionInfo {
			Id = login.Usuario.Id,
			Username = login.Usuario.Username,
			Nombres = login.Usuario.Nombres,
			Apellidos = login.Usuario.Apellidos,
			Tipo = login.Usuario.Tipo ?? "normal",
			Email = login.Usuario.Email,
			Permisos = login.Permisos
		};

		HttpContext.Session.Remove("returnUrl");
		HttpContext.Session.SetString("user", JSON.Stringify(ses));

		var res = new LoginWeb();

		if (ValidRetUrl(model.Url))
			res.Url = model.Url;
		else {
			res.Url = login.Manager ? Url.Content("~/Manage") : Url.Content("~/Cuenta");
		}

		return Ok(res);
	}

	public bool ValidRetUrl(string? url) {
		return !string.IsNullOrEmpty(url) && Url.IsLocalUrl(url);
	}

}

public class LoginWeb {
	public string? Error { get; set; }
	public string? Url { get; set; } = "";
}

public class LoginModel {
	public string Username { get; set; } = "";
	public string Password { get; set; } = "";
	public string? Url { get; set; } = "";
}