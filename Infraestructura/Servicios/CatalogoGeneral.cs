using Domain.Entidades;
using Infraestructura.Persistencia;

namespace Infraestructura.Servicios;

public class CatalogoGeneral {
	private readonly AppDbContext _db;

	public CatalogoGeneral(AppDbContext db) {
		_db = db;
	}

	public IList<string> Carreras() {
		var p = PorNombre("carreras");
		return p.DatosParam<List<string>>() ?? new List<string>();
	}
	
	protected Parametro PorNombre(string nombre) {
		return _db.Parametro.FirstOrDefault(x => x.Nombre == nombre)
			   ?? new Parametro { Nombre = nombre };
	}

}