using Dapper;
using Infraestructura.Persistencia;
using Domain.Entidades;
using Infraestructura.Filtros;
using Infraestructura.Logging;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Servicios;

public class EvaluacionesService {
	private readonly AppDbContext _db;
	private readonly IAuditor<EvaluacionesService> _logger;

	public EvaluacionesService(AppDbContext db, IAuditor<EvaluacionesService> logger) {
		_db = db;
		_logger = logger;
	}

	public IQueryable<VSesionPersona> Sesiones(FiltroEvaluacion f) {
		IQueryable<VSesionPersona> q = _db.VSesiones;

		if (!string.IsNullOrEmpty(f.Email))
			q = q.Where(x => x.Email!.ToUpper().Contains(f.Email.ToUpper()));
		if (!string.IsNullOrEmpty(f.Nombres))
			q = q.Where(x => x.Nombres!.ToUpper().Contains(f.Nombres.ToUpper()));
		if (!string.IsNullOrEmpty(f.Apellidos))
			q = q.Where(x => x.Apellidos!.ToUpper().Contains(f.Apellidos.ToUpper()));

		// nombre completo
		if (!string.IsNullOrEmpty(f.Nombre))
			q = q.Where(x => x.Nombre!.ToUpper().Contains(f.Nombre.ToUpper()));

		if (f.MinExito.HasValue)
			q = q.Where(x => x.Exito >= f.MinExito);
		if (f.MaxExito.HasValue)
			q = q.Where(x => x.Exito <= f.MaxExito);

		if (f.TomadoDesde.HasValue)
			q = q.Where(x => x.FechaExamen >= f.TomadoDesde);
		if (f.TomadoHasta.HasValue)
			q = q.Where(x => x.FechaFin >= f.TomadoHasta);
		if (f.Mes.HasValue)
			q = q.Where(x => x.FechaExamen.HasValue && x.FechaExamen.Value.Month == f.Mes);
		if (f.Anio.HasValue)
			q = q.Where(x => x.FechaExamen.HasValue && x.FechaExamen.Value.Year == f.Anio);

		if (f.Actividad.Any())
			q = q.Where(x => f.Actividad.Contains(x.Actividad!));

		if (!string.IsNullOrEmpty(f.Estado))
			q = q.Where(x => x.Estado == f.Estado);
		if (!string.IsNullOrEmpty(f.Ocupacion))
			q = q.Where(x => x.Ocupacion == f.Ocupacion);
		if (!string.IsNullOrEmpty(f.Genero))
			q = q.Where(x => x.Ocupacion == f.Genero);

		var ordenador = new SortExpressionHelper<VSesionPersona>()
			.Add("email", x => x.Email!)
			.Add("nombres", x => x.Apellidos!, x => x.Nombres!)
			.Add("nombreCompleto", x => x.Apellidos!, x => x.Nombres!)
			.Add("fechaExamen", x => x.FechaExamen!)
			.Add("exito", x => x.Exito!)
			.Add("tiempo", x => x.TiempoTotal!)
			.Add("avgTiempo", x => x.AvgTiempo!)
			.Add("avgScore", x => x.AvgScore!)
			.Add("estado", x => x.Estado!)
			.Add("percepcion", x => x.Percepcion!)
			.Add("score", x => x.Score!);

		var order = f.OrdenCampo;
		if (!string.IsNullOrEmpty(f.OrdenCampo)) {
			q = ordenador.SetOrder(q, f.OrdenCampo, f.OrdenDir);
		}

		return q;
	}

	public IQueryable<Persona> Personas(FiltroEvaluacion f) {
		IQueryable<Persona> q = _db.Persona;

		if (!string.IsNullOrEmpty(f.Email))
			q = q.Where(x => x.Email!.ToUpper().Contains(f.Email.ToUpper()));
		if (!string.IsNullOrEmpty(f.Nombres))
			q = q.Where(x => x.Nombre!.ToUpper().Contains(f.Nombres.ToUpper()));
		if (!string.IsNullOrEmpty(f.Apellidos))
			q = q.Where(x => x.Apellido!.ToUpper().Contains(f.Apellidos.ToUpper()));

		if (!string.IsNullOrEmpty(f.Nombre)) {
			var like = f.Nombre.ToUpper();
			q = q.Where(x => x.Apellido!.ToUpper().Contains(like) || x.Nombre!.ToUpper().Contains(like));
		}
		if (f.Actividad.Any())
			q = q.Where(x => f.Actividad.Contains(x.Actividad!));

		if (!string.IsNullOrEmpty(f.Ocupacion))
			q = q.Where(x => x.Ocupacion == f.Ocupacion);
		if (!string.IsNullOrEmpty(f.Genero))
			q = q.Where(x => x.Ocupacion == f.Genero);

		if (f.EdadMin.HasValue)
			q = q.Where(x => x.Edad >= f.EdadMin);

		if (f.EdadMax.HasValue)
			q = q.Where(x => x.Edad <= f.EdadMax);

		var ordenador = new SortExpressionHelper<Persona>()
			.Add("email", x => x.Email!)
			.Add("ocupacion", x => x.Ocupacion!)
			.Add("actividad", x => x.Actividad!)
			.Add("edad", x => x.Edad!)
			.Add("experiencia", x => x.ExperienciaSeguridad!)
			.Add("nombreCompleto", x => x.Apellido!, x => x.Nombre!)
			.Add("creacion", x => x.Creacion!)
			.Add("genero", x => x.Genero!);

		var order = f.OrdenCampo;
		if (!string.IsNullOrEmpty(f.OrdenCampo)) {
			q = ordenador.SetOrder(q, f.OrdenCampo, f.OrdenDir);
		}

		return q;
	}

	public dynamic ActividadPersona(int id) {
		var sql = @"select estado, count(*) num, max(fecha_examen) fexamen, max(fecha_actividad) factividad
			from sesion_persona
			where persona_id = @id
			group by persona_id, estado
			order by estado;";

		var lista = _db.Database.GetDbConnection()
			.Query<dynamic>(sql, new { id });

		return lista;
	}

	public OperationResult<string> EliminarEvaluacion(int id) {
		var eval = _db.SesionPersona
			.Include(x => x.CuestionarioRespuestas)
			.Include(x => x.SesionRespuesta)
			.FirstOrDefault(x => x.Id == id);

		if (eval == null)
			return OperationResult<string>.Problem("Evaluacion no encontrada");

		using var tx = _db.Database.BeginTransaction();
		try {
			_db.SesionPersona.Remove(eval);
			_db.SaveChanges();
			tx.Commit();
			var t = $"Evaluación de {eval.Nombre} eliminada";
			var tlog = $"{t}, id:{eval.Id}";
			_logger.Info(tlog, new { eval.Id });
			return OperationResult<string>.Ok(t);
		} catch (Exception ex) {
			tx.Rollback();
			_logger.Error($"Eliminando {eval.Id}", ex);
			return OperationResult<string>.Problem("Error eliminando evaluacion");
		}
	}
}