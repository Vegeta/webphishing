using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructura.Persistencia;

/// <summary>
/// Utilitario para definir ordenes con expresiones de linq y adicionar el comando a un queryable.
/// Ejecutar siempre luego de los filtros where
/// </summary>
/// <typeparam name="T"></typeparam>
public class SortExpressionHelper<T> {
	public Dictionary<string, IList<Func<T, object>>> Config { get; set; }
	public string DefaultColumn { get; set; } = "";
	public string DefaultOrder { get; set; }

	public SortExpressionHelper() {
		DefaultOrder = "asc";
		Config = new Dictionary<string, IList<Func<T, object>>>();
	}

	public SortExpressionHelper<T> WithDefault(string column) {
		DefaultColumn = column;
		return this;
	}

	public SortExpressionHelper<T> Add(string prop, Func<T, object> expression, params Func<T, object>[] expressions) {
		var lista = expressions.ToList();
		lista.Insert(0, expression);
		Config[prop] = lista;
		return this;
	}

	public bool HasKey(string key) { return Config.ContainsKey(key); }

	public IList<Func<T, object>> Get(string key) {
		return Config[key];
	}

	public IQueryable<T> SetOrder(IQueryable<T> queryable, string sortColumn, string sortDirection = "") {
		if (string.IsNullOrEmpty(sortColumn) || !HasKey(sortColumn))
			return queryable.AsQueryable();

		var column = string.IsNullOrEmpty(sortColumn) ? DefaultColumn : sortColumn;
		if (string.IsNullOrEmpty(column))
			throw new ApplicationException("La columna para ordenar no debe ser nula");
		if (!Config.ContainsKey(column))
			throw new ApplicationException("La columna " + column + " no ha sido definida para ordenar");
		var exp = Config[column];
		var isdesc = (sortDirection ?? "").ToLower() == "desc";
		if (!isdesc) {
			var lista = queryable.OrderBy(exp[0]);
			for (var i = 1; i < exp.Count; i++) {
				lista = lista.ThenBy(exp[i]);
			}
			return lista.AsQueryable();
		}
		var listadesc = queryable.OrderByDescending(exp[0]);
		for (var i = 1; i < exp.Count; i++) {
			listadesc = listadesc.ThenByDescending(exp[i]);
		}
		return listadesc.AsQueryable();
		//return (!isdesc ? lista.OrderBy(exp) : lista.OrderByDescending(exp)).AsQueryable();
	}
}