using Microsoft.Build.Framework;

namespace Webapp.Models;

public class CuestionarioModelWeb {
	public int? Id { get; set; }

	public DateTime? Creacion { get; set; }
	public DateTime? Modificacion { get; set; }
	[Required]
	public string? Titulo { get; set; }

	public List<CuestPreguntaModel> Preguntas { get; set; } = new();
}

public class CuestPreguntaModel {
	public int Orden { get; set; }
	public string? Texto { get; set; }

}