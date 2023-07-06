using Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Webapp.Models;

public class OpcionesConfig {

	public static List<SelectListItem> DificultadesCombo() {
		return ComboDict(DificultadPregunta.Mapa());
	}

	public static List<SelectListItem> TipoExamenCombo() {
		return ComboDict(TipoExamen.Mapa());
	}

	public static List<SelectListItem> ComboDict(IDictionary<string, string> mapa, string? primeraOpcion = null) {
		var list = new List<SelectListItem>();
		if (primeraOpcion != null)
			list.Add(new SelectListItem(primeraOpcion, ""));
		foreach (var item in mapa) {
			list.Add(new SelectListItem(item.Value, item.Key));
		}
		return list;
	}
}


