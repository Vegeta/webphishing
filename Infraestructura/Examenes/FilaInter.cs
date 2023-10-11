namespace Infraestructura.Examenes;

public class FilaInter {
	public string Link { get; set; } = "";
	public string Tipo { get; set; } = "";
	
	public int Clicks { get; set; }
	public decimal Hover { get; set; }

	public FilaInter() {
	}

	public FilaInter(string tipo, string link) {
		Tipo = tipo;
		Link = link;
	}
}