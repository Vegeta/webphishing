using Infraestructura;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Webapp.Models;

public class ConsultaEvaluacion {
	private readonly AppDbContext _db;

	public ConsultaEvaluacion(AppDbContext db) {
		_db = db;
	}

	public IEnumerable<dynamic> RespuestasWeb(int sessionId) {
		return _db.SesionRespuesta
			.Include(x => x.Pregunta)
			.Where(x => x.SesionId == sessionId)
			.OrderBy(x => x.Inicio)
			.Select(x => new {
				x.Pregunta.ImagenRetro,
				x.Pregunta.Dificultad,
				x.Pregunta.Explicacion,
				x.Pregunta.Email,
				adjuntos = JSON.Parse<dynamic>(x.Pregunta.Adjuntos ?? "[]"),
				x.Respuesta,
				tipo = x.Pregunta.Legitimo == 0 ? "PHISHING" : "LEGITIMO",
				x.Score,
				x.Tiempo,
				x.Comentario,
				x.PreguntaId
			})
			.ToList();
	}
}