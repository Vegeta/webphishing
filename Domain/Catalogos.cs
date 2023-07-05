using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain;

public class DificultadPregunta {

	public const string Facil = "facil";
	public const string Medio = "medio";
	public const string Dificil = "dificil";

	public static IDictionary<string, string> Mapa() {
		return new Dictionary<string, string>{
			{ "facil" , "Facil" },
			{ "medio" , "Medio" },
			{ "dificil" , "Dificil" }
		};
	}

}

public class TipoExamen {
	public const string Predeterminado = "predeterminado";
	public const string Personalizado = "personalizado";

	public static IDictionary<string, string> Mapa() {
		return new Dictionary<string, string>{
			{ "predeterminado" , "Predeterminado" },
			{ "personalizado" , "Personalizado" },
		};
	}
}

public class PermisosApp {

	public static IDictionary<string, string> Permisos() {
		return new Dictionary<string, string> {
			{"seguridad" , "Editar usuarios, perfiles" },
			{"preguntas" , "Editar preguntas" },
			{"examenes" , "Editar Tests" },
			{"resultados" , "Ver resultados" },
		};
	}
}
