using Microsoft.AspNetCore.Mvc.Rendering;
using Domain.Transferencia;
using Newtonsoft.Json;

namespace Webapp.Models;

public class ExamenModelWeb : ExamenModel {
	public List<SelectListItem> Tipos { get; set; } = new();
	public List<int> Deleted { get; set; } = new(); // preguntas borradas
}


