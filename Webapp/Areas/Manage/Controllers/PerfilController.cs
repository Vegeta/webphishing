using Infraestructura.Logging;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webapp.Controllers;

namespace Webapp.Areas.Manage.Controllers; 

public class PerfilController : BaseAdminController {
	private readonly AppDbContext _db;
	private readonly IUserAccesor _userAccesor;
	private readonly IAuditor<InicioController> _logger;
	private readonly UsuariosService _usuarios;
	
	public PerfilController(AppDbContext db, IUserAccesor userAccesor, IAuditor<InicioController> logger, UsuariosService usuarios) {
		_db = db;
		_userAccesor = userAccesor;
		_logger = logger;
		_usuarios = usuarios;
	}
	
	public IActionResult Index() {
		BreadcrumbsAdmin.Add("Mi Perfil", "/Manage/Perfil");
		Titulo("Mi Cuenta");

		var sesion = _userAccesor.CurrentUser();
		var user = _db.Usuario
			.Include(x => x.Perfil)
			.First(x => x.Id == sesion.Id);
		var model = new {
			user.Username,
			user.Nombres,
			user.Apellidos,
			user.Creacion,
			user.Email,
			user.Celular,
			user.Tipo,
			perfil = user.Perfil?.Nombre,
			user.FechaPassword
		};
		ViewBag.modelo = ToJson(model);
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> CambioPassword(string original, string nuevo) {
		var sesion = _userAccesor.CurrentUser();
		var user = await _db.Usuario.FindAsync(sesion.Id);
		if (user == null) {
			return Problem();
		}

		if (!_usuarios.VerifyPassword(user.Password, original)) {
			return Ok(new { error = "Contraseña original inválida" });
		}

		UsuariosService.UpdatePassword(user, nuevo);
		user.Modificacion = DateTime.Now;
		await _db.SaveChangesAsync();
		_logger.Info("Password cambiado por usuario: " + user.Username);
		ConfirmaWeb("Ha actualizado su contraseña.");
		return Ok(new { msg = "Password cambiado" });
	}
}