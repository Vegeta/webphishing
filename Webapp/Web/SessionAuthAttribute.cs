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

		if (cumple && !string.IsNullOrEmpty(tipo)) {
			var user = userMan?.CurrentUser() ?? new SessionInfo();
			cumple = cumple && user.Tipo == tipo;
		}

		if (cumple && !string.IsNullOrEmpty(args)) {
			var user = userMan?.CurrentUser() ?? new SessionInfo();
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
}
