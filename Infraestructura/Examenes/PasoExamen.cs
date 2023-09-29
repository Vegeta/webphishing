namespace Infraestructura.Examenes;

/// <summary>
/// Representa cada paso de un examen configurado y la data que ingresa el usuario
/// </summary>
public class PasoExamen {
	public string Accion { get; set; } = "";
	public int EntidadId { get; set; }
	public int Score { get; set; }
	public float Tiempo { get; set; }
	public string Respuesta { get; set; } = "";
	public string Real { get; set; } = "";
	public string Dificultad { get; set; } = "";
	public bool Ejecutado { get; set; }
}