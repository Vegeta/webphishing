namespace Infraestructura.Persistencia;

public static class DbHelpers {
	public static string InParameters<T>(List<T> lista) {
		Type tipo = lista.GetType().GetGenericArguments().Single();
		if (tipo == typeof(string))
			return string.Join(",", lista.Select(x => $"'{x}'"));
		return string.Join(",", lista.Select(x => x?.ToString()));
	}

	public static float DiferenciaSegundos(DateTime? inicio, DateTime? fin) {
		if (inicio.HasValue && fin.HasValue) {
			var ts = fin - inicio;
			return (float)ts.Value.TotalSeconds;
		}
		return 0;
	}
}

public sealed class Numeric {
	/// <summary>
	/// Determines if a type is numeric.  Nullable numeric types are considered numeric.
	/// </summary>
	/// <remarks>
	/// Boolean is not considered numeric.
	/// </remarks>
	public static bool Is(Type? type) {
		// from http://stackoverflow.com/a/5182747/172132
		switch (Type.GetTypeCode(type)) {
			case TypeCode.Byte:
			case TypeCode.Decimal:
			case TypeCode.Double:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.SByte:
			case TypeCode.Single:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
				return true;
			case TypeCode.Object:
				if (type!.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
					return Is(Nullable.GetUnderlyingType(type));
				}
				return false;
		}
		return false;
	}

	/// <summary>
	/// Determines if a type is numeric.  Nullable numeric types are considered numeric.
	/// </summary>
	/// <remarks>
	/// Boolean is not considered numeric.
	/// </remarks>
	public static bool Is<T>() {
		return Is(typeof(T));
	}
}