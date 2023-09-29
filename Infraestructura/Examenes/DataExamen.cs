namespace Infraestructura.Examenes;

/// <summary>
/// Contiene las estadisticas del examen y datos generales de avance
/// </summary>
public class DataExamen {
	public float? AvgScore { get; set; }
	public float? AvgTiempo { get; set; }
	public float? TiempoTotal { get; set; }
	public int Exito { get; set; } = 0;
	public string Estado { get; set; } = "";
	public int MaxScore { get; set; }

}