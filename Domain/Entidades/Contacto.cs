using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("contacto")]
public class Contacto : BaseEntity {
	[Column("fecha", TypeName = "timestamp(0) without time zone")]
	public DateTime Fecha { get; set; }

	[Column("nombre")] [StringLength(100)] public string? Nombre { get; set; }

	[Column("email")] [StringLength(100)] public string? Email { get; set; }

	[Column("titulo")] [StringLength(255)] public string? Titulo { get; set; }

	[Column("mensaje")] public string? Mensaje { get; set; }

	[Column("ipaddress")] public string? IpAddress { get; set; }
}