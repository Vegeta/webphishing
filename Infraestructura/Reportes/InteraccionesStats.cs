using Domain.Entidades;
using Infraestructura.Examenes;

namespace Infraestructura.Reportes;

public class InteraccionesStats {
	public DatosInter Calcular(IList<InteraccionesDto> lista) {
		var total = new DatosInter();

		var clicks = new List<int>();

		foreach (var dto in lista) {
			var parcial = SumarInter(dto);
			total.Hovers.AddRange(parcial.Hovers);
			total.TotalClicks += parcial.TotalClicks;
			total.HoverTotal += parcial.HoverTotal;
			clicks.Add(parcial.TotalClicks);
		}

		total.HoverAvg = total.Hovers.Average();
		total.AvgClicksPregunta = (decimal)clicks.Average();
		return total;
	}

	public void SetRespuesta(SesionRespuesta resp, InteraccionesDto dto) {
		var sumas = SumarInter(dto);
		resp.HoverTotal = sumas.HoverTotal;
		resp.HoverAvg = sumas.HoverAvg;
		resp.ClicksTotal = sumas.TotalClicks;
	}

	public DatosInter SumarInter(InteraccionesDto dto) {
		var res = new DatosInter();
		if (dto.HoverFiles != null)
			res.Hovers.AddRange(dto.HoverFiles.Values);
		if (dto.HoverLinks != null)
			res.Hovers.AddRange(dto.HoverLinks.Values);

		res.HoverTotal = res.Hovers.Sum();
		if (res.Hovers.Count > 0)
			res.HoverAvg = res.Hovers.Average();

		if (dto.ClickLinks != null)
			res.TotalClicks += dto.ClickLinks.Values.Sum();
		if (dto.ClickFiles != null)
			res.TotalClicks += dto.ClickFiles.Values.Sum();

		return res;
	}

	public static List<FilaInter> TablaInter(InteraccionesDto dto) {
		var mapa = new Dictionary<string, FilaInter>();
		if (dto.ClickLinks != null) {
			foreach (var par in dto.ClickLinks) {
				if (!mapa.ContainsKey(par.Key))
					mapa[par.Key] = new FilaInter("link", par.Key);
				mapa[par.Key].Clicks += par.Value;
			}
		}
		if (dto.ClickFiles != null) {
			foreach (var par in dto.ClickFiles) {
				if (!mapa.ContainsKey(par.Key))
					mapa[par.Key] = new FilaInter("file", par.Key);
				mapa[par.Key].Clicks += par.Value;
			}
		}
		if (dto.HoverLinks != null) {
			foreach (var par in dto.HoverLinks) {
				if (!mapa.ContainsKey(par.Key))
					mapa[par.Key] = new FilaInter("link", par.Key);
				mapa[par.Key].Hover += par.Value;
			}
		}
		if (dto.HoverFiles != null) {
			foreach (var par in dto.HoverFiles) {
				if (!mapa.ContainsKey(par.Key))
					mapa[par.Key] = new FilaInter("file", par.Key);
				mapa[par.Key].Hover += par.Value;
			}
		}
		return mapa.Values.ToList();
	}
}

public class DatosInter {
	public int TotalClicks { get; set; }
	public decimal HoverAvg { get; set; }
	public decimal HoverTotal { get; set; }

	public decimal AvgClicksPregunta { get; set; }

	public List<decimal> Hovers { get; set; } = new();
}

public class FilaInter {
	public string Tipo { get; set; } = "";
	public string Link { get; set; } = "";
	public int Clicks { get; set; }
	public decimal Hover { get; set; }

	public FilaInter() {
	}

	public FilaInter(string tipo, string link) {
		Tipo = tipo;
		Link = link;
	}
}