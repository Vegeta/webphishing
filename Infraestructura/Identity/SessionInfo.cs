namespace Infraestructura.Identity;

public class SessionInfo {
	public string Username { get; set; } = "";
	public string Nombres { get; set; } = "";
	public string Apellidos { get; set; } = "";
	public string Email { get; set; } = "";
	public string Tipo { get; set; } = "";
	public int Id { get; set; }
	public IList<string> Permisos { get; set; } = new List<string>();
}