﻿using Domain.Entidades;
using Infraestructura.Servicios;

namespace Infraestructura.Persistencia;

public static class ModelExtensions {
	public static T? DatosParam<T>(this Parametro p) {
		return p.Datos == null ? default! : JSON.Parse<T>(p.Datos);
	}

	public static SesionFlujo GetSesionFlujo(this SesionPersona p) {
		return p.Flujo == null ? default! : JSON.Parse<SesionFlujo>(p.Flujo);
	}
}