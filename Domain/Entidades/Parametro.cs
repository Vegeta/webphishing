using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("parametro")]
public partial class Parametro : BaseEntity {
	[Column("fecha_creacion", TypeName = "timestamp(6) without time zone")]
	public DateTime FechaCreacion { get; set; }

	[Column("fecha_modificacion", TypeName = "timestamp(6) without time zone")]
	public DateTime FechaModificacion { get; set; }

	[Column("datos", TypeName = "json")]
	public string? Datos { get; set; }

	[Column("nombre")]
	[StringLength(255)]
	public string Nombre { get; set; } = null!;

	[Column("tipo")]
	[StringLength(100)]
	public string? Tipo { get; set; }

	[Column("comentario")]
	[StringLength(500)]
	public string? Comentario { get; set; }
}