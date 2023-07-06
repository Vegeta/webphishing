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

public class Generos {

	public static IDictionary<string, string> Mapa() {
		return new Dictionary<string, string>{
			{ "M" , "MASCULINO" },
			{ "F" , "FEMENINO" },
		};
	}
}

public class Ocupaciones {
	public static IDictionary<string, string> Mapa() {
		return new Dictionary<string, string>{
			{ "estudiante" , "Estudiante" },
			{ "profesional" , "Profesional" },
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

public class RespuestaCuestionario {
	public const string Siempre = "siempre";
	public const string CasiSiempre = "casi_siempre";
	public const string AVeces = "a_veces";
	public const string CasiNunca = "casi_nunca";
	public const string Nunca = "nunca";

	public static IDictionary<string, string> Mapa() {
		return new Dictionary<string, string> {
			{"siempre" , "Siempre" },
			{"casi_siempre" , "Casi Siempre" },
			{"a_veces" , "A Veces" },
			{"casi_nunca" , "Casi Nunca" },
			{"nunca" , "Nunca" },
		};
	}
}

