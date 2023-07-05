using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebUI.Data;
using WebUI.Dto;

namespace WebUI.Controllers {
	[ApiController]
	[Route("api/codigo")]
	public class CodigoClaseController : ControllerBase {
		protected DbPhishing db;
		protected IMapper mapper;

		public CodigoClaseController(DbPhishing db, IMapper mapper) {
			this.db = db;
			this.mapper = mapper;
		}

		[HttpGet]
		public IEnumerable<CodigoDto> GetAll() {
			var lista = db.codigos.ToList();
			return mapper.Map<List<CodigoDto>>(lista);
		}

		[HttpPost]
		public IActionResult CrearCodigo(CodigoDto dto) {
			var cod = new CodigoClase { Key = dto.Key, FechaCreacion = DateTime.Now };
			db.codigos.Add(cod);
			db.SaveChanges();
			return Ok();
		}
	}
}