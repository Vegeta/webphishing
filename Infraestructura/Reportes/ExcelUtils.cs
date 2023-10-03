using ClosedXML.Excel;

namespace Infraestructura.Reportes;

public static class ExcelUtils {
	public const string TipoMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

	public static string ColumnIndexToColumnLetter(int colIndex) {
		// https://www.add-in-express.com/creating-addins-blog/convert-excel-column-number-to-name/
		int div = colIndex;
		string colLetter = String.Empty;
		int mod = 0;

		while (div > 0) {
			mod = (div - 1) % 26;
			colLetter = (char)(65 + mod) + colLetter;
			div = (int)((div - mod) / 26);
		}
		return colLetter;
	}

	public static int LlenarHeader(IXLWorksheet ws, int row, IEnumerable<string> headers) {
		int col = 0;
		foreach (var head in headers) {
			col++;
			ws.Cell(row, col).Value = head;
			ws.Cell(row, col).Style.Font.Bold = true;
		}
		return col;
	}

	public static string? UpperNoUnder(string? txt) {
		if (string.IsNullOrEmpty(txt))
			return null;
		return txt.ToUpperInvariant().Replace("_", " ");
	}

}