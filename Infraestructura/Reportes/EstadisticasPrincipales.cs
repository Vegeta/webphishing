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
		var numPersonas =  conn.QuerySingle<int>("select count(*) num from persona");

		// 10 ultimas evaluaciones
		var sql1 = @"select id, fecha_examen, fecha_actividad, estado, nombre, tipo 
		from sesion_persona order by fecha_examen desc limit 10";

		var ultimas10 = conn.Query<dynamic>(sql1);

		// preguntas mas dificiles, esto seria de hacerla una consulta mas chevere en algun otro lado
		var sql2 = @"select pregunta_id, p.nombre, p.dificultad, p.legitimo, count(*) as num
		from sesion_respuesta r
		join pregunta p on p.id = r.pregunta_id
		where score = 0
		group by pregunta_id, p.nombre, p.dificultad, p.legitimo
		order by count(*) desc";

		var masDifiles = conn.Query<dynamic>(sql2);

		return new {
			numSesiones,
			numPersonas,
			ultimas10,
			masDifiles
		};
	}

	public IQueryable<SesionRespuesta> PreguntasDificiles(FiltroEvaluacion? filtros = null) {
		filtros ??= new FiltroEvaluacion();

		var q = _db.SesionRespuesta
			.Include(x => x.Pregunta)
			.Include(x => x.Sesion)
			.Include(x => x.Sesion.Persona)
			.Where(x => x.Score == 0);

		if (!string.IsNullOrEmpty(filtros.Genero))
			q = q.Where(x => x.Sesion.Persona.Genero == filtros.Genero);

		if (!string.IsNullOrEmpty(filtros.Estado))
			q = q.Where(x => x.Sesion.Estado == filtros.Estado);

		if (!string.IsNullOrEmpty(filtros.Ocupacion))
			q = q.Where(x => x.Sesion.Persona.Ocupacion == filtros.Ocupacion);

		if (filtros.Actividad.Any())
			q = q.Where(x => filtros.Actividad.Contains(x.Sesion.Persona.Actividad));

		if (!string.IsNullOrEmpty(filtros.Nombre) && filtros.Nombre.Length > 2)
			q = q.Where(x => x.Sesion.Persona.Nombre.ToUpper().Contains(filtros.Nombre.ToUpper()));

		if (filtros.Anio.HasValue)
			q = q.Where(x => x.Sesion.FechaExamen.Value.Year == filtros.Anio);

		if (filtros.Mes.HasValue)
			q = q.Where(x => x.Sesion.FechaExamen.Value.Month == filtros.Mes);

		q.GroupBy(x => new {
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
		return q;
	}


}