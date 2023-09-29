using Dapper;
using Domain;
using Domain.Entidades;
using Infraestructura.Examenes.Asignacion;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Examenes;

public class AsignadorAleatorio : IAsignadorExamen {
	private readonly AppDbContext _db;
	private static readonly Random Rng = new();

	public AlgoritmoAsignacion Algoritmo { get; }

	public AsignadorAleatorio(AppDbContext db) {
		_db = db;
		Algoritmo = new AlgoritmoAsignacion();
	}

	public FlujoExamenDto CrearFlujo(ConfigExamen config) {
		var flujo = new FlujoExamenDto {
			NumPreguntas = config.NumPreguntas, CuestionarioPos = config.CuestionarioPos,
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

	public void ResolverPreguntas(ConfigExamen config, FlujoExamenDto flujo) {
		var respondidas = flujo.Pasos.Count(x => x is { Accion: "pregunta", Ejecutado: true });

		var next = Algoritmo.SiguienteAsignacion(respondidas, flujo.Score);
		if (next != null) {
			AsignarPreguntas(flujo, next);
		}
	}

	public void AsignarPreguntas(FlujoExamenDto flujo, Step pasoAlgo) {
		if (!pasoAlgo.DebeAsignar)
			return;
		var idsUsados = flujo.Pasos
			.Where(x => x.Accion == "pregunta")
			.Select(x => x.EntidadId).ToList();
		var cuenta = idsUsados.Count;
		// TODO control num preguntas
		var lista = new List<PasoExamen>();
		foreach (var dificultad in pasoAlgo.TomarPreguntas) {
			if (cuenta >= flujo.NumPreguntas)
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
			cuenta++;
		}
		if (lista.Count == 0)
			return;
		// randomizar la lista aun mas, comprobar con el flujo?
		lista = lista.OrderBy(_ => Rng.Next()).ToList();
		flujo.Pasos.AddRange(lista);
	}

	public Pregunta? SiguientePregunta(string dificultad, List<int> excluir) {
		// ojo, esto en teoria devuelve elementos aleatorios, probar
		// esta desgracia es necesaria por que EF no es flexible, asdjajsdajhjs

		var builder = new SqlBuilder();
		builder.Select("id, nombre, dificultad, legitimo")
			.Where("dificultad = @d", new { d = dificultad })
			.OrderBy("random()");

		if (excluir.Count > 0) {
			var inParams = DbHelpers.inParameters(excluir);
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