using Domain.Entidades;

namespace Infraestructura.Persistencia;

public static class ModelExtensions {
	public static T? DatosParam<T>(this Parametro p) {
		return p.Datos == null ? default! : JSON.Parse<T>(p.Datos);
	}
}