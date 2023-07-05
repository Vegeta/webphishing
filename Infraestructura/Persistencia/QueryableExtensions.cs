using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructura.Persistencia {
	/// <summary>
	/// Extensiones para gestionar paginacion de forma mas facil
	/// </summary>
	public static class QueryableExtensions {
		// https://www.codingame.com/playgrounds/5363/paging-with-entity-framework-core
		public static PagedResult<T> GetPaged<T>(this IQueryable<T> query,
										int page, int pageSize) where T : class {
			var result = new PagedResult<T>();
			result.CurrentPage = page;
			result.PageSize = pageSize;
			result.RowCount = query.Count();

			var pageCount = (double)result.RowCount / pageSize;
			result.PageCount = (int)Math.Ceiling(pageCount);

			var skip = (page - 1) * pageSize;
			result.Results = query.Skip(skip).Take(pageSize).ToList();

			return result;
		}

		public static PagedResult<T> GetPagedSimple<T>(this IQueryable<T> query,
										int page, int pageSize) where T : class {
			var result = new PagedResult<T>();
			result.CurrentPage = page;
			result.PageSize = pageSize;

			var skip = (page - 1) * pageSize;
			result.Results = query.Skip(skip).Take(pageSize).ToList();

			return result;
		}

	}
}
