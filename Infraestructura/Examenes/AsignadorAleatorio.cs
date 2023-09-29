using Dapper;
using Domain;
using Domain.Entidades;
using Infraestructura.Examenes.Asignacion;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Examenes;

/// <summary>
/// Asignador para Examenes generales aleatorios que usan to do el pool
/// de preguntas
/// </summary>
public class AsignadorAleatorio : IAsignadorExamen {
	private readonly AppDbContext _db;
	private static readonly Random Rng = new();

	public AlgoritmoAsignacion Algoritmo { get; }

	public AsignadorAleatorio(AppDbContext db) {
		_db = db;
		Algoritmo = new AlgoritmoAsignacion();
	}

	public FlujoExamenDto CrearFlujo(ConfigExamen config) {
		var flujo = new FlujoExamenDto {
			NumPreguntas = config.NumPreguntas,
			CuestionarioPos = config.CuestionarioPos,
			Aleatorio = true,
			Tipo = TipoExamen.Predeterminado
		};

		var inicio = Algoritmo.SiguienteAsignacion(0, 0) ?? new Step();
		if (!inicio.DebeAsignar) {
			flujo.Error = "No se encontraron preguntas para asignar";
			return flujo;
		}

		AsignarPreguntas(flujo, inicio);
		return flujo;
	}

	public void ResolverPreguntas(ConfigExamen config, FlujoExamenDto flujo) {
		var respondidas = flujo.Respondidas;

		var next = Algoritmo.SiguienteAsignacion(respondidas, flujo.Score);
		if (next != null) {
			AsignarPreguntas(flujo, next);
		}
	}

	public void AsignarPreguntas(FlujoExamenDto flujo, Step pasoAlgo) {
		if (!pasoAlgo.DebeAsignar)
			return;
		var idsUsados = flujo.Pasos
			.Where(x => x.Accion == "pregunta")
			.Select(x => x.EntidadId).ToList();
		var cuenta = idsUsados.Count;
		// TODO control num preguntas
		var lista = new List<PasoExamen>();
		foreach (var dificultad in pasoAlgo.TomarPreguntas) {
			if (cuenta >= flujo.NumPreguntas)
				break;
			var preg = SiguientePregunta(dificultad, idsUsados);
			if (preg == null)
				continue;
			idsUsados.Add(preg.Id);
			var item = new PasoExamen {
				Accion = "pregunta",
				EntidadId = preg.Id,
				Dificultad = preg.Dificultad ?? DificultadPregunta.Facil,
				Real = preg.Legitimo == 0 ? "phishing" : "legitimo",
			};
			//flujo.Pasos.Add(item);
			lista.Add(item);
			flujo.MaxScore += DificultadPregunta.ScoreRespuesta(item.Dificultad);
			cuenta++;
		}
		if (lista.Count == 0)
			return;
		// randomizar la lista aun mas, comprobar con el flujo?
		lista = lista.OrderBy(_ => Rng.Next()).ToList();
		flujo.Pasos.AddRange(lista);
	}

	public Pregunta? SiguientePregunta(string dificultad, List<int> excluir) {
		// ojo, esto en teoria devuelve elementos aleatorios, probar
		// esta desgracia es necesaria por que EF no es flexible, asdjajsdajhjs
		// Dapper sql builder
		
		var builder = new SqlBuilder();
		builder.Select("id, nombre, dificultad, legitimo")
			.Where("dificultad = @d", new { d = dificultad })
			.OrderBy("random()");

		if (excluir.Count > 0) {
			var inParams = DbHelpers.InParameters(excluir);
			builder.Where($"id not in ({inParams})");
		}

		var tpl = builder.AddTemplate("select /**select**/ from pregunta /**where**/ /**orderby**/ limit 1");

		var res = _db.Database.GetDbConnection()
			.QueryFirst(tpl.RawSql, tpl.Parameters);

		return res == null
			? null
			: new Pregunta {
				Id = res.id,
				Dificultad = res.dificultad,
				Legitimo = res.legitimo,
				Nombre = res.nombre
			};
	}
}

public class SimpleSqlBuilder {
	private readonly MySqlBuilder _builder;
	private Dictionary<string, bool> _existsClause = new();

	public SimpleSqlBuilder() {
		_builder = new MySqlBuilder();
	}

	public class MySqlBuilder : SqlBuilder {
		public SqlBuilder From(string sql, dynamic parameters = null) =>
			AddClause("from", sql, parameters, " , ", "", "\n", false);
	}

	public SimpleSqlBuilder From(string exp, dynamic parameters = null) {
		_existsClause["from"] = true;
		_builder.From(exp, parameters);
		return this;
	}

	public SimpleSqlBuilder Select(string exp, dynamic parameters = null) {
		_existsClause["select"] = true;
		_builder.Select(exp, parameters);
		return this;
	}

	public SimpleSqlBuilder Where(string exp, dynamic parameters = null) {
		_existsClause["where"] = true;
		_builder.Where(exp, parameters);
		return this;
	}

	public SimpleSqlBuilder WhereIn<T>(string exp, ICollection<T> list) {
		_existsClause["where"] = true;
		var dic = new Dictionary<string, object>();
		var i = 0;
		foreach (var item in list) {
		}


		return this;
	}

	public SimpleSqlBuilder WhereNotIn<T>(string exp, ICollection<T> list) {
		return this;
	}

	public SimpleSqlBuilder OrderBy(string exp, dynamic parameters = null) {
		return this;
	}

	public SimpleSqlBuilder GroupBy(string exp, dynamic parameters = null) {
		return this;
	}

	public SimpleSqlBuilder Having(string exp, dynamic parameters = null) {
		return this;
	}

	public SimpleSqlBuilder InnerJoin(string exp, dynamic parameters = null) {
		return this;
	}

	public SimpleSqlBuilder LeftJoin(string exp, dynamic parameters = null) {
		return this;
	}

	public SimpleSqlBuilder RightJoin(string exp, dynamic parameters = null) {
		return this;
	}

	public SimpleSqlBuilder Join(string exp, dynamic parameters = null) {
		return this;
	}
}

public sealed class Numeric {
	/// <summary>
	/// Determines if a type is numeric.  Nullable numeric types are considered numeric.
	/// </summary>
	/// <remarks>
	/// Boolean is not considered numeric.
	/// </remarks>
	public static bool Is(Type type) {
		// from http://stackoverflow.com/a/5182747/172132
		switch (Type.GetTypeCode(type)) {
			case TypeCode.Byte:
			case TypeCode.Decimal:
			case TypeCode.Double:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.SByte:
			case TypeCode.Single:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
				return true;
			case TypeCode.Object:
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
					return Is(Nullable.GetUnderlyingType(type));
				}
				return false;
		}
		return false;
	}

	/// <summary>
	/// Determines if a type is numeric.  Nullable numeric types are considered numeric.
	/// </summary>
	/// <remarks>
	/// Boolean is not considered numeric.
	/// </remarks>
	public static bool Is<T>() {
		return Is(typeof(T));
	}
}