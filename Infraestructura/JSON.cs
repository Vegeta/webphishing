using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Infraestructura;

/// <summary>
/// Codificacion y decodificacion de JSON usando la libreria de Microsoft con un api similar a Javascript
/// </summary>
public class JSON {
	public static string Stringify(object? data, bool pretty = false) {
		if (data == null) return "null";
		return JsonSerializer.Serialize(data, new JsonSerializerOptions {
			WriteIndented = pretty,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});
	}

	public static T Parse<T>(string json) {
		var options = new JsonSerializerOptions {
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			NumberHandling = JsonNumberHandling.AllowReadingFromString,
		};
		options.Converters.Add(new JsonConverterNullableInt());
		options.Converters.Add(new JsonConverterDoubleInt());
		return JsonSerializer.Deserialize<T>(json, options) ?? default!;
	}
}

public class JsonConverterNullableInt : JsonConverter<int?> {
	public override bool HandleNull => true;

	public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		if (reader.TokenType == JsonTokenType.Null)
			return null;
		if (reader.TokenType == JsonTokenType.String) {
			var r = reader.GetString();
			if (string.IsNullOrEmpty(r))
				return null;
			if (!int.TryParse(r, out int val))
				throw new JsonException("Valor no puede ser convertido en int");
			return val;
		}
		return reader.GetInt32();
	}

	public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options) {
		if (value.HasValue)
			writer.WriteNumberValue(value.Value);
		else
			writer.WriteStringValue(string.Empty);
	}
}

public class JsonConverterDoubleInt : JsonConverter<float?> {
	public override bool HandleNull => true;

	public override float? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		if (reader.TokenType == JsonTokenType.Null)
			return null;
		if (reader.TokenType == JsonTokenType.String) {
			var r = reader.GetString();
			if (string.IsNullOrEmpty(r))
				return null;
			if (!float.TryParse(r, out float val))
				throw new JsonException("Valor no puede ser convertido en float");
			return val;
		}
		return reader.GetInt32();
	}

	public override void Write(Utf8JsonWriter writer, float? value, JsonSerializerOptions options) {
		if (value.HasValue)
			writer.WriteNumberValue(value.Value);
		else
			writer.WriteStringValue(string.Empty);
	}
}