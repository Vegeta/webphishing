using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("auditoria")]
public partial class Auditoria: BaseEntity
{
    [Column("fecha", TypeName = "timestamp(0) without time zone")]
    public DateTime Fecha { get; set; }

    [Column("nivel")]
    [StringLength(50)]
    public string? Nivel { get; set; }

    [Column("mensaje")]
    [StringLength(200)]
    public string Mensaje { get; set; } = null!;

    [Column("usuario")]
    [StringLength(120)]
    public string? Usuario { get; set; }

    [Column("modulo")]
    [StringLength(100)]
    public string? Modulo { get; set; }

    [Column("datos")]
    public string? Datos { get; set; }

    [Column("ipaddress")]
    [StringLength(100)]
    public string? Ipaddress { get; set; }

    [Column("fuente")]
    [StringLength(100)]
    public string? Fuente { get; set; }

    [Column("url")]
    [StringLength(500)]
    public string? Url { get; set; }
}
