﻿using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;

namespace Infraestructura.Persistencia;

public class SimpleSqlBuilder {
	private readonly MySqlBuilder _builder;
	private Dictionary<string, bool> _existsClause = new();

	private int specialPar = 0;
	private bool dirty = false;

	private List<JoinDef> _joins = new();

	class JoinDef {
		public string Tipo { get; set; }
		public string Sql { get; set; }
		public dynamic Parameters { get; set; }
	}

	public SimpleSqlBuilder() {
		_builder = new MySqlBuilder();
	}

	protected void CheckClause(string clause) {
		dirty = true;
		_existsClause[clause] = true;
	}

	public class MySqlBuilder : SqlBuilder {
		public SqlBuilder From(string sql, dynamic parameters = null) =>
			AddClause("from", sql, parameters, " , ", "FROM ", "\n", false);

		//  protected SqlBuilder AddClause(string name, string sql, object parameters, string joiner, string prefix = "", string postfix = "", bool isInclusive = false)

		public SqlBuilder Add(string name, string sql, object parameters, string joiner, string prefix = "",
			string postfix = "", bool isInclusive = false) {
			return AddClause(name, sql, parameters, joiner, prefix, postfix, isInclusive);
		}

	}

	public SimpleSqlBuilder From(string exp, dynamic parameters = null) {
		CheckClause("from");
		_builder.From(exp, parameters);
		return this;
	}

	public SimpleSqlBuilder Select(string exp, dynamic parameters = null) {
		CheckClause("select");
		_builder.Select(exp, parameters);
		return this;
	}

	public SimpleSqlBuilder Where(string exp, dynamic parameters = null) {
		CheckClause("where");
		if (Regex.IsMatch(exp, @"^(\w|_)+$") && parameters != null) {
			specialPar++;
			var name = $"pes{specialPar}";
			exp = $"{exp} = @{name}";
			var d = new Dictionary<string, object>();
			d[name] = parameters;
			_builder.Where(exp, d);
			return this;
		}
		_builder.Where(exp, parameters);
		return this;
	}

	protected SimpleSqlBuilder InClause<T>(string op, string field, ICollection<T> list) {
		CheckClause("where");
		specialPar++;
		var i = 0;

		dynamic cust = new ExpandoObject();
		var parStr = new List<string>();
		foreach (var item in list) {
			var par = $"p{specialPar}_{i}";
			((IDictionary<string, object>)cust)[par] = item;
			parStr.Add("@" + par);
			i++;
		}

		var instr = string.Join(",", parStr);
		_builder.Where($"{field} {op} ({instr})", cust);
		return this;
	}

	public SimpleSqlBuilder WhereIn<T>(string field, ICollection<T> list) {
		return InClause("in", field, list);
	}

	public SimpleSqlBuilder WhereNotIn<T>(string field, ICollection<T> list) {
		return InClause("not in", field, list);
	}

	public SimpleSqlBuilder OrderBy(string exp, dynamic parameters = null) {
		CheckClause("orderby");
		_builder.OrderBy(exp, parameters);
		return this;
	}

	public SimpleSqlBuilder GroupBy(string exp, dynamic parameters = null) {
		CheckClause("groupby");
		_builder.GroupBy(exp, parameters);
		return this;
	}

	public SimpleSqlBuilder Having(string exp, dynamic parameters = null) {
		CheckClause("having");
		_builder.Having(exp, parameters);
		return this;
	}

	public SimpleSqlBuilder Join(string exp, dynamic parameters = null) {
		CheckClause("join");
		_joins.Add(new JoinDef {
			Tipo = "JOIN",
			Sql = exp,
			Parameters = parameters
		});
		return this;
	}

	public SimpleSqlBuilder InnerJoin(string exp, dynamic parameters = null) {
		CheckClause("join");
		_joins.Add(new JoinDef {
			Tipo = "INNER JOIN",
			Sql = exp,
			Parameters = parameters
		});
		return this;
	}

	public SimpleSqlBuilder LeftJoin(string exp, dynamic parameters = null) {
		CheckClause("join");
		_joins.Add(new JoinDef {
			Tipo = "LEFT JOIN",
			Sql = exp,
			Parameters = parameters
		});
		return this;
	}

	public SimpleSqlBuilder RightJoin(string exp, dynamic parameters = null) {
		CheckClause("join");
		_joins.Add(new JoinDef {
			Tipo = "RIGHT JOIN",
			Sql = exp,
			Parameters = parameters
		});
		return this;
	}

	public SimpleSqlBuilder Limit(int num) {
		CheckClause("limit");
		_builder.Add("limit", num.ToString(), null!, "", "LIMIT ", "\n", false);
		return this;
	}

	private SqlBuilder.Template tpl;
	protected void BuildStuff() {
		var checks = new List<string>() {
			"select", "from", "joins", "where",
			"groupby", "having", "orderby", "limit"
		};

		if (!_existsClause.ContainsKey("select")) {
			CheckClause("select");
			_builder.Select("*");
		}

		var ss = new StringBuilder("select ");

		foreach (var tipo in checks) {
			if (!_existsClause.ContainsKey(tipo))
				continue;
			if (tipo == "joins") {
				if (!_joins.Any())
					continue;
				var i = 0;
				foreach (var j in _joins) {
					var strJoin = $"\n{j.Tipo} ";
					_builder.Add($"join{i}", j.Sql, j.Parameters,
						strJoin, strJoin, "\n", false);
					ss.AppendFormat($"/**{strJoin}**/ ");
					i++;
				}
				continue;
			}
			ss.AppendFormat($"/**{tipo}**/ ");
		}

		tpl = _builder.AddTemplate(ss.ToString());
	}

	public object Parameters {
		get {
			if (dirty) {
				BuildStuff();
				dirty = false;
			}
			return tpl.Parameters;
		}
	}

	public string Sql {
		get {
			if (dirty) {
				BuildStuff();
				dirty = false;
			}
			return tpl.RawSql;
		}
	}

}