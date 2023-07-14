using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("sesion_persona")]
public partial class SesionPersona : BaseEntity {
	[Column("examen_id")]
	public int ExamenId { get; set; }

	[Column("persona_id")]
	public int? PersonaId { get; set; }

	[Column("fecha_examen", TypeName = "timestamp without time zone")]
	public DateTime? FechaExamen { get; set; }

	[Column("fecha_fin", TypeName = "timestamp without time zone")]
	public DateTime? FechaFin { get; set; }

	[Column("flujo", TypeName = "json")]
	public string? Flujo { get; set; }

	[Column("estado")]
	[StringLength(100)]
	public string? Estado { get; set; }

	[Column("cuestionario_id")]
	public int? CuestionarioId { get; set; }

	[Column("fecha_actividad", TypeName = "timestamp without time zone")]
	public DateTime? FechaActividad { get; set; }

	[Column("token")]
	[StringLength(255)]
	public string? Token { get; set; }

	[Column("respuesta_cuestionario", TypeName = "json")]
	public string? RespuestaCuestionario { get; set; }

	[Column("grupo_id")]
	public int? GrupoId { get; set; }

	[Column("nombre")]
	[StringLength(255)]
	public string? Nombre { get; set; }

	[Column("avatar")]
	[StringLength(255)]
	public string? Avatar { get; set; }

	[Column("tipo")]
	[StringLength(255)]
	public string? Tipo { get; set; }

	[Column("score")]
	public int? Score { get; set; }

	[Column("tiempo_total")]
	public float? TiempoTotal { get; set; }

	[Column("avg_score")]
	public float? AvgScore { get; set; }

	[Column("avg_tiempo")]
	public float? AvgTiempo { get; set; }

	[Column("tasa_exito")]
	public decimal? TasaExito{ get; set; }

	[ForeignKey("CuestionarioId")]
	[InverseProperty("SesionPersona")]
	public virtual Cuestionario? Cuestionario { get; set; }

	[ForeignKey("ExamenId")]
	[InverseProperty("SesionPersona")]
	public virtual Examen Examen { get; set; } = null!;

	[ForeignKey("GrupoId")]
	[InverseProperty("SesionPersona")]
	public virtual SesionGrupo? Grupo { get; set; }

	[ForeignKey("PersonaId")]
	[InverseProperty("SesionPersona")]
	public virtual Persona? Persona { get; set; }

	[InverseProperty("Sesion")]
	public virtual ICollection<SesionRespuesta> SesionRespuesta { get; } = new List<SesionRespuesta>();
}