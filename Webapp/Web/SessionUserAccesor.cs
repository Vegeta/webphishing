using Infraestructura;
using Infraestructura.Identity;
using Infraestructura.Servicios;

namespace Webapp.Web;

public class SessionUserAccesor : IUserAccesor {
	private readonly IHttpContextAccessor _accesor;

	public SessionUserAccesor(IHttpContextAccessor accesor) {
		this._accesor = accesor;
	}

	public SessionInfo? GetSessionInfo() {
		var txt = _accesor.HttpContext?.Session.GetString("user");
		return string.IsNullOrEmpty(txt) ? null : JSON.Parse<SessionInfo>(txt);
	}

	public SessionInfo CurrentUser() {
		return GetSessionInfo() ?? new SessionInfo();
	}

	public string? CurrentUsername() {
		var info = GetSessionInfo();
		return info?.Username;
	}

	public int UserId() {
		var info = GetSessionInfo();
		return info?.Id ?? 0;
	}

	public string? IpAddress() {
		return _accesor.HttpContext?.Connection.RemoteIpAddress?.ToString();
	}
}