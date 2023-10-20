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

	public static int ScoreRespuesta(string dificultad) {
		return dificultad switch {
			Facil => 1,
			Medio => 2,
			Dificil => 3,
			_ => 0
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

public class TipoUsuario {
	public const string Normal = "normal";
	public const string Manager = "manager";

	public static IDictionary<string, string> Mapa() {
		return new Dictionary<string, string>{
			{ "normal" , "Normal" },
			{ "manager" , "Manager" },
		};
	}
}

public class PermisosApp {
	public const string Seguridad = "seguridad";
	public const string Preguntas = "preguntas";
	public const string Examenes = "examenes";
	public const string Resultados = "resultados";

	public static IDictionary<string, string> Permisos() {
		return new Dictionary<string, string> {
			{"seguridad" , "Editar usuarios, perfiles" },
			{"preguntas" , "Editar preguntas" },
			{"examenes" , "Editar Tests" },
			{"reportes" , "Ver Reportes" },
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

	public static IDictionary<string, int> Calificacion() {
		return new Dictionary<string, int> {
			{"siempre" , 4 },
			{"casi_siempre" , 3},
			{"a_veces" , 2 },
			{"casi_nunca" , 1},
			{"nunca" , 0 },
		};
	}

	public static string Percepcion(decimal puntaje) {
		return puntaje switch {
			>= 7 => "ALTO",
			>= 4 => "MEDIO",
			< 7 => "BAJO",
		};
	}
}

