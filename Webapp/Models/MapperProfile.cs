using AutoMapper;
using Domain.Entidades;
using Webapp.Models;

public class MapperProfile : Profile {
	public MapperProfile() {
		CreateMap<Pregunta, PreguntaModelWeb>();
		CreateMap<PreguntaModelWeb, Pregunta>()
			.ForMember(x => x.Id, o => o.Ignore())
			.ForMember(x => x.Adjuntos, o => o.Ignore());

		CreateMap<Examen, ExamenModelWeb>();
		CreateMap<ExamenModelWeb, Examen>()
			.ForMember(x => x.Id, o => o.Ignore())
			.ForMember(x => x.Creacion, o => o.Ignore())
			.ForMember(x => x.Modificacion, o => o.Ignore());

		// https://www.paraesthesia.com/archive/2022/02/14/automapper-nullable-datetime-constructors/

		CreateMap<UsuarioModel, Usuario>()
			.ForMember(x => x.Creacion, o => o.Ignore())
			.ForMember(x => x.Modificacion, o => o.Ignore())
			.ForMember(x => x.Password, o => o.Ignore());
		CreateMap<Usuario, UsuarioModel>()
			.ForMember(x => x.Password, o => o.Ignore())
			.ConstructUsing((input, context) => new UsuarioModel()); // necesario para cambios datetime

		CreateMap<PerfilModelWeb, Perfil>()
			.ForMember(x => x.Permisos, o => o.Ignore());
		CreateMap<Perfil, PerfilModelWeb>()
			.ForMember(x => x.Permisos, o => o.Ignore());

	}
}