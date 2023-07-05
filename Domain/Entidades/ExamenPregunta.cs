using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("examen_pregunta")]
public partial class ExamenPregunta : BaseEntity {
	[Column("examen_id")] public int ExamenId { get; set; }

	[Column("pregunta_id")] public int PreguntaId { get; set; }

	[Column("orden")] public int? Orden { get; set; }

	[ForeignKey("ExamenId")]
	[InverseProperty("ExamenPregunta")]
	public virtual Examen Examen { get; set; } = null!;

	[ForeignKey("PreguntaId")]
	[InverseProperty("ExamenPregunta")]
	public virtual Pregunta Pregunta { get; set; } = null!;
}