using Domain.Entidades;
using Infraestructura.Examenes;
using Infraestructura.Servicios;

namespace Infraestructura.Persistencia;

public static class ModelExtensions {
	public static T? DatosParam<T>(this Parametro p) {
		return p.Datos == null ? default! : JSON.Parse<T>(p.Datos);
	}

	public static FlujoExamenDto GetSesionFlujo(this SesionPersona p) {
		return p.Flujo == null ? default! : JSON.Parse<FlujoExamenDto>(p.Flujo);
	}

	public static FlujoExamenDto GetSesionFlujo(this VSesionPersona p) {
		return p.Flujo == null ? default! : JSON.Parse<FlujoExamenDto>(p.Flujo);
	}
}