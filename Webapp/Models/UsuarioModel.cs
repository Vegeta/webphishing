using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Webapp.Models;

public class UsuarioModel {
	public int? Id { get; set; }
	[Required]
	public string Username { get; set; } = null!;
	[Required]
	public string Password { get; set; } = null!;
	[Required]
	public string Nombres { get; set; } = null!;
	[Required]
	public string Apellidos { get; set; } = null!;

	public string Email { get; set; } = null!;

	public string? Celular { get; set; }

	public string? Tipo { get; set; }

	[Required]
	public bool Activo { get; set; } = true;


	public bool? CambiarPassword { get; set; }

	public DateTime? FechaPassword { get; set; }
	public DateTime? Creacion { get; set; }
	public DateTime? Modificacion { get; set; }

	public int? PerfilId { get; set; }

	[JsonIgnore]
	public List<SelectListItem> Perfiles { get; set; } = default!;
}