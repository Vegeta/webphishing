namespace Infraestructura.Persistencia;

public class DbHelpers {
	public static string inParameters<T>(List<T> lista) {
		Type tipo = lista.GetType().GetGenericArguments().Single();
		if (tipo == typeof(string))
			return string.Join(",", lista.Select(x => $"'{x}'"));
		return string.Join(",", lista.Select(x => x.ToString()));
	}
}