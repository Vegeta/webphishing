using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Webapp.Models;
using Webapp.Web;

namespace Webapp.Controllers;

[AdminMenu]
[Area("manage")]
public abstract class BaseAdminController : BaseController {
	protected BreadcrumbsBuilder BreadcrumbsAdmin {
		get {
			var b = Breadcrumbs;
			if (!b.Items.Any()) {
				b.Add("Inicio", "~/manage");
			}
			return b;
		}
	}
}
