using Infraestructura.Persistencia;
using Domain.Entidades;
using Infraestructura.Filtros;
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
		user.FechaPassword = DateTime.Now;
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

	public IQueryable<Usuario> ListaUsuarios(FiltrosUsuario filtros) {
		IQueryable<Usuario> q = _db.Usuario;

		if (!string.IsNullOrEmpty(filtros.Email))
			q = q.Where(x => x.Email.ToUpper().Contains(filtros.Email.ToUpper()));
		if (!string.IsNullOrEmpty(filtros.Nombre))
			q = q.Where(x => x.Nombres.ToUpper().Contains(filtros.Nombre.ToUpper()));

		var ordenador = new SortExpressionHelper<Usuario>()
			.Add("email", x => x.Email)
			.Add("nombres", x => x.Apellidos, x => x.Nombres)
			.Add("creacion", x => x.Creacion)
			.Add("modificacion", x => x.Modificacion);

		var order = filtros.OrdenCampo;
		if (!string.IsNullOrEmpty(filtros.OrdenCampo)) {
			q = ordenador.SetOrder(q, filtros.OrdenCampo, filtros.OrdenDir);
		}

		return q;
	}

	public IQueryable<Perfil> ListaPerfiles(FiltrosPerfiles filtros) {
		IQueryable<Perfil> q = _db.Perfil;

		if (!string.IsNullOrEmpty(filtros.Nombre))
			q = q.Where(x => x.Nombre.ToUpper().Contains(filtros.Nombre.ToUpper()));

		if (!string.IsNullOrEmpty(filtros.Identificador))
			q = q.Where(x => x.Identificador.ToUpper().Contains(filtros.Identificador.ToUpper()));

		var ordenador = new SortExpressionHelper<Perfil>()
			.Add("nombre", x => x.Nombre)
			.Add("identificador", x => x.Identificador);

		var order = filtros.OrdenCampo;
		if (!string.IsNullOrEmpty(filtros.OrdenCampo)) {
			q = ordenador.SetOrder(q, filtros.OrdenCampo, filtros.OrdenDir);
		}

		return q;
	}
}