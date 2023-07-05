using System.Data.SqlTypes;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebUI.Dto {
#pragma warning disable IDE1006 // Naming Styles
	public class PerfilDto {

		public int id { get; set; }

		public string? nombre { get; set; }
		public string? apellido { get; set; }
		public string? profesion { get; set; }
		public string? carrera { get; set; }
		public string? email { get; set; }
		public string? genero { get; set; }
		public int? edad { get; set; }
		public string? key { get; set; }

		public DateTime? FechaInicio { get; set; }
		public List<int> ordenEjercicios { get; set; } = new List<int>();
		public List<EjercicioDto> ejercicios { get; set; } = new List<EjercicioDto>();
		public List<CuestionarioDto> cuestionario { get; set; } = new List<CuestionarioDto>();
		public ResultadoDto resultados { get; set; } = new ResultadoDto();
	}

	public class EjercicioDto {
		public List<List<string>> focus { get; set; } = new List<List<string>>();
		public List<List<string>> click { get; set; } = new List<List<string>>();
		public float? tiempo { get; set; }
		public string? comentario { get; set; }
		public string? respuesta { get; set; }
		public string? tipo { get; set; }
	}

	public class CuestionarioDto {
		public string? pregunta { get; set; }
		public string? respuesta { get; set; }
		public int? puntuacion { get; set; }
	}
	public class ResultadoDto {
		public float? tiempoTotal { get; set; }
		public float? tiempoPromedio { get; set; }
		public int? tasaExito { get; set; }
		public int? tasaError { get; set; }
	}

#pragma warning restore IDE1006 // Naming Styles
}
