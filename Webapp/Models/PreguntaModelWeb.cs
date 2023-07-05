using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json.Serialization;
using Domain.Transferencia;

namespace Webapp.Models;

public class PreguntaModelWeb : PreguntaModel {
	[JsonIgnore]
	public List<SelectListItem> Validez => new List<SelectListItem> {
		new SelectListItem {Text="Legitimo", Value="1"},
		new SelectListItem {Text="Phishing", Value="0"},
	};
	public List<SelectListItem> Dificultades { get; set; } = default!;
}