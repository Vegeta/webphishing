using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infraestructura.Servicios;

public class OperationResult<T> {
	public T? Data { get; set; }
	public string Mensaje { get; set; } = string.Empty;
	public string Error { get; set; } = string.Empty;
	public bool Valido { get; set; }

	public bool HasData => Data != null;

	public OperationResult(bool valido = false, T? data = default) {
		Data = data;
		Valido = valido;
	}
	
	public static OperationResult<T> Problem(string error, T? data = default) {
		return new() {
			Valido = false,
			Mensaje = string.Empty,
			Error = error,
			Data = data,
		};
	}

	public static OperationResult<T> Ok(string mensaje = "", T? data = default) {
		return new() {
			Valido = true,
			Mensaje = mensaje,
			Error = string.Empty,
			Data = data,
		};
	}

}