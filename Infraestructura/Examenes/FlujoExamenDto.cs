namespace Infraestructura.Examenes;

public class FlujoExamenDto {
	public int IndicePaso { get; set; }
	public int Score { get; set; } = 0;
	public bool Aleatorio { get; set; }
	public string Error { get; set; } = "";
	public int NumPreguntas { get; set; }
	public int? CuestionarioPos { get; set; }
	public DateTime? Inicio { get; set; }
	public DateTime? Fin { get; set; }
	public int MaxScore { get; set; }
	public string Tipo { get; set; }
	public List<PasoExamen> Pasos { get; set; } = new();

	public PasoExamen PasoActual() {
		return IndicePaso > Pasos.Count ? Pasos.Last() : Pasos[IndicePaso];
	}

	public bool EsFin {
		get {
			var offset = CuestionarioPos.HasValue ? 1 : 0;
			return Pasos.Count(x => x.Ejecutado) == NumPreguntas + offset;
		}
	}
}