using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Webapp.Models {

	public class BreadcrumbsBuilder {

		public IList<BreadCrumbsItem> Items { get; set; } = new List<BreadCrumbsItem>();
		private readonly Controller con;

		public BreadcrumbsBuilder(Controller con) {
			this.con = con;
			con.ViewData["bread"] = this.Items;
		}

		public BreadcrumbsBuilder Add(BreadCrumbsItem item) {
			Items.Add(item);
			return this;
		}

		public BreadcrumbsBuilder Root(string? name = null) {
			return Add(name ?? "Inicio", "~/");
		}

		public BreadcrumbsBuilder RootArea(string? name = null, string? ruta = null) {
			var root = "~/";
			if (con.RouteData.Values.ContainsKey("area")) {
				root = root + con.RouteData.Values["area"];
				if (!String.IsNullOrEmpty(ruta))
					root = root + "/" + ruta;
			}
			return Add(name ?? "Inicio", root);
		}

		public BreadcrumbsBuilder Active(string name, string? link = null) {
			return Add(name, link, true);
		}

		public BreadcrumbsBuilder Add(string name, string? link = null, bool activo = false) {
			return Add(new BreadCrumbsItem {
				Name = name,
				Uri = link != null ? con.Url.Content(link) : "",
				Activo = activo ? "active" : ""
			});
		}

		public BreadcrumbsBuilder CurrentMenu(string path) {
			con.ViewData["currentMenu"] = path;
			return this;
		}


		public static bool Exists(ViewDataDictionary keys) {
			return keys.ContainsKey("bread");
		}

		public static IList<BreadCrumbsItem> Lista(ViewDataDictionary keys) {
			return keys["bread"] as List<BreadCrumbsItem> ??
				new List<BreadCrumbsItem>();
		}

	}

	public class BreadCrumbsItem {
		public string Name { get; set; } = "";
		public string Uri { get; set; } = "";
		public string Activo { get; set; } = "";

		public bool HasUri => !string.IsNullOrEmpty(Uri);
	}

}
