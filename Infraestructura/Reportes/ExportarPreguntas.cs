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

	private static int? NumAdjuntos(string? txt) {
		// para usar en vez de la funcion rara esa mapeada en el dbcontext
		if (string.IsNullOrEmpty(txt))
			return null;
		try {
			var temp = JSON.Parse<List<object>>(txt);
			return temp.Count;
		} catch {
			return null;
		}
	}

	public XLWorkbook Exportar() {
		// sin datatable, a pelo
		var wb = new XLWorkbook();
		var ws = wb.AddWorksheet("Preguntas");

		var l = from p in _db.Pregunta
			orderby p.Nombre
			select new {
				p.Id,
				p.Nombre,
				p.Legitimo,
				p.Dificultad,
				p.Explicacion,
				p.ImagenRetro,
				p.Subject,
				p.Sender,
				p.Email,
				Adjuntos = AppDbContext.JsonArrayLength(p.Adjuntos),
			};
		var headers = new[] {
			"Nombre Pregunta",
			"Dificultad",
			"Legitimo",
			"Titulo Email",
			"Remitente",
			"Direccion Remitente",
			"# Adjuntos",
			"Explicacion",
			"Imagen retroalimentacion",
		};

		ExcelUtils.LlenarHeader(ws, 1, headers);

		var row = 2;
		foreach (var preg in l) {
			var col = 0;
			ws.Cell(row, ++col).Value = preg.Nombre;
			ws.Cell(row, ++col).Value = preg.Dificultad;
			ws.Cell(row, ++col).Value = preg.Legitimo;
			ws.Cell(row, ++col).Value = preg.Subject;
			ws.Cell(row, ++col).Value = preg.Sender;
			ws.Cell(row, ++col).Value = preg.Email;
			ws.Cell(row, ++col).Value = preg.Adjuntos;
			ws.Cell(row, ++col).Value = preg.Explicacion;
			ws.Cell(row, ++col).Value = preg.ImagenRetro;
			row++;
		}
		return wb;
	}
}