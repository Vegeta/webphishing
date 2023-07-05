using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("sesion_grupo")]
public partial class SesionGrupo : BaseEntity {
	[Column("examen_id")]
	public int? ExamenId { get; set; }

	[Column("creacion", TypeName = "timestamp without time zone")]
	public DateTime? Creacion { get; set; }

	[Column("modificacion", TypeName = "timestamp without time zone")]
	public DateTime? Modificacion { get; set; }

	[Column("creado_por")]
	[StringLength(255)]
	public string? CreadoPor { get; set; }

	[Column("instrucciones")]
	public string? Instrucciones { get; set; }

	[Column("opciones", TypeName = "json")]
	public string? Opciones { get; set; }

	/// <summary>
	/// sync token
	/// </summary>
	[Column("token")]
	[StringLength(255)]
	public string? Token { get; set; }

	/// <summary>
	/// codigo acceso ui
	/// </summary>
	[Column("codigo_acceso")]
	[StringLength(255)]
	public string? CodigoAcceso { get; set; }

	/// <summary>
	/// codigo acceso web
	/// </summary>
	[Column("url_slug")]
	[StringLength(255)]
	public string? UrlSlug { get; set; }

	[Column("estado")]
	[StringLength(255)]
	public string? Estado { get; set; }

	[Column("inicio", TypeName = "timestamp without time zone")]
	public DateTime? Inicio { get; set; }

	[Column("fin", TypeName = "timestamp without time zone")]
	public DateTime? Fin { get; set; }

	[Column("tipo")]
	[StringLength(255)]
	public string? Tipo { get; set; }

	[Column("creador_id")]
	public int? CreadorId { get; set; }

	[ForeignKey("ExamenId")]
	[InverseProperty("SesionGrupo")]
	public virtual Examen? Examen { get; set; }

	[InverseProperty("Grupo")]
	public virtual ICollection<SesionPersona> SesionPersona { get; } = new List<SesionPersona>();
}