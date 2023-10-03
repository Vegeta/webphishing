using Infraestructura.Persistencia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Reportes;

public class EstadisticasPrincipales {
	private readonly AppDbContext _db;

	public EstadisticasPrincipales(AppDbContext db) {
		_db = db;
	}

	public void TopDificiiles() {
		// _db.Database.GetDbConnection().QueryAsync()
	}

	public void EstadisticasExamenes() { }

	public dynamic HomeScreenStats() {
		var numSesiones = _db.Database.GetDbConnection()
			.QuerySingle<int>("select count(*) num from sesion_persona");
		var numPersonas = _db.Database.GetDbConnection()
			.QuerySingle<int>("select count(*) num from persona");

		return new {
			numSesiones,
			numPersonas
		};
	}

}