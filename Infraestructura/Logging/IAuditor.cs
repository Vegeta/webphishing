using System.Reflection;

namespace Infraestructura.Logging;

public interface IAuditor<T> {
	public void Info(string msg, object? data = null, string modulo = "");
	public void Warn(string msg, object? data = null, string modulo = "");
	public void Debug(string msg, object? data = null, string modulo = "");
	public void Error(string msg, object? data = null, string modulo = "");
	public void Error(string msg, Exception ex, string modulo = "");
}