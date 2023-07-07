using Infraestructura.Persistencia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entidades;
using System.Runtime.ConstrainedExecution;
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
			FechaActividad = DateTime.Now
		};



		var config = new SesionFlujo();
		config.Originales = examen.ExamenPregunta.Select(x => x.PreguntaId).ToList(); // sacar ids
		config.Propuestas.Add(config.Originales[0]); // la primera pregunta
		config.PreguntaActual = 0;

		if (examen.CuestionarioPos.HasValue) {
			var cues = _db.Cuestionario.Select(x => new {
				id = x.Id
			}).FirstOrDefault();
			if (cues != null) {
				s.CuestionarioId = cues.id;
				config.CuestionarioPos = examen.CuestionarioPos;
			}
		}


		s.Flujo = JSON.Stringify(config);

		_db.SesionPersona.Add(s);
		_db.SaveChanges();

		return new SesionCreada(s, config);
	}



}

public class SesionCreada {
	public SesionPersona? Sesion { get; set; }
	public SesionFlujo? Flujo { get; set; }
	public string Error { get; set; }

	public SesionCreada(SesionPersona? sesion, SesionFlujo? flujo, string error = "") {
		Sesion = sesion;
		Flujo = flujo;
		Error = error;
	}
}

public class SesionFlujo {
	public int PreguntaActual { get; set; }
	public int? CuestionarioPos { get; set; }
	public List<int> Originales { get; set; } = new();
	public List<int> Propuestas { get; set; } = new();
}