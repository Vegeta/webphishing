using AutoMapper;
using Infraestructura.Persistencia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Webapp.Controllers;

namespace Webapp.Areas.Manage.Controllers;

public class ResultadosController : BaseAdminController {
	private readonly AppDbContext _db;
	private readonly IMapper _mapper;

	public ResultadosController(AppDbContext db, IMapper mapper) {
		_db = db;
		_mapper = mapper;
	}

	public IActionResult Index() {
		return View();
	}

}