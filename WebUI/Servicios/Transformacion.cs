using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using WebUI.Data;
using WebUI.Dto;

namespace WebUI.Servicios {
	public class Transformacion {

		public DbPhishing Db { get; }

		public Transformacion(DbPhishing db) {
			this.Db = db;
		}

		public PerfilDto Transformar(Perfil p, PruebaPerfil? prueba = null) {
			// automapper se complica
			var dto = new PerfilDto {
				apellido = p.Apellido,
				nombre = p.Nombre,
				carrera = p.Carrera,
				email = p.Email,
				edad = p.Edad,
				FechaInicio = p.Creacion,
				genero = p.Genero,
				profesion = p.Profesion,
				key = p.Key,
			};

			//checks, etc.
			var test = prueba ?? p.Tests[0];

			dto.resultados = new ResultadoDto {
				tasaError = test.TasaError,
				tasaExito = test.TasaExito,
				tiempoPromedio = test.TiempoPromedio,
				tiempoTotal = test.TiempoTotal
			};

			if (test.Cuestionario != null)
				dto.cuestionario = JsonSerializer.Deserialize<List<CuestionarioDto>?>(test.Cuestionario) ?? new List<CuestionarioDto>(); // que joda

			foreach (var r in test.Respuestas) {
				dto.ordenEjercicios = test.OrdenEjercicios ?? default!;

				var edto = new EjercicioDto {
					comentario = r.Comentario,
					respuesta = r.Texto,
					tiempo = r.Tiempo,
					tipo = r.Tipo,
					click = r.Click ?? new List<List<string>>(),
					focus = r.Focus ?? new List<List<string>>(),
				};
				dto.ejercicios.Add(edto);
			}
			return dto;
		}

		public List<PerfilDto> ListaPerfiles() {
			var perfiles = Db.perfiles.AsNoTracking().ToList();
			var pruebas = Db.pruebas
				.Include(p => p.Respuestas)
				.ToList()
				.ToLookup(x => x.PerfilId);

			var list = new List<PerfilDto>();
			foreach (var p in perfiles) {
				if (!pruebas.Contains(p.Id))
					continue;
				var test = pruebas[p.Id].First();
				var dto = Transformar(p, test);
				list.Add(dto);
			}
			return list;
		}

		public Perfil UpsertPerfil(PerfilDto dto) {
			var perfil = Db.perfiles.Where(x => x.Email == dto.email).FirstOrDefault();

			perfil ??= new Perfil {
				Apellido = dto.apellido,
				Nombre = dto.nombre,
				Carrera = dto.carrera,
				Email = dto.email,
				Edad = dto.edad,
				Creacion = DateTime.Now,
				Genero = dto.genero,
				Profesion = dto.profesion,
				Key = dto.key,
				Tests = new List<PruebaPerfil>()
			};

			var test = Db.PrimeraPrueba(perfil.Id);

			if (test == null) {
				test = new PruebaPerfil {
					Fecha = DateTime.Now,
				};
				perfil.Tests.Add(test);
			}
			test.OrdenEjercicios = dto.ordenEjercicios ?? new List<int>();
			if (perfil.Id == 0) {
				Db.perfiles.Add(perfil);
			}
			Db.SaveChanges();

			return perfil;
		}

		public Perfil GuardarTest(PerfilDto dto) {
			var perfil = Db.perfiles.Where(x => x.Email == dto.email).FirstOrDefault();

			perfil ??= new Perfil {
				Apellido = dto.apellido,
				Nombre = dto.nombre,
				Carrera = dto.carrera,
				Email = dto.email,
				Edad = dto.edad,
				Creacion = DateTime.Now,
				Genero = dto.genero,
				Profesion = dto.profesion,
				Tests = new List<PruebaPerfil>()
			};

			var test = new PruebaPerfil {
				Cuestionario = JsonSerializer.Serialize(dto.cuestionario),
				Fecha = DateTime.Now,
				Respuestas = new List<Respuesta>()
			};

			var tiempoTotal = 0f;
			var tasa = 0;
			var i = 0;
			foreach (var ej in dto.ejercicios) {
				var res = new Respuesta {
					Click = ej.click,
					Focus = ej.focus,
					Comentario = ej.comentario,
					Texto = ej.respuesta,
					Tiempo = ej.tiempo,
					Orden = i + 1,
					Numero = dto.ordenEjercicios[i]
				};
				tiempoTotal += ej.tiempo ?? 0;
				if (ej.respuesta == ej.tipo)
					tasa++;
				test.Respuestas.Add(res);
				i++;
			}

			// resultado
			test.TasaExito = tasa;
			test.TasaError = dto.ejercicios.Count - tasa;
			test.TiempoTotal = tiempoTotal;
			test.TiempoPromedio = tiempoTotal / dto.ejercicios.Count;

			perfil.Tests.Add(test);
			if (perfil.Id == default) {
				Db.perfiles.Add(perfil);
			}
			Db.SaveChanges();
			return perfil;
		}

	}
}