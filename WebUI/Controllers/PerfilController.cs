using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebUI.Data;
using WebUI.Dto;
using WebUI.Servicios;

namespace WebUI.Controllers {
	[ApiController]
	[Route("api/[controller]")]
	public class PerfilController : ControllerBase {
		protected Transformacion serv;
		protected IMapper mapper;

		public PerfilController(Transformacion serv, IMapper mapper) {
			this.serv = serv;
			this.mapper = mapper;
		}

		[HttpGet("{id}")]
		public IActionResult PorId(int id) {
			var perfil = serv.Db.Find<Perfil>(id);
			if (perfil == null) {
				return NotFound();
			}
			var dto = serv.Transformar(perfil, null);
			return Ok(dto);
		}

		[HttpGet]
		public List<PerfilDto> Perfiles() {
			var lista = serv.ListaPerfiles();
			return lista;
		}

		[HttpPost]
		public IActionResult Crear([FromBody] PerfilDto dto) {
			var p = serv.UpsertPerfil(dto);
			return Ok(p.Id);
		}

	}
}