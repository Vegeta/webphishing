using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entidades;

/// <summary>
/// Vista combinada de resultado con info persona
/// </summary>
[Keyless]
public class VSesionPersona {
	[Column("id")]
	public int Id { get; set; }

	[Column("examen_id")]
	public int? ExamenId { get; set; }

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

	[Column("max_score")]
	public int? MaxScore { get; set; }

	[Column("tiempo_total")]
	public float? TiempoTotal { get; set; }

	[Column("avg_score")]
	public float? AvgScore { get; set; }

	[Column("avg_tiempo")]
	public float? AvgTiempo { get; set; }

	[Column("exito")]
	public decimal? Exito { get; set; }

	[Column("percepcion")]
	public string? Percepcion { get; set; }

	[Column("tiempo_cuestionario")]
	public float? TiempoCuestionario { get; set; }

	[Column("score_cuestionario")]
	public float? ScoreCuestionario { get; set; }

	// porcion de persona

	[Column("nombres")]
	public string? Nombres { get; set; }

	[Column("apellidos")]
	public string? Apellidos { get; set; }

	[Column("ocupacion")]
	[StringLength(255)]
	public string? Ocupacion { get; set; }

	[Column("actividad")]
	[StringLength(255)]
	public string? Actividad { get; set; }

	[Column("email")]
	[StringLength(255)]
	public string? Email { get; set; }

	[Column("genero")]
	[StringLength(255)]
	public string? Genero { get; set; }

	[Column("edad")]
	public short? Edad { get; set; }

	[Column("experiencia_seguridad")]
	public short? ExperienciaSeguridad { get; set; }

	[Column("usuario_id")]
	public int? UsuarioId { get; set; }

	public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();

}