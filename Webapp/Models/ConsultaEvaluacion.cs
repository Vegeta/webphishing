using System.Collections.Generic;
using System.Linq;
using Domain.Transferencia;
using Domain;
using Domain.Entidades;
using Infraestructura;
using Infraestructura.Examenes;
using Infraestructura.Persistencia;
using Infraestructura.Reportes;
using Microsoft.EntityFrameworkCore;

namespace Webapp.Models;

public class ConsultaEvaluacion {
	private readonly AppDbContext _db;

	public ConsultaEvaluacion(AppDbContext db) {
		_db = db;
	}

	public IEnumerable<dynamic> RespuestasWeb(int sessionId) {
		var lista = _db.SesionRespuesta
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
				x.PreguntaId,
				interacciones = InteraccionesDto.Parse(x.Interacciones ?? "[]")
			})
			.ToList();
		return lista;
	}

	public object? PreguntaData(int id) {
		return _db.Pregunta
			.Where(x => x.Id == id)
			.Select(vm => new {
				vm.Id,
				vm.Subject,
				vm.Email,
				vm.Html,
				vm.Sender,
				adjuntos = JSON.Parse<List<AdjuntoView>>(vm.Adjuntos ?? "[]"),
			})
			.FirstOrDefault();
	}

	public int? IdPrimercuestionario() {
		return _db.Cuestionario.Select(x => x.Id).FirstOrDefault();
	}

	public object? CuestionarioData(int? id = null) {
		IEnumerable<Cuestionario> q = _db.Cuestionario.AsNoTracking();
		if (id.HasValue)
			q = q.Where(x => x.Id == id);

		var opciones = OpcionesConfig.ComboDict(RespuestaCuestionario.Mapa());

		var cues = q.FirstOrDefault();
		if (cues == null)
			return new {
				opciones
			};

		return new {
			preguntas = JSON.Parse<List<CuestRespuestaModel>>(cues.Preguntas ?? "[]"),
			cues.Titulo,
			cues.Instrucciones,
			opciones
		};
	}
}