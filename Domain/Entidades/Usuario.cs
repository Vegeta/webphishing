using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("usuario")]
public partial class Usuario : BaseEntity {
	[Column("username")]
	[StringLength(100)]
	public string Username { get; set; } = null!;

	[Column("password")]
	[StringLength(255)]
	public string Password { get; set; } = null!;

	[Column("creacion", TypeName = "timestamp(0) without time zone")]
	public DateTime Creacion { get; set; }

	[Column("nombres")]
	[StringLength(255)]
	public string Nombres { get; set; } = null!;

	[Column("apellidos")]
	[StringLength(255)]
	public string Apellidos { get; set; } = null!;

	[Column("email")]
	[StringLength(255)]
	public string Email { get; set; } = null!;

	[Column("modificacion", TypeName = "timestamp(0) without time zone")]
	public DateTime Modificacion { get; set; }

	[Column("activo")]
	public bool? Activo { get; set; }

	[Column("cambiar_password")]
	public bool? CambiarPassword { get; set; }

	[Column("celular")]
	[StringLength(50)]
	public string? Celular { get; set; }

	[Column("tipo")]
	[StringLength(100)]
	public string? Tipo { get; set; }

	[Column("fecha_password", TypeName = "timestamp(0) without time zone")]
	public DateTime? FechaPassword { get; set; }

	[Column("perfil_id")]
	public int? PerfilId { get; set; }

	[ForeignKey("PerfilId")]
	[InverseProperty("Usuario")]
	public virtual Perfil? Perfil { get; set; }

	[InverseProperty("Usuario")]
	public virtual ICollection<Persona> Persona { get; } = new List<Persona>();

	public bool EstaActivo => Activo.HasValue && Activo.Value;
}