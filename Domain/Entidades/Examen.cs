using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("examen")]
public partial class Examen : BaseEntity {
	[Required]
	[Column("creacion", TypeName = "timestamp without time zone")]
	public DateTime? Creacion { get; set; }

	[Required]
	[Column("modificacion", TypeName = "timestamp without time zone")]
	public DateTime? Modificacion { get; set; }

	[Column("titulo")][StringLength(255)] public string? Titulo { get; set; }

	[Column("tipo")][StringLength(255)] public string? Tipo { get; set; }

	[Column("fecha_inicio", TypeName = "timestamp without time zone")]
	public DateTime? FechaInicio { get; set; }

	[Column("fecha_fin", TypeName = "timestamp without time zone")]
	public DateTime? FechaFin { get; set; }

	[Column("activo")] public bool? Activo { get; set; }

	[Column("token")][StringLength(255)] public string? Token { get; set; }

	[Column("creador_id")] public int? CreadorId { get; set; }

	[Column("cuestionario_pos")] public int? CuestionarioPos { get; set; }

	[Column("opciones", TypeName = "json")]
	public string? Opciones { get; set; }

	[InverseProperty("Examen")]
	public virtual ICollection<ExamenPregunta> ExamenPregunta { get; } = new List<ExamenPregunta>();

	[InverseProperty("Examen")]
	public virtual ICollection<SesionPersona> SesionPersona { get; } = new List<SesionPersona>();
}