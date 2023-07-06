using Microsoft.AspNetCore.Mvc;

namespace Webapp.Models {
	public class MenuConfig {
		private int _nav = 0;

		public MenuItem Prepare(MenuItem main, IUrlHelper url) {
			// aca iria permisos
			var root = main.Link;
			_nav = 0;
			var lista = PrepareItems(main.Children, url);

			return new MenuItem {
				Children = lista
			};
		}

		private List<MenuItem> PrepareItems(List<MenuItem> items, IUrlHelper url) {
			var lista = new List<MenuItem>();
			foreach (var item in items) {
				// TODO permisos
				if (item.Children.Count > 0) {
					var children = PrepareItems(item.Children, url);
					if (children.Count == 0)
						continue;
					item.Children.Clear();
					item.Children.AddRange(children);
					_nav++;
				}
				if (item.Children.Count > 0) {
					item.Link = "#";
					item.Navid = "menu-nav" + _nav;
				} else {
					item.Link = url.Content(item.Link);
				}
				lista.Add(item);
			}

			return lista;
		}


		// ----- configuraciones

		public static MenuItem AdminMenu() {
			var root = new MenuItem {
				Name = "Admin",
				Link = "~/manage",
				Area = "manage"
			};

			root.AddChildren(
				new MenuItem {
					Name = "Inicio",
					Link = "~/manage/inicio",
					Icon = "bi-house",
				},
				new MenuItem {
					Name = "Contenido",
					Accesos = "editor",
					Icon = "bi-journal-text",
				}.AddChildren(
					new MenuItem {
						Name = "Preguntas",
						Link = "~/manage/Preguntas"
					},
					new MenuItem {
						Name = "Examenes",
						Link = "~/manage/Examenes"
					},
					new MenuItem {
						Name = "Cuestionario",
						Link = "~/manage/Cuestionario"
					}
				),
				new MenuItem {
					Name = "Reportes",
					Accesos = "reportes, editor",
					Icon = "bi-bar-chart"
				}.AddChildren(
					new MenuItem {
						Name = "Resultados",
						Link = "~/manage/Resultados"
					}
				),
				new MenuItem {
					Name = "Administración",
					Accesos = "admin",
					Icon = "bi-terminal-fill"
				}.AddChildren(
					new MenuItem {
						Name = "Usuarios",
						Link = "~/manage/Usuarios",
					},
					new MenuItem {
						Name = "Perfiles",
						Link = "~/manage/Perfiles",
					}
				),
				new MenuItem {
					Name = "Salir",
					Link = "~/Logout",
					Icon = "bi-box-arrow-right"
				}
			);

			return root;
		}
	}
}