using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("cuestionario_respuesta")]
public partial class CuestionarioRespuesta : BaseEntity {

	[Column("sesion_id")]
	public int SesionId { get; set; }

	[Column("pregunta")]
	[StringLength(255)]
	public string? Pregunta { get; set; }

	[Column("respuesta")]
	[StringLength(50)]
	public string? Respuesta { get; set; }

	[Column("puntaje")]
	public int Puntaje { get; set; } = 0;

	[Column("dimension")]
	[StringLength(50)]
	public string? Dimension { get; set; }

	[ForeignKey("SesionId")]
	public virtual SesionPersona Sesion { get; set; } = null!;

}