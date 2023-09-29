using Domain.Entidades;

namespace Infraestructura.Examenes;

/// <summary>
/// Tiene los datos de avance del cuestionario de forma temporal
/// </summary>
public class DataCuestionario {
	public string? Percepcion { get; set; }
	public float? TiempoCuestionario { get; set; }
	public float? ScoreCuestionario { get; set; }
	public string? RespuestaCuestionario { get; set; }
	public List<CuestionarioRespuesta> Respuestas { get; set; } = new();
}