using Domain;

namespace Infraestructura.Examenes;

/// <summary>
/// Se utiliza en el manager para crear flujos de examenes y determinar la forma de asignacion
/// </summary>
public class ConfigExamen {
	public int NumPreguntas { get; set; }
	public int IdExamen { get; set; }
	public int? CuestionarioPos { get; set; }
	public bool Aleatorio { get; set; }
	public string Tipo { get; set; } = TipoExamen.Predeterminado;
}