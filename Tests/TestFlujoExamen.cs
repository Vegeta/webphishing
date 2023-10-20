using Domain;
using Domain.Entidades;
using Infraestructura.Examenes;
using Infraestructura.Examenes.Asignacion;
using Infraestructura.Persistencia;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Tests;

public class TestFlujoExamen : TestBed<TestFixture> {
	public TestFlujoExamen(ITestOutputHelper testOutputHelper, TestFixture fixture) : base(testOutputHelper, fixture) {
	}

	[Fact]
	public void TestAsignador() {
		var manager = GetManager();
		var db = _fixture.GetService<AppDbContext>(_testOutputHelper);

		var config = new ConfigExamen {
			NumPreguntas = 10,
			CuestionarioPos = 5,
			Aleatorio = true,
			Tipo = TipoExamen.Predeterminado,
		};

		var as1 = manager!.GetAsignador(config);
		Assert.IsType<FlujoAleatorio>(as1);

		// prueba asignador con examen particular
		var examen = db!.Examen.First();
		config.IdExamen = examen.Id;
		config.Tipo = TipoExamen.Personalizado;
		config.Aleatorio = false;

		var as2 = manager.GetAsignador(config);
		Assert.IsType<FlujoParticular>(as2);
	}

	private ManagerExamen? GetManager() {
		return _fixture.GetService<ManagerExamen>(_testOutputHelper);
	}

	[Fact]
	public void TestExamenAleatorio() {
		// probar examen aleatorio con mas preguntas de lo que jala el algoritmo
		var manager = GetManager();

		var config = new ConfigExamen {
			NumPreguntas = 12,
			CuestionarioPos = 5,
			Aleatorio = true,
			Tipo = TipoExamen.Predeterminado,
		};

		var sesion = new SesionPersona {
			FechaExamen = DateTime.Now
		};

		var flujo = manager!.CrearFlujo(config);
		flujo.Inicio = DateTime.Now;

		for (var i = 0; i < flujo.NumPreguntas + 1; i++) {
			var paso = manager.RespuestaActual(config, flujo, "phishing", 0.5f);
			WriteLine(i + ", " + paso.Accion);
			if (flujo.EsFin)
				break;
		}

		// ver numero de preguntas
		Assert.NotEmpty(flujo.Pasos);
		Assert.True(flujo.NumPreguntas == config.NumPreguntas);
		// ver si asigno mas alla de lo del algoritmo
		Assert.True(flujo.NumPreguntas > AlgoritmoAsignacion.MaxPreguntas);
		// ver inclusion cuestionario
		Assert.True(flujo.Pasos.Count == flujo.NumPreguntas + 1);
		Assert.True(flujo.Pasos[config.CuestionarioPos.Value - 1].Accion == "cuestionario");

		flujo.Fin = DateTime.Now;
		manager.FinalizarSesion(flujo, sesion);
		
		// check calculos
		Assert.NotNull(sesion.AvgTiempo);
		Assert.NotNull(sesion.AvgScore);
	}

	private void WriteLine(object? o) {
		switch (o) {
			case null:
				_testOutputHelper.WriteLine("NULL");
				return;
			case string:
				_testOutputHelper.WriteLine(o.ToString());
				break;
			default:
				_testOutputHelper.WriteLine("{0}", o);
				break;
		}
	}

	[Fact]
	public void TestCuestionario() {
		var manager = GetManager();

		var config = new ConfigExamen {
			NumPreguntas = 3,
			CuestionarioPos = 1,
			Aleatorio = true,
			Tipo = TipoExamen.Predeterminado,
		};

		var flujo = manager!.CrearFlujo(config);
		Assert.True(flujo.Pasos[0].Accion == "cuestionario");

		config.CuestionarioPos = 5; // fuera del rango
		var flujo2 = manager.CrearFlujo(config);
		Assert.DoesNotContain(flujo2.Pasos, x => x.Accion == "cuestionario");

		config.CuestionarioPos = 4; // al final
		var flujo3 = manager.CrearFlujo(config);
		Assert.True(flujo3.Pasos.Last().Accion == "cuestionario");

		config.CuestionarioPos = 2; // al al medio
		var flujo4 = manager.CrearFlujo(config);
		Assert.True(flujo4.Pasos[config.CuestionarioPos.Value - 1].Accion == "cuestionario");
	}
}