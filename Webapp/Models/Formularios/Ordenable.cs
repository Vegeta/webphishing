namespace Webapp.Models.Formularios;

public abstract class Ordenable {
	public string OrdenCampo { get; set; } = "";
	public string OrdenDir { get; set; } = "asc";
}
