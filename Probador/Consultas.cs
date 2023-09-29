using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Dapper.SimpleSqlBuilder;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Infraestructura;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Probador;

public class Consultas {
	public void PruebaDapperBuilder(AppDbContext db) {
		var id = "facil";

		var builder = new SqlBuilder();
		var tpl = builder.AddTemplate(@"select r.pregunta_id, r.nombre, r.dificultad, count(*), sum(r.correcta) as correctas,
		sum(r.correcta)::numeric / count(*)::numeric as diff
		from v_respuestas r
		join v_sesion_persona s on s.id = r.sesion_id 
		/**where**/
		GROUP BY r.pregunta_id, r.nombre, r.dificultad");

		builder.Where($"r.dificultad = @dif", new { dif = "facil" });

		Console.WriteLine(tpl.RawSql);
		Console.WriteLine(ObjectDumper.Dump(tpl.Parameters, new DumpOptions { MaxLevel = 3, DumpStyle = DumpStyle.CSharp }));

		var lista = db.Database.GetDbConnection()
			.Query<dynamic>(tpl.RawSql, tpl.Parameters)
			.ToList();

		var o = lista.OrderByDescending(x => x.diff);

		foreach (var row in lista) {
			Console.WriteLine(JSON.Stringify(row, true));
		}
	}

	public string inParameters<T>(List<T> lista) {
		Type tipo = lista.GetType().GetGenericArguments().Single();
		if (tipo == typeof(string))
			return string.Join(",", lista.Select(x => $"'{x}'"));
		return string.Join(",", lista.Select(x => x.ToString()));
	}

	public void PruebaFluent(AppDbContext db) {
		var dif = "dificil";
		var excluir = new List<int> { 1, 2, 3 };
		// Type myListElementType = myList.GetType().GetGenericArguments().Single();
		var inParams = inParameters(excluir);

		var builder = new SqlBuilder();
		builder.Select("id, nombre")
			.Where("dificultad = @d", new { d = dif })
			.OrderBy("random()");

		builder.Where($"id not in ({inParams})");

		var tpl = builder.AddTemplate("select /**select**/ from pregunta /**where**/ /**orderby**/ limit 1");
		
		
		var res = db.Database.GetDbConnection()
			.QueryFirst(tpl.RawSql, tpl.Parameters);

		Console.WriteLine(ObjectDumper.Dump(res));
	}
}