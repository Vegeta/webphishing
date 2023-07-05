using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebUI.Data {
	[Table("codigo_clase")]
	public class CodigoClase {
		[Key, Required]
		[Column("id")]
		public int Id { get; set; }
		[Column("key"), Required, StringLength(255)]
		public string? Key { get; set; }
		[Column("fecha_creacion")]
		public DateTime? FechaCreacion { get; set; }
	}

}
