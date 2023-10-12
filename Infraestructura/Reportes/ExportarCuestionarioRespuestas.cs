using ClosedXML.Excel;
using Dapper;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Reportes;

public class ExportarCuestionarioRespuestas {
	private readonly AppDbContext _db;

	public ExportarCuestionarioRespuestas(AppDbContext db) {
		_db = db;
	}

	public IList<dynamic> Consulta() {
		var sql = @"SELECT * FROM cuestionario_respuesta order by sesion_id desc";
		return _db.Database.GetDbConnection()
			.Query<dynamic>(sql)
			.ToList();
	}

	public XLWorkbook Exportar() {
		var wb = new XLWorkbook();
		var ws = wb.AddWorksheet("Respuestas Cuest.");

		var lista = Consulta();

		var campos = new CamposExcel<dynamic>();
		campos
			.AddCampo("id", x => x.id)
			.AddCampo("sesion_id", x => x.sesion_id)
			.AddCampo("pregunta", x => x.pregunta)
			.AddCampo("respuesta", x => x.respuesta)
			.AddCampo("puntaje", x => x.puntaje)
			.AddCampo("dimension", x => x.dimension);

		ExcelUtils.LlenarHeader(ws, 1, campos.GetHeader());

		campos.Row = 2;
		foreach (var rec in lista) {
			campos.WriteRow(ws, rec);
		}

		return wb;
	}
}