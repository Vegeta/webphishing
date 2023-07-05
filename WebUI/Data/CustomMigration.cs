using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;
using WebUI.Data;

namespace WebUI.Migrations {
	[DbContext(typeof(DbPhishing))]
	[Migration("CustomMigration_Usuario")]
	public partial class CustomMigration : Migration {

		protected override void Up(MigrationBuilder migrationBuilder) {
			var fecha = DateTime.Now.ToString("s");
			migrationBuilder.Sql($"insert into codigo_clase (key, fecha_creacion) values ('test', '${fecha}');");

			migrationBuilder.Sql(@"create view v_respuestas as
			select r.*, 
			test.perfil_id,
			test.fecha,
			test.tiempo_total,
			test.tiempo_promedio,
			test.tasa_exito,
			test.tasa_error,
			test.cuestionario
			from respuesta r
			join prueba_perfil test on r.prueba_id = test.id;
			");

			var host = Program.config();

			using (var scope = host.Services.CreateScope()) {
				var db = scope.ServiceProvider.GetRequiredService<DbPhishing>();
				db.Add(new CodigoClase {
					Key = "otro",
					FechaCreacion = DateTime.Now
				});
				db.SaveChanges();
			}

		}

		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.Sql("drop view v_respuestas;");
			base.Down(migrationBuilder);
		}
	}
}