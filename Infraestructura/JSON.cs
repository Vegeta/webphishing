using System.Text.Json;

namespace Infraestructura {
	/// <summary>
	/// Codificacion y decodificacion de JSON usando la libreria de Microsoft con un api similar a Javascript
	/// </summary>
	public class JSON {
		public static string Stringify(object data, bool pretty = false) {
			return JsonSerializer.Serialize(data, new JsonSerializerOptions {
				WriteIndented = pretty,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});
		}

		public static T Parse<T>(string json) {
			return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}) ?? default!;
		}
	}
}
