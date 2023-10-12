using Infraestructura.Identity;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Webapp.Models {
	public class MenuConfig {
		private int _nav = 0;

		private readonly IUserAccesor _userAccesor;

		public MenuConfig(IUserAccesor userAccesor) {
			_userAccesor = userAccesor;
		}

		protected SessionInfo userInfo = new();

		public MenuItem Prepare(MenuItem main, IUrlHelper url) {
			// aca iria permisos
			userInfo = _userAccesor.CurrentUser();
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
				if (item.TienePermisos) {
					if (!UsuariosService.AutorizarPermisos(userInfo.Permisos, item.Accesos))
						continue;
				}

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
					Accesos = "preguntas|examenes",
					Icon = "bi-journal-text",
				}.AddChildren(
					new MenuItem {
						Name = "Preguntas",
						Link = "~/manage/Preguntas",
						Accesos = "preguntas"
					},
					new MenuItem {
						Name = "Examenes",
						Link = "~/manage/Examenes",
						Accesos = "examenes"
					},
					new MenuItem {
						Name = "Cuestionario",
						Link = "~/manage/Cuestionario",
						Accesos = "examenes"
					},
					new MenuItem {
						Name = "Imágenes",
						Link = "~/manage/Imagenes",
						Accesos = "preguntas"
					}
				),
				new MenuItem {
					Name = "Reportes",
					Accesos = "resultados",
					Icon = "bi-bar-chart"
				}.AddChildren(
					new MenuItem {
						Name = "Evaluaciones",
						Link = "~/manage/Evaluaciones"
					},
					new MenuItem {
						Name = "Personas",
						Link = "~/manage/Personas"
					}
				),
				new MenuItem {
					Name = "Administración",
					Icon = "bi-terminal-fill",
					Accesos = "seguridad",
				}.AddChildren(
					new MenuItem {
						Name = "Configuración",
						Link = "~/manage/Configuracion",
					},
					new MenuItem {
						Name = "Usuarios",
						Link = "~/manage/Usuarios",
					},
					new MenuItem {
						Name = "Perfiles",
						Link = "~/manage/Perfiles",
					},
					new MenuItem {
						Name = "Auditorias",
						Link = "~/manage/Auditorias",
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