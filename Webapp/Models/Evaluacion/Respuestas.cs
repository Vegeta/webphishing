using Domain.Entidades;
using Infraestructura.Examenes;
using Infraestructura.Servicios;

namespace Webapp.Models.Evaluacion;

public class AccionExamenWeb {
	public object? Data { get; set; }
	public string Accion { get; set; } = "";

	public object? Estado { get; set; }
	public string? Error { get; set; }
	public int Indice { get; set; }
	public string? Url { get; set; }
}

public class RespuestaCuest {
	public List<CuestRespuestaModel> Respuestas { get; set; } = new();

	public float? TiempoCuest { get; set; }
	
	public FlujoExamenDto? Estado { get; set; }
}

public class RespuestaSim : RespuestaWeb {
	public FlujoExamenDto? Estado { get; set; }
}

public class RespuestaWeb {
	public string Token { get; set; } = "";
	public int PreguntaId { get; set; }
	public DateTime? Inicio { get; set; }
	public DateTime? Fin { get; set; }
	public string? Respuesta { get; set; }
	public string? Comentario { get; set; }
	public string? Interaccion { get; set; } // esto es el JSON de clicks y focus
}

public class InfoExamen {
	public SesionPersona Sesion { get; set; } = new();
	public string Error { get; set; } = "";
	public string Token { get; set; } = "";
	public bool HasError => !string.IsNullOrEmpty(Error);
}