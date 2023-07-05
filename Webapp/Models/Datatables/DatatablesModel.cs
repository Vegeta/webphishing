using Webapp.Web;

namespace Webapp.Models.Datatables;

/// <summary>
/// Es el modelo json que se envia desde el componente Datatables de la UI para listas paginadas.
/// Se incluye un campo adicional flexible para obtener datos de filtros y mas
/// </summary>
public class DatatablesModel {
	public int Draw { get; set; }
	public int Length { get; set; }
	public int Start { get; set; }
	public int Page { get; set; } = 0;
	public string JsonFiltros { get; set; } = "";
	public List<DatatablesOrder> Order { get; set; } = new List<DatatablesOrder>();

	public T GetFiltros<T>(T defVal) {
		if (string.IsNullOrEmpty(JsonFiltros))
			return defVal;
		return JSON.Parse<T>(JsonFiltros);
	}

	public bool HasOrder => Order.Count > 0;

	public DatatablesOrder FirstOrder() {
		return Order.FirstOrDefault() ?? new DatatablesOrder();
	}

}
