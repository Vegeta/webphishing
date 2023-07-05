using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("pregunta")]
public partial class Pregunta : BaseEntity {
	[Column("nombre")]
	[StringLength(255)]
	public string? Nombre { get; set; }

	[Column("legitimo")]
	public short? Legitimo { get; set; }

	[Column("dificultad")]
	[StringLength(255)]
	public string? Dificultad { get; set; }

	[Column("imagen_retro")]
	[StringLength(255)]
	public string? ImagenRetro { get; set; }

	[Column("explicacion")]
	public string? Explicacion { get; set; }

	[Column("subject")]
	[StringLength(255)]
	public string? Subject { get; set; }

	[Column("sender")]
	[StringLength(255)]
	public string? Sender { get; set; }

	[Column("email")]
	[StringLength(255)]
	public string? Email { get; set; }

	[Column("html")]
	public string? Html { get; set; }

	[Column("adjuntos", TypeName = "json")]
	public string? Adjuntos { get; set; }

	[Column("data", TypeName = "json")]
	public string? Data { get; set; }

	[Column("links", TypeName = "json")]
	public string? Links { get; set; }

	[InverseProperty("Pregunta")]
	public virtual ICollection<ExamenPregunta> ExamenPregunta { get; } = new List<ExamenPregunta>();

	[InverseProperty("Pregunta")]
	public virtual ICollection<SesionRespuesta> SesionRespuesta { get; } = new List<SesionRespuesta>();
}