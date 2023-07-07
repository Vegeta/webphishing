namespace Webapp.Models;

public class RegistroModel {
	public string Nombre { get; set; } = "";
	public string Apellido { get; set; } = "";
	public string Ocupacion { get; set; } = "";
	public string Genero { get; set; } = "";
	public short? Edad { get; set; }
	public short? ExperienciaSeguridad { get; set; }
	public string Actividad { get; set; } = "";
	public string? Email { get; set; }
}