using Domain;
using Domain.Entidades;
using Infraestructura;
using Infraestructura.Persistencia;
using Infraestructura.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.RateLimiting;
using Dapper;
using Infraestructura.Examenes;
using Infraestructura.Examenes.Asignacion;

namespace Probador;

public class Program {
	private static void Main(string[] args) {
		// Host.CreateDefaultBuilder(args)
		// 	.ConfigureServices((hostContext, services) => {
		// 		var config = hostContext.Configuration;
		// 		services.AddDatabase(config);
		// 		services.AddServices(config);
		// 		services.AddHostedService<ConsoleService>();
		// 	}).RunConsoleAsync();

		var json = "{\"que\":\"\"}";
		var o = JSON.Parse<What>(json);

		Console.WriteLine(ObjectDumper.Dump(o));

		o.Que = 4;

		Console.WriteLine(JSON.Stringify(o));
	}
}

public class What {
	public int? Que { get; set; }
}

public class ConsoleService : IHostedService {
	private IServiceProvider services;

	public ConsoleService(IServiceProvider services) {
		this.services = services;
	}

	public Task StartAsync(CancellationToken cancellationToken) {
		using var scope = services.CreateScope();
		var db = services.GetRequiredService<AppDbContext>();

		var f = new PruebaFLujo2();
		f.PruebaAleatorio(db);

		// var con = new Consultas();
		// con.PruebaFluent(db);

		// var ss = new SimpleQueryBuilder();
		// ss.From("usuario u")
		// 	.WhereIn("id", new [] { 1, 2, 3 })
		// 	.Limit(1);
		// Console.WriteLine(ObjectDumper.Dump(ss.Parameters));
		// Console.WriteLine(ss.Sql);
		// var u = db.Database.GetDbConnection()
		// 	.QueryFirst(ss.Sql, ss.Parameters);
		// Console.WriteLine(ObjectDumper.Dump(u));

		// var al = new AsignadorAleatorio(db);
		// var preg = al.SiguientePregunta(DificultadPregunta.Facil, new());
		// Dump(preg);

		return Task.CompletedTask;
	}

	public void Dump(object? obj) {
		var s = ObjectDumper.Dump(obj, new DumpOptions { MaxLevel = 3, DumpStyle = DumpStyle.CSharp });
		Console.WriteLine(s);
	}

	void TestPermisos() {
		var permisos = new List<string> { "preguntas", "chorro" };
		var check = "preguntas|examenes, chorro";

		var vale = UsuariosService.AutorizarPermisos(permisos, check);
		Console.WriteLine(vale);
	}

	public Task StopAsync(CancellationToken cancellationToken) {
		return Task.CompletedTask;
	}
}