using System;
using System.Collections.Generic;
using System.Linq;
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

	public void Consultar(AppDbContext db) {

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


		// var bb = SimpleBuilder.CreateFluent()
		// 	.Select(
		// 		$"r.pregunta_id, r.nombre, r.dificultad, count(*), sum(r.correcta) as correctas,	sum(r.correcta)::numeric / count(*)::numeric as diff")
		// 	.From($"v_respuestas r join v_sesion_persona s on s.id = r.sesion_id")
		// 	.Where($"s.nombre = {id}")
		// 	.GroupBy($"r.pregunta_id, r.nombre, r.dificultad");
		//
		//
		// Console.WriteLine(bb.Sql);
		// Console.WriteLine(ObjectDumper.Dump(bb.Parameters, new DumpOptions { MaxLevel = 3, DumpStyle = DumpStyle.CSharp }));




	}

}