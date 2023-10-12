using ClosedXML.Excel;
using Domain.Entidades;

namespace Infraestructura.Reportes;

public class ExportarSesiones {
	public XLWorkbook Exportar(IQueryable<VSesionPersona> query) {
		var wb = new XLWorkbook();
		var ws = wb.AddWorksheet("Preguntas");

		var campos = new CamposExcel<VSesionPersona>();
		campos
			.AddCampo("Fecha Examen", x => x.FechaExamen!)
			.AddCampo("Fecha Fin", x => x.FechaFin!)
			.AddCampo("Estado", x => ExcelUtils.UpperNoUnder(x.Estado)!)
			.AddCampo("Tipo Prueba", x => x.Tipo!)
			.AddCampo("Score", x => x.Score!)
			.AddCampo("Tiempo Total(s)", x => x.TiempoTotal!)
			.AddCampo("Prom. Score", x => x.AvgScore!)
			.AddCampo("Prom. Tiempo (s)", x => x.AvgTiempo!)
			.AddCampo("% Exito", x => x.Exito!)
			.AddCampo("Max. Score", x => x.MaxScore!)
			.AddCampo("Percepcion Seguridad", x => x.Percepcion!)
			.AddCampo("Tiempo Cuestionario (s)", x => x.TiempoCuestionario!)
			.AddCampo("Score Cuestionario", x => x.ScoreCuestionario!)
			.AddCampo("Apellidos", x => x.Apellidos!)
			.AddCampo("Nombres", x => x.Nombres!)
			.AddCampo("Genero", x => x.Genero!)
			.AddCampo("Edad", x => x.Edad!)
			.AddCampo("Ocupación", x => x.Ocupacion!)
			.AddCampo("Actividad", x => x.Actividad!)
			.AddCampo("Años exp. seguridad", x => x.ExperienciaSeguridad!)
			.AddCampo("Email", x => x.Email!)
			.AddCampo("idSesion", x => x.Id)
			.AddCampo("idPersona", x => x.PersonaId!);

		ExcelUtils.LlenarHeader(ws, 1, campos.GetHeader());

		campos.Row = 2;
		foreach (var rec in query) {
			campos.WriteRow(ws, rec);
		}

		return wb;
	}
}