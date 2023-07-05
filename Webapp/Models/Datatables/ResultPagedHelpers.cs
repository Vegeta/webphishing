using Infraestructura.Persistencia;

namespace Webapp.Models.Datatables; 

public static class ResultPagedHelpers {

	/// <summary>
	/// Utilitario para construir un resultado de Datatables desde un objeto paginado
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="paged"></param>
	/// <param name="draw"></param>
	/// <returns></returns>
	public static DatatablesResult FromPaged<T>(PagedResult<T> paged, int? draw = null) where T : class {

		var dt = new DatatablesResult {
			RecordsTotal = paged.RowCount,
			RecordsFiltered = paged.RowCount,
			Data = paged.Results.ToArray()
		};

		if (draw.HasValue) dt.Draw = draw.Value;
		return dt;
	}

}