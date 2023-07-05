using AutoMapper;
using WebUI.Data;
using WebUI.Dto;

namespace WebUI.Utils {
	public class MapperProfile : Profile {
		public MapperProfile() {
			CreateMap<CodigoClase, CodigoDto>();
		}
	}
}
