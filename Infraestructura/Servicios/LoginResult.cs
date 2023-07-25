using Domain.Entidades;

namespace Infraestructura.Servicios;

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