using Domain.Entidades;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;

namespace Infraestructura.Logging;

public class AuditorDb<T> : IAuditor<T> {
	private readonly AppDbContext _db;
	private readonly IUserAccesor _users;

	public AuditorDb(AppDbContext db, IUserAccesor users) {
		_db = db;
		_users = users;
	}

	public string? ModuleName(string? mod) {
		return string.IsNullOrEmpty(mod) ? typeof(T).FullName : mod;
	}

	private void SaveLog(string nivel, string msg, string? mod, object? data = null) {
		var user = _users.CurrentUsername();
		var fixMod = ModuleName(mod);

		var log = new Auditoria {
			Fecha = DateTime.Now,
			Mensaje = msg,
			Modulo = fixMod,
			Nivel = nivel,
			Usuario = user,
			Datos = data == null ? null : JSON.Stringify(data)
		};
		// tx?
		_db.Auditoria.Add(log);
		_db.SaveChanges();
	}

	public void Info(string msg, object? data = null, string? modulo = "") {
		SaveLog(NivelAuditoria.INFO, msg, modulo, data);
	}

	public void Warn(string msg, object? data = null, string? modulo = "") {
		SaveLog(NivelAuditoria.WARNING, msg, modulo, data);
	}

	public void Debug(string msg, object? data = null, string? modulo = "") {
		SaveLog(NivelAuditoria.DEBUG, msg, modulo, data);
	}

	public void Error(string msg, object? data = null, string? modulo = "") {
		SaveLog(NivelAuditoria.ERROR, msg, modulo, data);
	}

	public void Error(string msg, Exception ex, string? modulo = "") {
		var data = new {
			ex.Message,
			ex.StackTrace,
			ex.Data
		};
		SaveLog(NivelAuditoria.ERROR, msg, modulo, data);
	}
}