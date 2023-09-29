using Domain;

namespace Infraestructura.Examenes.Asignacion {
	/// <summary>
	/// Esta clase tiene el algoritmo de asignacion de siguientes preguntas de acuerdo a
	/// la dificultad y condiciones de puntaje. Tiene un limite de 10 preguntas pero se puede
	/// adaptar para mas si se toma en trozos de 10 en 10 y se ajusta el score.
	///
	/// Los pasos en el metodo Init estan basados directamente en el trabajo original de la tesis
	/// de pregrado usada para construir el prototipo
	/// </summary>
	public class AlgoritmoAsignacion {
		private StepBuilder _config;
		public const int MaxPreguntas = 10;

		public AlgoritmoAsignacion() {
			_config = Init();
		}

		public StepBuilder Init() {
			var config = new StepBuilder();
			config.Asignar(0, DificultadPregunta.Facil, DificultadPregunta.Medio, DificultadPregunta.Dificil)
				.Switch(3, op => {
					op.CondScore(x => x >= 5, DificultadPregunta.Medio, DificultadPregunta.Dificil, DificultadPregunta.Dificil);
					op.CondScore(x => x is >= 3 and < 5, DificultadPregunta.Medio, DificultadPregunta.Medio, DificultadPregunta.Dificil);
					op.CondScore(x => true, DificultadPregunta.Facil, DificultadPregunta.Facil, DificultadPregunta.Medio);
				})
				.Switch(6, op => {
					op.CondScore(x => x >= 6, DificultadPregunta.Medio, DificultadPregunta.Medio, DificultadPregunta.Dificil,
						DificultadPregunta.Dificil);
					op.CondScore(x => x is >= 3 and < 6, DificultadPregunta.Medio, DificultadPregunta.Medio, DificultadPregunta.Dificil,
						DificultadPregunta.Facil);
					op.CondScore(x => true, DificultadPregunta.Medio, DificultadPregunta.Medio, DificultadPregunta.Facil,
						DificultadPregunta.Facil);
				});
			return config;
		}

		public Step? SiguienteAsignacion(int numRespuestas, int score) {
			foreach (var item in _config.Pasos) {
				var paso = item.Evaluar(numRespuestas, score);
				if (paso != null)
					return paso;
			}
			return null;
		}
	}

	public class Step {
		public int CheckNum { get; set; } = 0;
		public string Condicion { get; set; } = "";
		public Func<int, bool>? CondScore { get; set; }
		public List<string> TomarPreguntas { get; set; } = new();
		public List<Step> Deciciones { get; set; } = new();

		public bool DebeAsignar => TomarPreguntas.Count > 0;
		public bool EsSwitch => Deciciones.Count > 0;

		public bool TieneCond => CondScore != null;
		public bool Contar => CheckNum > 0;

		public Step? Evaluar(int respuestas, int score) {
			if (CheckNum != respuestas)
				return null;
			if (CondScore != null) {
				if (!CondScore(score))
					return null;
			}
			if (EsSwitch) {
				return EvaluarSwitch(score);
			}
			return this;
		}

		public bool EvaluarScore(int score) {
			return CondScore != null && CondScore(score);
		}

		public Step? EvaluarSwitch(int score) {
			return Deciciones.FirstOrDefault(step => step.EvaluarScore(score));
		}
	}

	public class StepBuilder {
		public List<Step> Pasos { get; set; } = new List<Step>();

		public StepBuilder Asignar(params string[] dificultad) {
			Pasos.Add(new Step {
				TomarPreguntas = dificultad.ToList()
			});
			return this;
		}

		public StepBuilder Asignar(int respuestas, params string[] dificultad) {
			Pasos.Add(new Step {
				CheckNum = respuestas,
				TomarPreguntas = dificultad.ToList()
			});
			return this;
		}

		public StepBuilder ContarRespuestas(int num) {
			Pasos.Add(new Step { CheckNum = num });
			return this;
		}

		public StepBuilder Condicion(string cond, params string[] dificultad) {
			Pasos.Add(new Step {
				Condicion = cond,
				TomarPreguntas = dificultad.ToList()
			});
			return this;
		}

		public StepBuilder CondScore(Func<int, bool> expression, params string[] dificultad) {
			Pasos.Add(new Step {
				CondScore = expression,
				TomarPreguntas = dificultad.ToList()
			});
			return this;
		}

		public StepBuilder Switch(Action<StepBuilder> opciones) {
			if (opciones == null)
				throw new ArgumentNullException();
			var inner = new StepBuilder();
			opciones(inner);
			Pasos.Add(new Step {
				Deciciones = inner.Pasos
			});
			return this;
		}

		public StepBuilder Switch(int respuestas, Action<StepBuilder> opciones) {
			if (opciones == null)
				throw new ArgumentNullException();
			var inner = new StepBuilder();
			opciones(inner);
			Pasos.Add(new Step {
				CheckNum = respuestas,
				Deciciones = inner.Pasos
			});
			return this;
		}
	}
}