using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebUI.Data {
	[Table("perfil")]
	public class Perfil {
		[Key, Required]
		[Column("id")]
		public int Id { get; set; }

		[Column("nombre"), StringLength(255)]
		public string? Nombre { get; set; }

		[Column("apellido"), StringLength(255)]
		public string? Apellido { get; set; }

		[Column("profesion"), StringLength(255)]
		public string? Profesion { get; set; }

		[Column("carrera"), StringLength(255)]
		public string? Carrera { get; set; }

		[Column("email"), StringLength(255)]
		public string? Email { get; set; }

		[Column("genero"), StringLength(255)]
		public string? Genero { get; set; }

		[Column("edad")]
		public int? Edad { get; set; }

		[Column("creacion")]
		public DateTime Creacion { get; set; }

		[StringLength(255)]
		public string? Key { get; set; }

		public List<PruebaPerfil> Tests { get; set; } = default!;
	}

	[Table("prueba_perfil")]
	public class PruebaPerfil {
		[Key, Required]
		public int Id { get; set; }
		public int PerfilId { get; set; }
		public Perfil? Perfil { get; set; }
		public DateTime? Fecha { get; set; }

		public List<Respuesta> Respuestas { get; set; } = default!;

		public float? TiempoTotal { get; set; }
		public float? TiempoPromedio { get; set; }
		public int? TasaExito { get; set; }
		public int? TasaError { get; set; }

		[Column(name: "orden_ejercicios", TypeName = "json")]
		public List<int>? OrdenEjercicios { get; set; } = default!; // TEMP

		[Column(name: "cuestionario", TypeName = "json")]
		public string? Cuestionario { get; set; } = default!;
	}

	[Table("respuesta")]
	public class Respuesta {
		[Key, Required]
		public int Id { get; set; }
		[ForeignKey("Prueba"), Column("prueba_id")]
		public int TestId { get; set; }
		public PruebaPerfil? Prueba { get; set; }

		public int? Numero { get; set; }
		public int? Orden { get; set; }

		[Column(name: "focus", TypeName = "json")]
		public List<List<string>>? Focus { get; set; } = new List<List<string>>();

		[Column(name: "click", TypeName = "json")]
		public List<List<string>>? Click { get; set; } = new List<List<string>>();
		public float? Tiempo { get; set; }
		[DataType("text")]
		public string? Comentario { get; set; }
		[StringLength(255)]
		public string? Texto { get; set; } // respuesta
		[StringLength(255)]
		public string? Tipo { get; set; }

	}

}
