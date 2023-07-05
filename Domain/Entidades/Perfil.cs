using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("perfil")]
public partial class Perfil : BaseEntity {
	[Column("nombre")]
	[StringLength(100)]
	public string Nombre { get; set; } = "";

	[Column("identificador")]
	[StringLength(100)]
	public string Identificador { get; set; } = null!;

	[Column("permisos", TypeName = "json")]
	public string? Permisos { get; set; }

	[InverseProperty("Perfil")]
	public virtual ICollection<Usuario> Usuario { get; } = new List<Usuario>();

}