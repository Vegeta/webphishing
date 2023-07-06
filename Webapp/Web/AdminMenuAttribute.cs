using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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

			
		}
		base.OnResultExecuting(context);
	}
}