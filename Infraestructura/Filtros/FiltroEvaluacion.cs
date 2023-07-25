namespace Infraestructura.Filtros;

public class FiltroEvaluacion : Ordenable {
	public string? Email { get; set; }
	public string? Nombres { get; set; }
	public string? Apellidos { get; set; }
	public int? MinExito { get; set; }
	public int? MaxExito { get; set; }
	public DateTime? TomadoDesde { get; set; }
	public DateTime? TomadoHasta { get; set; }
	public int? Mes { get; set; }
	public int? Anio { get; set; }

	public FiltroEvaluacion() {
		OrdenCampo = "fechaExamen";
		OrdenDir = "desc";
	}
}