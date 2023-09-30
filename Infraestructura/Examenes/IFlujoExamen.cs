namespace Infraestructura.Examenes;

/// <summary>
/// Representa un algoritmo para el comportamiento del examen
/// </summary>
public interface IFlujoExamen {
	public FlujoExamenDto CrearFlujo();
	public void ResolverPreguntas(FlujoExamenDto flujo);
}