using Dapper;
using Domain;
using Domain.Entidades;
using Infraestructura.Examenes.Asignacion;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Examenes;

/// <summary>
/// Asignador para Examenes generales aleatorios que usan to do el pool
/// de preguntas
/// </summary>
public class FlujoAleatorio : IFlujoExamen {
	private readonly AppDbContext _db;
	private static readonly Random Rng = new();
	private ConfigExamen _config;
	public AlgoritmoAsignacion Algoritmo { get; }

	public FlujoAleatorio(AppDbContext db, ConfigExamen config) {
		_db = db;
		_config = config;
		Algoritmo = new AlgoritmoAsignacion();
	}

	public FlujoExamenDto CrearFlujo() {
		var flujo = new FlujoExamenDto {
			NumPreguntas = _config.NumPreguntas,
			CuestionarioPos = _config.CuestionarioPos,
			Aleatorio = true,
			Tipo = TipoExamen.Predeterminado
		};

		var inicio = Algoritmo.SiguienteAsignacion(0, 0) ?? new Step();
		if (!inicio.DebeAsignar) {
			flujo.Error = "No se encontraron preguntas para asignar";
			return flujo;
		}

		AsignarPreguntas(flujo, inicio);
		return flujo;
	}

	public void ResolverPreguntas(FlujoExamenDto flujo) {
		var respondidas = flujo.Respondidas;

		// esto es para el caso cuando sean mas de 10 preguntas, aplica una correccion brutal
		var score = flujo.Score;
		var next = respondidas + 1;
		var mod = next % AlgoritmoAsignacion.MaxPreguntas;

		if (next >= AlgoritmoAsignacion.MaxPreguntas && mod == 0) {
			Console.WriteLine("reiniciar");
			var reinicio = Algoritmo.SiguienteAsignacion(0, 0);
			if (reinicio != null) {
				AsignarPreguntas(flujo, reinicio);
				return;
			}
		}

		if (respondidas >= AlgoritmoAsignacion.MaxPreguntas) {
			score = flujo.ScoreOffset(mod);
			respondidas = mod;
		}

		var nextPaso = Algoritmo.SiguienteAsignacion(respondidas, score);
		if (nextPaso != null) {
			AsignarPreguntas(flujo, nextPaso);
		}
	}

	public void AsignarPreguntas(FlujoExamenDto flujo, Step pasoAlgo) {
		if (!pasoAlgo.DebeAsignar)
			return;
		var idsUsados = flujo.Pasos
			.Where(x => x.Accion == "pregunta")
			.Select(x => x.EntidadId).ToList();
		// TODO control num preguntas
		var lista = new List<PasoExamen>();
		foreach (var dificultad in pasoAlgo.TomarPreguntas) {
			if (idsUsados.Count >= flujo.NumPreguntas)
				break;
			var preg = SiguientePregunta(dificultad, idsUsados);
			if (preg == null)
				continue;
			idsUsados.Add(preg.Id);
			var item = new PasoExamen {
				Accion = "pregunta",
				EntidadId = preg.Id,
				Dificultad = preg.Dificultad ?? DificultadPregunta.Facil,
				Real = preg.Legitimo == 0 ? "phishing" : "legitimo",
			};
			//flujo.Pasos.Add(item);
			lista.Add(item);
			flujo.MaxScore += DificultadPregunta.ScoreRespuesta(item.Dificultad);
		}
		if (lista.Count == 0)
			return;
		// randomizar la lista aun mas, comprobar con el flujo?
		lista = lista.OrderBy(_ => Rng.Next()).ToList();
		flujo.Pasos.AddRange(lista);
	}

	public Pregunta? SiguientePregunta(string dificultad, List<int> excluir) {
		// uso de funcion custom para el ordenamiento aleatorio
		var q = _db.Pregunta.Where(x => x.Dificultad == dificultad);

		if (excluir.Count > 0) {
			q = q.Where(x => !excluir.Contains(x.Id));
		}

		return q.Select(res => new Pregunta {
				Id = res.Id,
				Dificultad = res.Dificultad,
				Legitimo = res.Legitimo,
				Nombre = res.Nombre
			})
			.OrderBy(x => AppDbContext.Random())
			.FirstOrDefault();
	}

	public Pregunta? SiguientePreguntaDapper(string dificultad, List<int> excluir) {
		// ----- ESTO ES UNA PRUEBA DE CONCEPTO CON DAPPER ------
		// ojo, esto en teoria devuelve elementos aleatorios, probar
		// esta desgracia es necesaria por que EF no es flexible, asdjajsdajhjs
		// Dapper sql builder

		var builder = new SqlBuilder();
		builder.Select("id, nombre, dificultad, legitimo")
			.Where("dificultad = @d", new { d = dificultad })
			.OrderBy("random()");

		if (excluir.Count > 0) {
			var inParams = DbHelpers.InParameters(excluir);
			builder.Where($"id not in ({inParams})");
		}

		var tpl = builder.AddTemplate("select /**select**/ from pregunta /**where**/ /**orderby**/ limit 1");

		var res = _db.Database.GetDbConnection()
			.QueryFirst(tpl.RawSql, tpl.Parameters);

		return res == null
			? null
			: new Pregunta {
				Id = res.id,
				Dificultad = res.dificultad,
				Legitimo = res.legitimo,
				Nombre = res.nombre
			};
	}
}