namespace Infraestructura.Examenes;

/// <summary>
/// Define el algoritmo para el comportamiento del examen
/// </summary>
public interface IAsignadorExamen {
	public FlujoExamenDto CrearFlujo(ConfigExamen config);
	public void ResolverPreguntas(ConfigExamen config, FlujoExamenDto flujo);
}