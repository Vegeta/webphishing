﻿using Infraestructura.Persistencia;
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

public class EstadisticasPreguntas {
	private readonly AppDbContext _db;

	public EstadisticasPreguntas(AppDbContext db) {
		_db = db;
	}

	public void TopDificiiles() {
		// _db.Database.GetDbConnection().QueryAsync()
	}

	public void EstadisticasExamenes() {
	}
}