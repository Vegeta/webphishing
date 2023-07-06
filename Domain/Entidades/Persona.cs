using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("persona")]
public partial class Persona : BaseEntity {
	[Column("nombre")]
	[StringLength(255)]
	public string? Nombre { get; set; }

	[Column("apellido")]
	[StringLength(255)]
	public string? Apellido { get; set; }

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

	[Column("creacion", TypeName = "timestamp(6) without time zone")]
	public DateTime? Creacion { get; set; }

	[Column("modificacion", TypeName = "timestamp(6) without time zone")]
	public DateTime? Modificacion { get; set; }

	[Column("key")]
	[StringLength(255)]
	public string? Key { get; set; }

	[Column("usuario_id")]
	public int? UsuarioId { get; set; }

	[InverseProperty("Persona")]
	public virtual ICollection<SesionPersona> SesionPersona { get; } = new List<SesionPersona>();

	[ForeignKey("UsuarioId")]
	[InverseProperty("Persona")]
	public virtual Usuario? Usuario { get; set; }
}