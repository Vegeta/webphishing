using Dapper;
using Domain.Entidades;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Servicios;

public class ConsultasExamen {
	private readonly AppDbContext _db;

	public ConsultasExamen(AppDbContext db) {
		_db = db;
	}

	public SesionPersona? SesionPorToken(string token) {
		return _db.SesionPersona.FirstOrDefault(x => x.Token == token);
	}

	public SesionPersona? SesionActualPersona(int personaId) {
		return _db.SesionPersona.FirstOrDefault(x => x.PersonaId == personaId
		                                             && (x.Estado == SesionPersona.EstadoPendiente || x.Estado == SesionPersona.EstadoEnCurso));
	}

	public IList<Pregunta> PreguntasRandom(int numero = 10) {
		return _db.Database.GetDbConnection()
			.Query<Pregunta>("select id, dificultad from pregunta order by RANDOM() limit " + numero)
			.ToList();
	}

	public SesionPersona CrearSesion(Persona per, string tipo) {
		var token = PasswordHash.CreateSecureRandomString();
		return new SesionPersona {
			Estado = SesionPersona.EstadoPendiente,
			Token = token,
			Nombre = per.NombreCompleto,
			PersonaId = per.Id,
			FechaActividad = DateTime.Now,
			Tipo = tipo,
		};
	}

}