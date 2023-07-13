using System.Text.Json.Serialization;

namespace Webapp.Models {
	public class MenuItem {
		public string Name { get; set; } = "";
		public string Link { get; set; } = "";
		[JsonIgnore]
		public string Accesos { get; set; } = "";
		public string Area { get; set; } = "";
		public string Icon { get; set; } = "";
		public string Navid { get; set; } = "";
		public List<MenuItem> Children { get; set; } = new List<MenuItem>();

		public MenuItem Add(MenuItem child) {
			Children.Add(child);
			return this;
		}

		[JsonIgnore]
		public bool HasChildren => Children.Count > 0;

		public MenuItem AddChildren(params MenuItem[] items) {
			Children.AddRange(items);
			return this;
		}

		[JsonIgnore]
		public IList<string> Permisos { get; set; } = new List<string>();

		public bool TienePermisos => !string.IsNullOrEmpty(Accesos);
	}
}
