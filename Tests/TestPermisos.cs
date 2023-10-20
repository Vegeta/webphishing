using Infraestructura.Servicios;

namespace Tests;

public class TestPermisos {
	[Fact]
	public void TestAutorizacion() {
		// probar las combinaciones de permisos para usar en atributos y en el menu
		var permisosAsignados = new List<string> { "preguntas", "auditoria" };
		var combinado = "preguntas|examenes, auditoria";

		var vale = UsuariosService.AutorizarPermisos(permisosAsignados, combinado);
		Assert.True(vale);

		var checkOr = "preguntas|examenes";
		vale = UsuariosService.AutorizarPermisos(permisosAsignados, checkOr);
		Assert.True(vale);

		var checkAnd1 = "preguntas, auditoria";
		vale = UsuariosService.AutorizarPermisos(permisosAsignados, checkAnd1);
		Assert.True(vale);
		
		var checkAnd2 = "preguntas, notiene";
		vale = UsuariosService.AutorizarPermisos(permisosAsignados, checkAnd2);
		Assert.False(vale);
		
		var notiene = "notiene";
		vale = UsuariosService.AutorizarPermisos(permisosAsignados, notiene);
		Assert.False(vale);
	}
}