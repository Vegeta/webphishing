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

	static string[] _meses = new[] { "enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre" };

	public static List<string> Meses => _meses.ToList();

	public static List<SelectListItem> MesesWeb(string prompt = "") {
		var l = new List<SelectListItem>();
		if (!string.IsNullOrEmpty(prompt))
			l.Add(new SelectListItem(prompt, ""));
		var i = 1;
		foreach (var item in _meses) {
			l.Add(new SelectListItem(item, i.ToString()));
			i++;
		}
		return l;
	}
}