using Domain.Entidades;
using Infraestructura.Examenes;

namespace Infraestructura.Reportes;

public class InteraccionesStats {
	public TotalesInter TotalesLista(IList<InteraccionesDto> lista) {
		var total = new TotalesInter();

		var clicks = new List<int>();

		foreach (var dto in lista) {
			var parcial = SumarInter(dto);
			total.Hovers.AddRange(parcial.Hovers);
			total.TotalClicks += parcial.TotalClicks;
			total.HoverTotal += parcial.HoverTotal;
			clicks.Add(parcial.TotalClicks);
		}
		if (total.Hovers.Any())
			total.HoverAvg = total.Hovers.Average();
		if (clicks.Any())
			total.AvgClicksPregunta = (decimal)clicks.Average();
		return total;
	}

	public void CompletarRespuesta(SesionRespuesta resp, InteraccionesDto dto) {
		var sumas = SumarInter(dto);
		resp.HoverTotal = sumas.HoverTotal;
		resp.HoverAvg = sumas.HoverAvg;
		resp.ClicksTotal = sumas.TotalClicks;
	}

	public TotalesInter SumarInter(InteraccionesDto dto) {
		var res = new TotalesInter();
		foreach (var fila in dto) {
			if (fila.Hover > 0)
				res.Hovers.Add(fila.Hover);
			res.TotalClicks += fila.Clicks;
		}

		res.HoverTotal = res.Hovers.Sum();
		if (res.Hovers.Count > 0)
			res.HoverAvg = res.Hovers.Average();

		return res;
	}
}

public class TotalesInter {
	public int TotalClicks { get; set; }
	public decimal HoverAvg { get; set; }
	public decimal HoverTotal { get; set; }

	public decimal AvgClicksPregunta { get; set; }

	public List<decimal> Hovers { get; set; } = new();
}