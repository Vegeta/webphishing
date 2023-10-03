using Infraestructura.Persistencia;
using Domain.Entidades;
using Infraestructura.Filtros;

namespace Infraestructura.Servicios;

public class EvaluacionesService {
	private readonly AppDbContext _db;

	public EvaluacionesService(AppDbContext db) {
		_db = db;
	}

	public IQueryable<VSesionPersona> Sesiones(FiltroEvaluacion f) {
		IQueryable<VSesionPersona> q = _db.VSesiones;

		if (!string.IsNullOrEmpty(f.Email))
			q = q.Where(x => x.Email.ToUpper().Contains(f.Email.ToUpper()));
		if (!string.IsNullOrEmpty(f.Nombres))
			q = q.Where(x => x.Nombres.ToUpper().Contains(f.Nombres.ToUpper()));
		if (!string.IsNullOrEmpty(f.Apellidos))
			q = q.Where(x => x.Apellidos.ToUpper().Contains(f.Apellidos.ToUpper()));

		// nombre completo
		if (!string.IsNullOrEmpty(f.Nombre))
			q = q.Where(x => x.Nombre.ToUpperInvariant().Contains(f.Nombre.ToUpper()));

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
			q = q.Where(x => f.Actividad.Contains(x.Actividad));

		if (!string.IsNullOrEmpty(f.Estado))
			q = q.Where(x => x.Estado == f.Estado);
		if (!string.IsNullOrEmpty(f.Ocupacion))
			q = q.Where(x => x.Ocupacion == f.Ocupacion);
		if (!string.IsNullOrEmpty(f.Genero))
			q = q.Where(x => x.Ocupacion == f.Genero);

		var ordenador = new SortExpressionHelper<VSesionPersona>()
			.Add("email", x => x.Email)
			.Add("nombres", x => x.Apellidos, x => x.Nombres)
			.Add("nombreCompleto", x => x.Apellidos, x => x.Nombres)
			.Add("fechaExamen", x => x.FechaExamen)
			.Add("exito", x => x.Exito)
			.Add("tiempo", x => x.TiempoTotal)
			.Add("avgTiempo", x => x.AvgTiempo)
			.Add("avgScore", x => x.AvgScore)
			.Add("score", x => x.Score);

		var order = f.OrdenCampo;
		if (!string.IsNullOrEmpty(f.OrdenCampo)) {
			q = ordenador.SetOrder(q, f.OrdenCampo, f.OrdenDir);
		}

		return q;
	}
}