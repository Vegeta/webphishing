using Domain.Entidades;
using Infraestructura.Persistencia;

namespace Infraestructura.Servicios;

public class CatalogoGeneral {
	private readonly AppDbContext _db;

	public CatalogoGeneral(AppDbContext db) {
		_db = db;
	}

	public IList<string> Carreras() {
		return DatosParametro<List<string>>("carreras") ?? new();
	}

	public ConfiguracionEvaluacion ConfigGeneral() {
		return DatosParametro<ConfiguracionEvaluacion>("general") ?? new();
	}

	// metodos generales

	public Parametro PorNombre(string nombre) {
		return _db.Parametro.FirstOrDefault(x => x.Nombre == nombre)
		       ?? new Parametro { Nombre = nombre };
	}

	public T? DatosParametro<T>(string nombre, T defecto = default!) {
		var p = PorNombre(nombre);
		return p.Datos == null ? defecto : p.DatosParam<T>();
	}

	public string? JsonData(string nombre) {
		return PorNombre(nombre).Datos;
	}

	public dynamic? ParametroDinamico(string nombre) {
		var param = PorNombre(nombre);
		return param.DatosParam<dynamic?>();
	}

	public Parametro GuardarParametro(string nombre, object? data, string? comentario = null) {
		var param = PorNombre(nombre);
		param.Nombre = nombre;
		if (comentario != null)
			param.Comentario = comentario;
		param.Datos = JSON.Stringify(data);
		param.FechaModificacion = DateTime.Now;
		if (param.Id == 0) {
			param.FechaCreacion = DateTime.Now;
			_db.Parametro.Add(param);
		}
		_db.SaveChanges();
		return param;
	}
}