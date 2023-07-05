using Infraestructura.Persistencia;

namespace Webapp.Models.Datatables;

/// <summary>
/// Definicion del resultado de la consulta desde el backend
/// </summary>
public class DatatablesResult {
	public int Draw { get; set; } // requerido por la ui asi no se use
	public int RecordsTotal { get; set; }
	public int RecordsFiltered { get; set; }
	public object? Data { get; set; }

	public DatatablesResult() { }

}
