using ClosedXML.Excel;
using Domain.Entidades;

namespace Infraestructura.Reportes;

public class ExportarPersonas {
	public XLWorkbook Exportar(IQueryable<Persona> query) {
		var wb = new XLWorkbook();
		var ws = wb.AddWorksheet("Personas registradas");

		var campos = new CamposExcel<Persona>();
		campos
			.AddCampo("Apellidos", x => x.Apellido!)
			.AddCampo("Nombres", x => x.Nombre!)
			.AddCampo("Genero", x => x.Genero!)
			.AddCampo("Edad", x => x.Edad!)
			.AddCampo("Ocupación", x => x.Ocupacion!)
			.AddCampo("Actividad", x => x.Actividad!)
			.AddCampo("Años exp. seguridad", x => x.ExperienciaSeguridad!)
			.AddCampo("Email", x => x.Email!)
			.AddCampo("Fecha Registro", x => x.Creacion!)
			.AddCampo("idPersona", x => x.Id);

		ExcelUtils.LlenarHeader(ws, 1, campos.GetHeader());

		campos.Row = 2;
		foreach (var rec in query) {
			campos.WriteRow(ws, rec);
		}


		return wb;
	}
}