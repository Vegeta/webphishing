using Domain;
using Domain.Entidades;
using Infraestructura.Persistencia;
using Microsoft.AspNetCore.Mvc.Rendering;
using Webapp.Models.Formularios;

namespace Webapp.Models.Consultas;
public class ConsultasAuth {
	readonly AppDbContext _db;

	public ConsultasAuth(AppDbContext db) {
		_db = db;
	}

	public IQueryable<Usuario> ListaUsuarios(FiltrosUsuario filtros) {
		IQueryable<Usuario> q = _db.Usuario;

		if (!string.IsNullOrEmpty(filtros.Email))
			q = q.Where(x => x.Email.ToUpper().Contains(filtros.Email.ToUpper()));
		if (!string.IsNullOrEmpty(filtros.Nombre))
			q = q.Where(x => x.Nombres.ToUpper().Contains(filtros.Nombre.ToUpper()));

		var ordenador = new SortExpressionHelper<Usuario>()
				.Add("email", x => x.Email)
				.Add("nombres", x => x.Apellidos, x => x.Nombres)
				.Add("creacion", x => x.Creacion)
				.Add("modificacion", x => x.Modificacion);

		var order = filtros.OrdenCampo;
		if (!string.IsNullOrEmpty(filtros.OrdenCampo)) {
			q = ordenador.SetOrder(q, filtros.OrdenCampo, filtros.OrdenDir);
		}

		return q;
	}

	public IQueryable<Perfil> ListaPerfiles(FiltrosPerfiles filtros) {
		IQueryable<Perfil> q = _db.Perfil;

		if (!string.IsNullOrEmpty(filtros.Nombre))
			q = q.Where(x => x.Nombre.ToUpper().Contains(filtros.Nombre.ToUpper()));

		if (!string.IsNullOrEmpty(filtros.Identificador))
			q = q.Where(x => x.Identificador.ToUpper().Contains(filtros.Identificador.ToUpper()));

		var ordenador = new SortExpressionHelper<Perfil>()
				.Add("nombre", x => x.Nombre)
				.Add("identificador", x => x.Identificador);

		var order = filtros.OrdenCampo;
		if (!string.IsNullOrEmpty(filtros.OrdenCampo)) {
			q = ordenador.SetOrder(q, filtros.OrdenCampo, filtros.OrdenDir);
		}

		return q;
	}

	public List<SelectListItem> ComboPerfiles() {
		var lista = _db.Perfil
			.OrderBy(x => x.Nombre)
			.Select(x => new { nombre = x.Nombre, id = x.Id })
			.ToList()
			.Select(x => new SelectListItem(x.nombre, x.id.ToString()))
			.ToList();
		lista.Insert(0, new SelectListItem());
		return lista;
	}

}
