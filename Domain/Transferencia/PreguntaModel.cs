using System.ComponentModel.DataAnnotations;

namespace Domain.Transferencia;

public class PreguntaModel {
	public int? Id { get; set; }
	[Required]
	public string? Nombre { get; set; }
	[Required]
	public string? Subject { get; set; }
	public int Legitimo { get; set; } = 0;
	[Required]
	public string Dificultad { get; set; } = "facil";
	public string? Explicacion { get; set; }
	public string? Sender { get; set; }
	public string? Email { get; set; }
	public string? Html { get; set; }
	public List<AdjuntoView> ListaAdjuntos { get; set; } = new List<AdjuntoView>();
}

public class AdjuntoView {
	public string? Name { get; set; }
	public string? File { get; set; }
	public string? Size { get; set; }
}

public class PreguntaContentModel {
	public int Id { get; set; }
	[Required]
	public string? Html { get; set; }
	public string? Nombre { get; set; }
}


