namespace Infraestructura.Servicios;

/// <summary>
/// Version inicial de la configuracion del examen preeterminado (evaluacion rapida)
/// </summary>
public class ConfiguracionEvaluacion {
	public int NumPreguntas { get; set; }
	public int? PosCuestionario { get; set; }
	public int? IdPredeterminado { get; set; } // id del examen predeterminado
}