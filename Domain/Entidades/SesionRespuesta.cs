﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Comunes;

namespace Domain.Entidades;

[Table("sesion_respuesta")]
public partial class SesionRespuesta : BaseEntity {
	[Column("sesion_id")]
	public int SesionId { get; set; }

	[Column("pregunta_id")]
	public int PreguntaId { get; set; }

	[Column("respuesta")]
	[StringLength(255)]
	public string? Respuesta { get; set; }

	[Column("clicks", TypeName = "json")]
	public string? Clicks { get; set; }

	[Column("tiempo")]
	public float? Tiempo { get; set; }

	[Column("inicio", TypeName = "timestamp without time zone")]
	public DateTime? Inicio { get; set; }

	[Column("fin", TypeName = "timestamp without time zone")]
	public DateTime? Fin { get; set; }

	[Column("resultado")]
	[StringLength(255)]
	public string? Resultado { get; set; }

	[ForeignKey("PreguntaId")]
	[InverseProperty("SesionRespuesta")]
	public virtual Pregunta Pregunta { get; set; } = null!;

	[ForeignKey("SesionId")]
	[InverseProperty("SesionRespuesta")]
	public virtual SesionPersona Sesion { get; set; } = null!;
}