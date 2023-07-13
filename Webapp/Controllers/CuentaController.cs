using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Webapp.Controllers;

public class CuentaController : BaseController {
	private readonly ILogger<HomeController> _logger;
	private readonly AppDbContext _db;
	private readonly UsuariosService _usuarios;

	public CuentaController(ILogger<HomeController> logger, AppDbContext db, UsuariosService usuarios) {
		_logger = logger;
		_db = db;
		_usuarios = usuarios;
	}

	public IActionResult Index(string? Url) {
		return View();
	}


}

