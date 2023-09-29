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