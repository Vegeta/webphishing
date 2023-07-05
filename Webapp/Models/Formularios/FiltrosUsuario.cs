using Microsoft.AspNetCore.Mvc.Rendering;

namespace Webapp.Models.Formularios;

public class FiltrosUsuario : Ordenable {
	public string? Email { get; set; }
	public string? Nombre { get; set; }
	public string? Estado { get; set; }
	public List<SelectListItem> Estados = new List<SelectListItem> {
		new SelectListItem("Activo", "1"),
		new SelectListItem("Inactivo", "0"),
	};


}
