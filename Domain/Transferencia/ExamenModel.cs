using System.ComponentModel.DataAnnotations;

namespace Domain.Transferencia;

public class ExamenModel {
	public int? Id { get; set; }

	public DateTime? Creacion { get; set; }
	public DateTime? Modificacion { get; set; }
	[Required]
	public string? Titulo { get; set; }
	[Required]
	public string? Tipo { get; set; }

	public DateTime? FechaInicio { get; set; }
	public DateTime? FechaFin { get; set; }

	public string? Opciones { get; set; }
	public string? Token { get; set; }

	[Required]
	public bool Activo { get; set; } = true;

	public List<PreguntaExModel> Preguntas { get; set; } = new();
}

public class PreguntaExModel {
	public int? Id { get; set; }
	public int PreguntaId { get; set; }
	public int Orden { get; set; } = 1;
	public string? Nombre { get; set; }
	public string? Dificultad { get; set; }
	public string? Legitimo { get; set; }
}
