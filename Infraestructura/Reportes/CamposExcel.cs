using ClosedXML.Excel;

namespace Infraestructura.Reportes;

/// <summary>
/// Permite llenar la definicion de campos exportables a excel con headers y contenido dinamico
/// </summary>
/// <typeparam name="T"></typeparam>
public class CamposExcel<T> {
	public List<Tuple<string, Func<T, object>>> Lista { get; } = new();

	public int Row { get; set; }

	public CamposExcel<T> AddCampo(string titulo, Func<T, object> exp) {
		var item = new Tuple<string, Func<T, object>>(titulo, exp);
		Lista.Add(item);
		return this;
	}

	public IList<string> GetHeader() {
		return Lista.Select(x => x.Item1).ToList();
	}

	public void WriteRow(IXLWorksheet ws, T subject) {
		if (Row == 0)
			Row = 1;
		var col = 0;
		foreach (var cell in Lista) {
			var func = cell.Item2.Invoke(subject);
			var value = XLCellValue.FromObject(func);
			ws.Cell(Row, ++col).Value = value;
		}
		Row++;
	}
}