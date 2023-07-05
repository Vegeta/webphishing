using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebUI.Dto {
	public class CodigoDto {
		[JsonPropertyName("key")]
		public string? Key { get; set; }
	}

}
