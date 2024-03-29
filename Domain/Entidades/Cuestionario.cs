﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("cuestionario")]
public partial class Cuestionario : BaseEntity {
	[Column("creacion", TypeName = "timestamp without time zone")]
	public DateTime? Creacion { get; set; }

	[Column("modificacion", TypeName = "timestamp without time zone")]
	public DateTime? Modificacion { get; set; }

	[Column("titulo")][StringLength(255)] public string? Titulo { get; set; }
	[Column("instrucciones")] public string? Instrucciones { get; set; }

	[Column("preguntas", TypeName = "json")]
	public string? Preguntas { get; set; }

	[Column("activo")] public bool? Activo { get; set; }

	[InverseProperty("Cuestionario")]
	public virtual ICollection<SesionPersona> SesionPersona { get; } = new List<SesionPersona>();
}