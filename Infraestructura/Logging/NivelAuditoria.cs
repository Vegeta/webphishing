namespace Infraestructura.Logging;

public class NivelAuditoria {
	public const string WARNING = "WARNING";
	public const string ERROR = "ERROR";
	public const string INFO = "INFO";
	public const string DEBUG = "DEBUG";

	public static List<string> Niveles() {
		return new List<string>() { INFO, WARNING, ERROR, DEBUG };
	}
}