using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("sesion_respuesta")]
public partial class SesionRespuesta : BaseEntity {
	[Column("sesion_id")] public int SesionId { get; set; }

	[Column("pregunta_id")] public int PreguntaId { get; set; }

	[Column("respuesta")]
	[StringLength(255)]
	public string? Respuesta { get; set; }

	[Column("interacciones", TypeName = "json")]
	public string? Interacciones { get; set; }

	[Column("tiempo")] public float? Tiempo { get; set; }

	[Column("inicio", TypeName = "timestamp without time zone")]
	public DateTime? Inicio { get; set; }

	[Column("fin", TypeName = "timestamp without time zone")]
	public DateTime? Fin { get; set; }

	[Column("score")] public int? Score { get; set; }

	[DataType("text")] public string? Comentario { get; set; }

	[Column("dificultad")]
	[StringLength(255)]
	public string? Dificultad { get; set; }

	[Column("hover_total")] 
	public decimal? HoverTotal { get; set; }

	[Column("hover_avg")] 
	public decimal? HoverAvg { get; set; }
	
	[Column("clicks_total")] 
	public int? ClicksTotal { get; set; }

	[ForeignKey("PreguntaId")]
	[InverseProperty("SesionRespuesta")]
	public virtual Pregunta Pregunta { get; set; } = null!;

	[ForeignKey("SesionId")]
	[InverseProperty("SesionRespuesta")]
	public virtual SesionPersona Sesion { get; set; } = null!;
}