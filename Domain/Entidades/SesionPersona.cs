using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("sesion_persona")]
public class SesionPersona : BaseEntity {
	public const string EstadoPendiente = "pendiente";
	public const string EstadoEnCurso = "en_curso";
	public const string EstadoTerminado = "terminado";

	public static IList<string> ListaEstados() {
		return new List<string> { EstadoPendiente, EstadoEnCurso, EstadoTerminado };
	}

	[Column("examen_id")] public int? ExamenId { get; set; }

	[Column("persona_id")] public int? PersonaId { get; set; }

	[Column("fecha_examen", TypeName = "timestamp without time zone")]
	public DateTime? FechaExamen { get; set; }

	[Column("fecha_fin", TypeName = "timestamp without time zone")]
	public DateTime? FechaFin { get; set; }

	[Column("flujo", TypeName = "json")] public string? Flujo { get; set; }

	[Column("estado")] [StringLength(100)] public string? Estado { get; set; }

	[Column("cuestionario_id")] public int? CuestionarioId { get; set; }

	[Column("fecha_actividad", TypeName = "timestamp without time zone")]
	public DateTime? FechaActividad { get; set; }

	[Column("token")] [StringLength(255)] public string? Token { get; set; }

	[Column("respuesta_cuestionario", TypeName = "json")]
	public string? RespuestaCuestionario { get; set; }

	[Column("nombre")] [StringLength(255)] public string? Nombre { get; set; }

	[Column("avatar")] [StringLength(255)] public string? Avatar { get; set; }

	[Column("tipo")] [StringLength(255)] public string? Tipo { get; set; }

	[Column("score")] public int? Score { get; set; }

	[Column("max_score")] public int? MaxScore { get; set; }

	[Column("tiempo_total")] public float? TiempoTotal { get; set; }

	[Column("avg_score")] public float? AvgScore { get; set; }

	[Column("avg_tiempo")] public float? AvgTiempo { get; set; }

	[Column("exito")] public decimal? Exito { get; set; }

	[Column("percepcion")] public string? Percepcion { get; set; }

	[Column("tiempo_cuestionario")] public float? TiempoCuestionario { get; set; }

	[Column("score_cuestionario")] public float? ScoreCuestionario { get; set; }

	[ForeignKey("CuestionarioId")]
	[InverseProperty("SesionPersona")]
	public virtual Cuestionario? Cuestionario { get; set; }

	[ForeignKey("ExamenId")]
	[InverseProperty("SesionPersona")]
	public virtual Examen? Examen { get; set; } = null!;

	[ForeignKey("PersonaId")]
	[InverseProperty("SesionPersona")]
	public virtual Persona? Persona { get; set; }

	[InverseProperty("Sesion")] public virtual ICollection<SesionRespuesta> SesionRespuesta { get; } = new List<SesionRespuesta>();

	[InverseProperty("Sesion")] public virtual ICollection<CuestionarioRespuesta> CuestionarioRespuestas { get; } = new List<CuestionarioRespuesta>();
}