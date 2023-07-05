using Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Webapp.Models;

public class PreguntaConfig {

	public static List<SelectListItem> DificultadesCombo() {
		return ComboDict(DificultadPregunta.Mapa());
	}

	public static List<SelectListItem> TipoExamenCombo() {
		return ComboDict(TipoExamen.Mapa());
	}

	public static List<SelectListItem> ComboDict(IDictionary<string, string> mapa) {
		var list = new List<SelectListItem>();
		foreach (var item in mapa) {
			list.Add(new SelectListItem(item.Value, item.Key));
		}
		return list;
	}
}


