using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Text.Json;
using Webapp.Models;
using Webapp.Web;

namespace Webapp.Controllers {
	[FiltrosWeb]
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public abstract class BaseController : Controller {

		private BreadcrumbsBuilder? _bread;

		protected virtual BreadcrumbsBuilder Breadcrumbs {
			get {
				if (_bread == null) {
					_bread = new BreadcrumbsBuilder(this);
				}
				return _bread;
			}
		}

		protected void Titulo(string titulo) {
			ViewData["titulo"] = titulo;
		}

		protected BaseController CurrentMenu(string path) {
			ViewData["currentMenu"] = path;
			return this;
		}

		protected string ToJson(object data, bool pretty = false) {
			return JsonSerializer.Serialize(data, new JsonSerializerOptions {
				WriteIndented = pretty,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});
		}

		protected T FromJson<T>(string json) {
			return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions {
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}) ?? default!;
		}

		public BaseController ConfirmaWeb(string confirma) {
			TempData["confirma"] = confirma;
			return this;
		}

		public BaseController ErrorWeb(string confirma) {
			TempData["error"] = confirma;
			return this;

		}
	}

}