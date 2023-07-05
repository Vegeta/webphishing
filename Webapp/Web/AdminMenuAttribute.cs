using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Serialization;
using Webapp.Models;

namespace Webapp.Web;

public class AdminMenuAttribute : ActionFilterAttribute {
	public override void OnResultExecuting(ResultExecutingContext context) {
		// before
		if (context.Controller is Controller con) {
			var mainMenu = MenuConfig.AdminMenu();
			var menu = (new MenuConfig()).Prepare(mainMenu, con.Url);
			con.ViewBag.mainMenu = JsonSerializer.Serialize(menu.Children, new JsonSerializerOptions {
				//WriteIndented = true
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});
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
		base.OnResultExecuting(context);
	}
}