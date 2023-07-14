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

}
