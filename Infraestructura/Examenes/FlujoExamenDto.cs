namespace Infraestructura.Examenes;

/// <summary>
/// Contiene la informacion de un examen en curso
/// </summary>
public class FlujoExamenDto {
	public int ExamenId { get; set; }
	public int IndicePaso { get; set; }
	public int Score { get; set; } = 0;
	public bool Aleatorio { get; set; }
	public string Error { get; set; } = "";
	public int NumPreguntas { get; set; }
	public int? CuestionarioPos { get; set; }
	public DateTime? Inicio { get; set; }
	public DateTime? Fin { get; set; }
	public int MaxScore { get; set; }
	public string Tipo { get; set; } = "";
	public List<PasoExamen> Pasos { get; set; } = new();

	public DataCuestionario? DatosCuestionario { get; set; }

	public string? ExtraData { get; set; }

	public PasoExamen PasoActual() {
		return IndicePaso >= Pasos.Count ? Pasos.Last() : Pasos[IndicePaso];
	}

	public bool EsFin {
		get {
			var offset = CuestionarioPos.HasValue ? 1 : 0;
			return Pasos.Count(x => x.Ejecutado) == NumPreguntas + offset;
		}
	}

	public int Respondidas {
		get { return Pasos.Count(x => x is { Ejecutado: true, Accion: "pregunta" }); }
	}

	/// <summary>
	/// Esto calcula el score desde la ultima pregunta contestada X elementos atras
	/// Para cuando el examen tiene mas de 10 elementos (algoritmo)
	/// </summary>
	/// <param name="tomar"></param>
	/// <returns></returns>
	public int ScoreOffset(int tomar) {
		return Pasos.Where(x => x is { Ejecutado: true, Accion: "pregunta" })
			.Reverse()
			.Take(tomar)
			.Sum(x => x.Score);
	}
}