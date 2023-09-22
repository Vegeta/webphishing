using Infraestructura;
using Infraestructura.Identity;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Webapp.Web;

public class SessionAuthAttribute : ActionFilterAttribute {

	private readonly string? args;
	private readonly string? tipo;

	public SessionAuthAttribute(string? args = null, string? tipo = null) {
		this.tipo = tipo;
		this.args = args;
	}

	public override void OnResultExecuting(ResultExecutingContext context) {
		var publico = context.ActionDescriptor.EndpointMetadata.Any(x => x.GetType() == typeof(AllowAnonymousAttribute));
		if (publico)
			return;

		var userMan = context.HttpContext.RequestServices.GetService<IUserAccesor>();
		var userData = context.HttpContext.Session.GetString("user");
		var ajax = context.HttpContext.Request.IsAjaxRequest();
		var cumple = !string.IsNullOrEmpty(userData);

		var user = userMan?.CurrentUser() ?? new SessionInfo();

		if (cumple && !string.IsNullOrEmpty(tipo)) {
			cumple = cumple && user.Tipo == tipo;
		}

		if (cumple && !string.IsNullOrEmpty(args)) {
			cumple = cumple && UsuariosService.AutorizarPermisos(user.Permisos, args);
		}

		if (cumple)
			return;

		if (ajax) {
			context.Result = new UnauthorizedObjectResult(new { error = "No autorizado" });
		} else {
			var request = context.HttpContext.Request;
			//var url = $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}";
			var url = $"{request.PathBase}{request.Path}{request.QueryString}";
			var res = new RedirectToActionResult("Index", "Ingreso", new { area = "" });
			context.HttpContext.Session.SetString("returnUrl", url);
			context.Result = res;
		}
	}

	public override void OnActionExecuting(ActionExecutingContext context) {
		if (context.Controller is not Controller con)
			return;
		var userMan = context.HttpContext.RequestServices.GetService<IUserAccesor>();
		var user = userMan?.CurrentUser() ?? new SessionInfo();

		if (string.IsNullOrEmpty(user.Apellidos))
			return;

		con.ViewData["userNombre"] = $"{user.Nombres} {user.Apellidos}";

		var inicial = (user.Nombres.Length > 0) ? user.Nombres[0].ToString().ToUpper() + ". " : "";
		con.ViewData["userCorto"] = $"{inicial}{user.Apellidos}";
	}
}
