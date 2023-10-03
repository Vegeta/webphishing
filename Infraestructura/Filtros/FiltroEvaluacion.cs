using System.Text.Json.Serialization;

namespace Infraestructura.Filtros;

public class FiltroEvaluacion : Ordenable {
	public string? Estado { get; set; }
	public string? Ocupacion { get; set; }
	public string? Genero { get; set; }
	public List<string> Actividad { get; set; } = new();
	public string? Email { get; set; }
	public string? Nombres { get; set; }
	public string? Apellidos { get; set; }
	public int? MinExito { get; set; }
	public int? MaxExito { get; set; }
	public DateTime? TomadoDesde { get; set; }
	public DateTime? TomadoHasta { get; set; }

	[JsonConverter(typeof(JsonConverterNullableInt))]
	public int? Mes { get; set; }

	[JsonConverter(typeof(JsonConverterNullableInt))]
	public int? Anio { get; set; }

	public string? Nombre { get; set; }

	public FiltroEvaluacion() {
		OrdenCampo = "fechaExamen";
		OrdenDir = "desc";
	}
}