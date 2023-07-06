using System.ComponentModel.DataAnnotations;
using Domain;
using Domain.Entidades;
using Infraestructura;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Webapp.Models;

public class PerfilModelWeb {
	public int? Id { get; set; }
	[Required]
	public string Nombre { get; set; } = null!;
	[Required]
	public string Identificador { get; set; } = null!;

	public List<string> Permisos { get; set; } = default!;

	public List<SelectListItem> ObtenerOpciones(Perfil perfil) {
		var actuales = ParsePermisos(perfil);

		var lista = new List<SelectListItem>();
		var opciones = PermisosApp.Permisos();
		foreach (var item in opciones) {
			var listItem = new SelectListItem(item.Value,
				item.Key,
				actuales.Contains(item.Key)
			);
			lista.Add(listItem);
		}
		return lista;
	}

	public static List<string> ParsePermisos(Perfil perfil) {
		return JSON.Parse<List<string>>(perfil.Permisos ?? "[]");
	}


}