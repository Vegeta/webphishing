using Domain.Entidades;
using Domain.Transferencia;
using Infraestructura.Logging;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Servicios;

public class ExamenService {
	private readonly AppDbContext _db;
	private readonly IAuditor<ExamenService> _logger;

	public ExamenService(AppDbContext db, IAuditor<ExamenService> logger) {
		_db = db;
		_logger = logger;
	}

	public OperationResult<Examen> GuardarExamen(ExamenModel model, IList<int> deleted) {
		var e = model.Id.HasValue ? _db.Examen.Find(model.Id.Value) : new Examen();
		if (e == null)
			return OperationResult<Examen>.Problem("No se encuentra el examen");
		var hoy = DateTime.Now;

		e.Tipo = model.Tipo;
		e.Titulo = model.Titulo;
		e.Modificacion = hoy;
		e.CuestionarioPos = model.CuestionarioPos;
		if (e.Id == 0)
			e.Creacion = hoy;
		e.Activo = model.Activo;

		using var tx = _db.Database.BeginTransaction();
		try {
			if (e.Id == 0)
				_db.Examen.Add(e);
			_db.SaveChanges();

			// barrarse las preguntas y ver si es nueva o se ha cambiado
			foreach (var item in model.Preguntas) {
				var rec = new ExamenPregunta {
					ExamenId = e.Id,
					PreguntaId = item.PreguntaId,
					Orden = item.Orden,
				};
				if (item.Id.HasValue) {
					rec.Id = item.Id.Value;
					_db.Update(rec);
				} else {
					_db.ExamenPregunta.Add(rec);
				}
			}
			_db.SaveChanges();

			// si hay cosas que eliminar, pues aqui esta
			if (deleted.Count > 0) {
				_db.ExamenPregunta.Where(x => deleted.Contains(x.Id)).ExecuteDelete();
			}

			tx.Commit();
			_logger.Info($"Examen actualizado: {e.Titulo}", new { e.Id, e.Tipo, e.Titulo });
		} catch (Exception ex) {
			tx.Rollback();
			_logger.Error("Guardar examen", ex);
			return OperationResult<Examen>.Problem("Error de sistema");
		}
		return OperationResult<Examen>.Ok("Examen actualizado", e);
	}
}