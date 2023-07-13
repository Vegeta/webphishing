using Infraestructura.Persistencia;
using Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Servicios;

public class UsuariosService {

	private readonly AppDbContext _db;

	public UsuariosService(AppDbContext db) {
		_db = db;
	}

	public LoginResult Login(string username, string password) {
		if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			return LoginResult.ConError("Datos inválidos"); // no encontrado

		var check = username.ToLower();

		var user = _db.Usuario
			.Include(x => x.Perfil)
			.FirstOrDefault(x => x.Email.ToLower() == check);
		// si no hay por email, pruebe por username
		if (user == null) {
			user = _db.Usuario
				.Include(x => x.Perfil)
				.FirstOrDefault(x => x.Username.ToLower() == check);
		}

		if (user == null)
			return LoginResult.ConError("Acceso inválido"); // no encontrado

		if (!user.EstaActivo)
			return LoginResult.ConError("Usuario inactivo");

		var vale = VerifyPassword(user.Password, password);
		if (!vale)
			return LoginResult.ConError("Acceso inválido"); // password incorrecto

		//TODO auditar login
		var permisos = JSON.Parse<List<string>>(user.Perfil?.Permisos ?? "[]");
		var res = new LoginResult {
			Usuario = user,
			Permisos = permisos,
			Manager = user.Tipo == "manager"
		};

		if (user.Username.ToLower() == "admin") {
			res.Permisos.Add("admin");
			res.Manager = true;
		}

		return res;
	}

	public bool VerifyPassword(string hash, string password) {
		var hasher = new PasswordHash();
		return hasher.Verify(hash, password);
	}

	public static void UpdatePassword(Usuario user, string password) {
		var hasher = new PasswordHash();
		user.Password = hasher.Hash(password);
	}

	public static bool AutorizarPermisos(IList<string> permisos, string args) {
		if (string.IsNullOrEmpty(args)) return true;
		if (permisos.Count == 0) return false;
		if (permisos.Contains("admin"))
			return true;

		// partir primero con los 'and' (,) y dentro verificar los 'or'(|)
		var reqs = args.Split(",");
		foreach (var req in reqs) {
			var ops = req.Split("|");
			var existe = false;
			foreach (var op in ops) {
				if (permisos.Contains(op.Trim()))
					existe = true;
			}
			if (!existe)
				return false;
		}

		return true;
	}

}

public class LoginResult {
	public string? Error { get; set; }
	public Usuario? Usuario { get; set; }
	public bool Manager { get; set; } = false;
	public IList<string> Permisos { get; set; } = new List<string>();

	public bool HasError => !string.IsNullOrEmpty(Error);

	public static LoginResult ConError(string error) {
		return new LoginResult { Error = error };
	}
}