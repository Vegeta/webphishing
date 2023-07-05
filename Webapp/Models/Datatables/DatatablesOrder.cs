namespace Webapp.Models.Datatables;

/// <summary>
/// Define el ordenamiento de la lista
/// </summary>
public class DatatablesOrder {
	public string Campo { get; set; } = "";
	public string Dir { get; set; } = "asc";
}
