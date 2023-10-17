using ClosedXML.Excel;
using Domain.Entidades;
using Infraestructura.Persistencia;

namespace Infraestructura.Reportes;

/// <summary>
/// Para exportar la lista de preguntas (sin html)
/// </summary>
public class ExportarPreguntas {
	private readonly AppDbContext _db;

	public ExportarPreguntas(AppDbContext db) {
		_db = db;
	}

	class PreguntaExport : Pregunta {
		public int? NumAdjuntos { get; set; }
	}

	public XLWorkbook Exportar() {
		// sin datatable, a pelo
		var wb = new XLWorkbook();
		var ws = wb.AddWorksheet("Preguntas");

		var lista = from p in _db.Pregunta
			orderby p.Nombre
			select new PreguntaExport {
				Id = p.Id,
				Nombre = p.Nombre,
				Legitimo = p.Legitimo,
				Dificultad = p.Dificultad,
				Explicacion = p.Explicacion,
				ImagenRetro = p.ImagenRetro,
				Subject = p.Subject,
				Sender = p.Sender,
				Email = p.Email,
				NumAdjuntos = string.IsNullOrEmpty(p.Adjuntos) ? 0 : AppDbContext.JsonArrayLength(p.Adjuntos) ?? 0,
			};

		var campos = new CamposExcel<PreguntaExport>();
		campos.AddCampo("Nombre Pregunta", x => x.Nombre!)
			.AddCampo("Dificultad", x => x.Dificultad!)
			.AddCampo("Legitimo", x => x.Legitimo!)
			.AddCampo("Titulo Email", x => x.Subject!)
			.AddCampo("Remitente", x => x.Sender!)
			.AddCampo("Direccion Remitente", x => x.Email!)
			.AddCampo("# Adjuntos", x => x.NumAdjuntos!)
			.AddCampo("Explicacion", x => x.Explicacion!)
			.AddCampo("Imagen retroalimentacion", x => x.ImagenRetro!)
			.AddCampo("Comentarios", x => x.Comentarios!);

		ExcelUtils.LlenarHeader(ws, 1, campos.GetHeader());

		campos.Row = 2;
		foreach (var preg in lista) {
			campos.WriteRow(ws, preg);
		}

		return wb;
	}
}