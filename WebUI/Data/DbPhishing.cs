using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WebUI.Data {
	public class DbPhishing : DbContext {
		public const string connStringName = "phishingDb";

		public DbPhishing() {
		}

		public DbPhishing(DbContextOptions options) : base(options) { }

		//public DbSet<Perfil> perfiles => Set<Perfil>();
		public DbSet<Perfil> perfiles { get; set; } = default!;

		public DbSet<CodigoClase> codigos { get; set; } = default!;
		public DbSet<PruebaPerfil> pruebas { get; set; } = default!;
		public DbSet<Respuesta> respuestas { get; set; } = default!;

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			base.OnConfiguring(optionsBuilder);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder);
		}

		public PruebaPerfil? PrimeraPrueba(int perfilId) {
			if (perfilId == 0)
				return null;
			return pruebas.Where(x => x.PerfilId == perfilId).First();
		}
	}

}
