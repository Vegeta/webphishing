using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace Webapp.Web;

public class FiltrosWebAttribute : ActionFilterAttribute {
	public override void OnActionExecuting(ActionExecutingContext context) {
		if (context.Controller is Controller con) {
			con.ViewBag.claseHeader = "header-inner-pages";

			// https://www.blakepell.com/blog/aspnet-core-is-the-current-request-from-ajax

			var controllerRoute = con.RouteData.Values["controller"] as string;
			var areaRoute = con.RouteData.Values["area"] as string;

			var sb = new StringBuilder("~");
			if (!string.IsNullOrEmpty(areaRoute))
				sb.Append("/").Append(areaRoute);
			if (!string.IsNullOrEmpty(controllerRoute))
				sb.Append("/").Append(controllerRoute);

			var self = sb.ToString();
			if (self == "~")
				self = "~/";
			con.ViewData["self"] = con.Url.Content(self);
		}
		base.OnActionExecuting(context);
	}
}