using Domain;
using Domain.Entidades;
using Infraestructura.Servicios;

namespace Infraestructura.Examenes;

public class EvaluadorCuestionario {
	public DataCuestionario EvaluarCuestionario(IList<CuestionarioRespuesta> respuestas, SesionPersona? sesion = null) {
		var calif = RespuestaCuestionario.Calificacion();

		foreach (var item in respuestas) {
			var resp = item.Respuesta ?? "";
			if (calif.TryGetValue(resp, out var value))
				item.Puntaje = value;
		}

		var pordim = new Dictionary<string, StatsDimension>();
		// agrupar por dimensiones, sumar cada una y promedio de las sumas
		var grupos = respuestas.GroupBy(x => x.Dimension ?? "");
		foreach (var grupo in grupos) {
			var t = new StatsDimension {
				Dimension = grupo.Key,
				Suma = grupo.Select(x => x.Puntaje).Sum(),
				Prom = grupo.Select(x => x.Puntaje).Average()
			};
			pordim[grupo.Key] = t;
		}
		var avg = pordim.Values.Select(x => x.Suma).Average();
		var data = new DataCuestionario();
		data.ScoreCuestionario = (float?)avg;
		data.Percepcion = RespuestaCuestionario.Percepcion(Convert.ToDecimal(avg));
		data.RespuestaCuestionario = JSON.Stringify(pordim.Values);

		if (sesion != null) {
			sesion.ScoreCuestionario = data.ScoreCuestionario;
			sesion.Percepcion = data.Percepcion;
			sesion.RespuestaCuestionario = data.RespuestaCuestionario;
			sesion.Estado = SesionPersona.EstadoEnCurso;
		}
		return data;
	}
}