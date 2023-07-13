using Infraestructura.Persistencia;
using Domain.Entidades;
using System.Security.Cryptography;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Servicios;

public class RegistroService {
	private const string EstadoPendiente = "pendiente";
	private const string EstadoEjecutando = "ejecutando";
	private const string EstadoTerminado = "terminado";

	private readonly AppDbContext _db;

	public RegistroService(AppDbContext db) {
		_db = db;
	}

	public OperationResult<Persona> RegistrarPersona(Persona per) {
		var check = PorEmail(per.Email ?? "SN");
		if (check == null) {
			per.Creacion = DateTime.Now;
			_db.Persona.Add(per);
			check = per;
		} else {
			//update
			check.Nombre = per.Nombre;
			check.Apellido = per.Apellido;
			check.Actividad = per.Actividad;
			check.Edad = per.Edad;
			check.ExperienciaSeguridad = per.ExperienciaSeguridad;
			check.Genero = per.Genero;
			check.Key = per.Key;
			check.Ocupacion = per.Ocupacion;
		}
		check.Modificacion = DateTime.Now;
		_db.SaveChanges();
		return OperationResult<Persona>.Ok("", check);
	}

	public Persona? PorEmail(string email) {
		return _db.Persona.FirstOrDefault(x => x.Email == email);
	}

	/// <summary>
	/// Creates a cryptographically secure random key string.
	/// </summary>
	/// <param name="count">The number of bytes of random values to create the string from</param>
	/// <returns>A secure random string</returns>
	public static string CreateSecureRandomString(int count = 32) =>
		Convert.ToBase64String(RandomNumberGenerator.GetBytes(count));

	public SesionCreada IniciarSesionIndividual(Persona per) {

		var check = _db.SesionPersona.FirstOrDefault(x => x.PersonaId == per.Id && x.Estado == EstadoPendiente);
		if (check != null) {
			var f = check.GetSesionFlujo();
			return new SesionCreada(check, f);
		}

		var examen = _db.Examen
			.AsNoTracking()
			.Include(x => x.ExamenPregunta)
			.FirstOrDefault(x => x.Tipo == TipoExamen.Predeterminado);
		if (examen == null) {
			return new SesionCreada(null, null, "No existe un examen predeterminado");
		}

		var token = RegistroService.CreateSecureRandomString();
		var s = new SesionPersona {
			Estado = EstadoPendiente,
			Token = token,
			Nombre = per.NombreCompleto,
			ExamenId = examen.Id,
			PersonaId = per.Id,
			FechaActividad = DateTime.Now,
			Tipo = "simple",
		};

		var config = FlujoExamen.InitFlujo(examen);

		if (config.CuestionarioPos.HasValue) {
			var cues = _db.Cuestionario.Select(x => new {
				id = x.Id
			}).FirstOrDefault();
			s.CuestionarioId = cues?.id;
		}
		s.Flujo = JSON.Stringify(config);

		_db.SesionPersona.Add(s);
		_db.SaveChanges();

		return new SesionCreada(s, config);
	}



}

public class SesionCreada {
	public SesionPersona? Sesion { get; set; }
	public SesionFlujoWeb? Flujo { get; set; }
	public string Error { get; set; }

	public SesionCreada(SesionPersona? sesion, SesionFlujoWeb? flujo, string error = "") {
		Sesion = sesion;
		Flujo = flujo;
		Error = error;
	}
}

public class SesionFlujoWeb {
	public int Respuestas { get; set; } = 0;
	public int? CuestionarioPos { get; set; }
	public int Decision { get; set; } = 0;
	public List<PreguntaWeb> Lista { get; set; } = new();
	public List<PreguntaWeb> EnCola { get; set; } = new();

	public PreguntaWeb? Siguiente() {
		if (EnCola.Count == 0)
			return null;
		return Respuestas >= EnCola.Count ? null : EnCola[Respuestas];
	}

}

public class PreguntaWeb {
	public int Id { get; set; }
	public string Dificultad { get; set; } = "";
	public bool Usada { get; set; } = false;
}