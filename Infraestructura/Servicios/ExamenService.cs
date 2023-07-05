using Domain.Entidades;
using Domain.Transferencia;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Servicios;

public class ExamenService {

	AppDbContext _db;

	public ExamenService(AppDbContext db) {
		_db = db;
	}

	public OperationResult<Examen> GuardarExamen(ExamenModel model, IList<int> deleted) {
		var e = model.Id.HasValue ? _db.Examen.Find(model.Id.Value) : new Examen();
		if (e == null)
			return OperationResult<Examen>.Problem("No se encuentra el examen");
		var hoy = DateTime.Now;

		e.Tipo = model.Tipo;
		e.Titulo = model.Titulo;
		e.Modificacion = hoy;
		if (e.Id == 0)
			e.Creacion = hoy;
		e.Activo = model.Activo;

		using var tx = _db.Database.BeginTransaction();
		try {
			if (e.Id == 0)
				_db.Examen.Add(e);
			_db.SaveChanges();

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

			if (deleted.Count > 0) {
				_db.ExamenPregunta.Where(x => deleted.Contains(x.Id)).ExecuteDelete();
			}

			tx.Commit();
		} catch (Exception ex) {
			tx.Rollback();
			// log ex
			return OperationResult<Examen>.Problem("Error de sistema " + ex.Message + ex.StackTrace);
		}
		return OperationResult<Examen>.Ok("Examen actualizado", e);
	}

}