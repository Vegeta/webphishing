using ClosedXML.Excel;
using Dapper;
using Domain.Entidades;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Reportes;

public class ExportarRespuestas {
	private readonly AppDbContext _db;

	public ExportarRespuestas(AppDbContext db) {
		_db = db;
	}

	public IList<dynamic> Consulta() {
		var sql = @"SELECT * FROM v_respuestas order by sesion_id desc";
		return _db.Database.GetDbConnection()
			.Query<dynamic>(sql)
			.ToList();
	}

	public XLWorkbook Exportar() {
		var wb = new XLWorkbook();
		var ws = wb.AddWorksheet("Respuestas");

		var lista = Consulta();

		var campos = new CamposExcel<dynamic>();
		campos
			.AddCampo("nombre pregunta", x => x.nombre)
			.AddCampo("legitimo", x => x.legitimo)
			.AddCampo("id", x => x.id)
			.AddCampo("sesion_id", x => x.sesion_id)
			.AddCampo("pregunta_id", x => x.pregunta_id)
			.AddCampo("respuesta", x => x.respuesta)
			.AddCampo("interacciones", x => x.interacciones)
			.AddCampo("tiempo", x => x.tiempo)
			.AddCampo("inicio", x => x.inicio)
			.AddCampo("fin", x => x.fin)
			.AddCampo("score", x => x.score)
			.AddCampo("comentario", x => x.comentario)
			.AddCampo("dificultad", x => x.dificultad)
			.AddCampo("hover_total", x => x.hover_total)
			.AddCampo("hover_avg", x => x.hover_avg)
			.AddCampo("clicks_total", x => x.clicks_total)
			.AddCampo("correcta", x => x.correcta)
			.AddCampo("errada", x => x.errada);

		ExcelUtils.LlenarHeader(ws, 1, campos.GetHeader());

		campos.Row = 2;
		foreach (var rec in lista) {
			campos.WriteRow(ws, rec);
		}

		return wb;
	}
}