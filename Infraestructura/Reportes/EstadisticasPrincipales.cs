using Dapper;
using Domain.Entidades;
using Infraestructura.Filtros;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Reportes;

public class EstadisticasPrincipales {
	private readonly AppDbContext _db;

	public EstadisticasPrincipales(AppDbContext db) {
		_db = db;
	}

	public void TopDificiiles() {
		// _db.Database.GetDbConnection().QueryAsync()
	}

	public void EstadisticasExamenes() { }

	public dynamic HomeScreenStats() {
		var conn = _db.Database.GetDbConnection();
		var numSesiones = conn.QuerySingle<int>("select count(*) num from sesion_persona");
		var numPersonas = conn.QuerySingle<int>("select count(*) num from persona");
		var numPreguntas = conn.QuerySingle<int>("select count(*) num from pregunta");

		// 10 ultimas evaluaciones
		var sql1 = @"select id, fecha_examen, fecha_actividad, estado, nombre, tipo 
		from sesion_persona order by fecha_examen desc limit 10";

		var ultimas10 = conn.Query<dynamic>(sql1);

		// preguntas mas dificiles, esto seria de hacerla una consulta mas chevere en algun otro lado
		
		// var sql2 = @"select pregunta_id, p.nombre, p.dificultad, p.legitimo, count(*) as num
		// from sesion_respuesta r
		// join pregunta p on p.id = r.pregunta_id
		// where score = 0
		// group by pregunta_id, p.nombre, p.dificultad, p.legitimo
		// order by count(*) desc";

		//var masDificiles = conn.Query<dynamic>(sql2);
		// var masDificiles = PreguntasDificiles();

		var exitoPreguntas = ExitoPreguntas()
			.OrderByDescending(x => x.Erradas);


		return new {
			numSesiones,
			numPersonas,
			numPreguntas,
			ultimas10,
			exitoPreguntas
		};
	}

	public IEnumerable<dynamic> PreguntasDificiles(FiltroEvaluacion? filtros = null) {
		filtros ??= new FiltroEvaluacion();

		var q = _db.SesionRespuesta
			.Include(x => x.Pregunta)
			.Include(x => x.Sesion)
			.Include(x => x.Sesion.Persona)
			.Where(x => x.Score == 0);

		if (!string.IsNullOrEmpty(filtros.Genero))
			q = q.Where(x => x.Sesion.Persona!.Genero == filtros.Genero);

		if (!string.IsNullOrEmpty(filtros.Estado))
			q = q.Where(x => x.Sesion.Estado == filtros.Estado);

		if (!string.IsNullOrEmpty(filtros.Ocupacion))
			q = q.Where(x => x.Sesion.Persona!.Ocupacion == filtros.Ocupacion);

		if (filtros.Actividad.Any())
			q = q.Where(x => filtros.Actividad.Contains(x.Sesion.Persona!.Actividad!));

		if (!string.IsNullOrEmpty(filtros.Nombre) && filtros.Nombre.Length > 2)
			q = q.Where(x => x.Sesion.Persona!.Nombre!.ToUpper().Contains(filtros.Nombre.ToUpper()));

		if (filtros.Anio.HasValue)
			q = q.Where(x => x.Sesion.FechaExamen!.Value.Year == filtros.Anio);

		if (filtros.Mes.HasValue)
			q = q.Where(x => x.Sesion.FechaExamen!.Value.Month == filtros.Mes);

		return q.GroupBy(x => new {
			x.PreguntaId,
			x.Pregunta.Nombre,
			x.Dificultad,
			x.Pregunta.Legitimo
		})
			.Select(g => new {
				num = g.Count(),
				g.Key.PreguntaId,
				g.Key.Nombre,
				g.Key.Dificultad,
				g.Key.Legitimo,
			})
			.OrderByDescending(x => x.num);
	}

	public List<FilaPregunta> ExitoPreguntas(FiltroEvaluacion? filtros = null) {
		filtros ??= new FiltroEvaluacion();

		var q = new SimpleQueryBuilder();
		// select r.pregunta_id, r.nombre, r.dificultad, r.legitimo,  sum(r.correcta) as good, sum(r.errada) as bad
		// from v_respuestas r
		// 	join sesion_persona s on s.id = r.sesion_id
		// join persona p on p.id = s.persona_id
		// group by r.pregunta_id, r.nombre, r.dificultad, r.legitimo

		q.From(@"v_respuestas r 
			join v_sesion_persona s on s.id = r.sesion_id")
			.Select("r.pregunta_id, r.nombre, r.dificultad, r.legitimo,  sum(r.correcta) as good, sum(r.errada) as bad")
			.GroupBy("r.pregunta_id, r.nombre, r.dificultad, r.legitimo");

		if (!string.IsNullOrEmpty(filtros.Genero))
			q.Where("s.genero", filtros.Genero);

		if (!string.IsNullOrEmpty(filtros.Estado))
			q.Where("s.estado", filtros.Estado);

		if (!string.IsNullOrEmpty(filtros.Ocupacion))
			q.Where("s.ocupacion", filtros.Ocupacion);

		if (filtros.Actividad.Any())
			q.WhereIn("s.actividad", filtros.Actividad);

		if (!string.IsNullOrEmpty(filtros.Nombre) && filtros.Nombre.Length > 2) {
			var like = "%" + filtros.Nombre.ToUpper() + "%";
			q.Where("s.nombre like @nombre", new { nombre = like });
		}

		if (filtros.Anio.HasValue)
			q.Where(" s.anio", filtros.Anio);

		if (filtros.Mes.HasValue)
			q.Where(" s.mes", filtros.Mes);


		if (!string.IsNullOrEmpty(filtros.Dificultad))
			q.Where("r.dificultad", filtros.Dificultad);

		if (!string.IsNullOrEmpty(filtros.Clase)) {
			q.Where(filtros.Clase.ToLower() == "phishing" ? "r.legitimo = 1" : "r.legitimo = 0");
		}

		var conn = _db.Database.GetDbConnection();
		var lista = conn.Query<dynamic>(q.Sql, q.Parameters);

		var res = lista.Select(x => {
			var f = new FilaPregunta {
				Id = x.pregunta_id,
				Nombre = x.nombre,
				Dificultad = x.dificultad,
				Clase = x.legitimo == 0 ? "Phishing" : "Legitimo",
				Correctas = (int)x.good,
				Erradas = (int)x.bad,
			};
			f.Total = f.Correctas + f.Erradas;
			var por = (decimal)f.Correctas / f.Total;
			f.PorExito = Math.Round(por, 2) * 100; // sin decimales
			return f;
		});

		return res.ToList();
	}


}

public class FilaPregunta {
	public int Id { get; set; }
	public string Nombre { get; set; } = "";
	public string Dificultad { get; set; } = "";
	public string Clase { get; set; } = "";
	public int Total { get; set; }
	public int Correctas { get; set; }
	public int Erradas { get; set; }
	public decimal PorExito { get; set; }

}