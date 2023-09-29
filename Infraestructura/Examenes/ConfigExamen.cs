using Domain;

namespace Infraestructura.Examenes;

public class ConfigExamen {
	public int NumPreguntas { get; set; }
	public int IdExamen { get; set; }
	public int? CuestionarioPos { get; set; }
	public bool Aleatorio { get; set; }
	public string Tipo { get; set; } = TipoExamen.Predeterminado;
}