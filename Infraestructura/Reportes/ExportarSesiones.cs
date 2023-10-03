using ClosedXML.Excel;
using Infraestructura.Persistencia;
using Domain.Entidades;

namespace Infraestructura.Reportes;

public class ExportarSesiones {

	public XLWorkbook Exportar(IQueryable<VSesionPersona> query) {
		var wb = new XLWorkbook();
		var ws = wb.AddWorksheet("Preguntas");

		var headers = new[] {
			"Fecha Examen",
			"Fecha Fin",
			"Estado",
			"Tipo Prueba",
			"Score",
			"Tiempo Total(s)",
			"Prom. Score",
			"Prom. Tiempo (s)",
			"% Exito",
			"Max. Score",
			"Percepcion Seguridad",
			"Tiempo Cuestionario (s)",
			"Score Cuestionario",
			"Apellidos",
			"Nombres",
			"Genero",
			"Edad",
			"Ocupación",
			"Actividad",
			"Años exp. seguridad",
			"Email",
			"idSesion",
			"idPersona"
		};

		ExcelUtils.LlenarHeader(ws, 1, headers);

		var row = 2;
		foreach (var rec in query) {
			var col = 0;
			ws.Cell(row, ++col).Value = rec.FechaExamen;
			ws.Cell(row, ++col).Value = rec.FechaFin;
			ws.Cell(row, ++col).Value = ExcelUtils.UpperNoUnder(rec.Estado);
			ws.Cell(row, ++col).Value = rec.Tipo;
			ws.Cell(row, ++col).Value = rec.Score;
			ws.Cell(row, ++col).Value = rec.TiempoTotal;
			ws.Cell(row, ++col).Value = rec.AvgScore;
			ws.Cell(row, ++col).Value = rec.AvgTiempo;
			ws.Cell(row, ++col).Value = rec.Exito;
			ws.Cell(row, ++col).Value = rec.MaxScore;
			ws.Cell(row, ++col).Value = rec.Percepcion;
			ws.Cell(row, ++col).Value = rec.TiempoCuestionario;
			ws.Cell(row, ++col).Value = rec.ScoreCuestionario;
			ws.Cell(row, ++col).Value = rec.Apellidos;
			ws.Cell(row, ++col).Value = rec.Nombres;
			ws.Cell(row, ++col).Value = rec.Genero;
			ws.Cell(row, ++col).Value = rec.Edad;
			ws.Cell(row, ++col).Value = rec.Ocupacion;
			ws.Cell(row, ++col).Value = rec.Actividad;
			ws.Cell(row, ++col).Value = rec.ExperienciaSeguridad;
			ws.Cell(row, ++col).Value = rec.Email;
			ws.Cell(row, ++col).Value = rec.Id;
			ws.Cell(row, ++col).Value = rec.PersonaId;
			row++;
		}

		return wb;
	}


}
