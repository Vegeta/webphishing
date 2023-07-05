using Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Persistencia;

public partial class AppDbContext : DbContext {
	public const string ConnStringName = "phishingDb";

	public AppDbContext() { }

	public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options) { }

	public virtual DbSet<Auditoria> Auditoria { get; set; } = default!;

	public virtual DbSet<Cuestionario> Cuestionario { get; set; } = default!;

	public virtual DbSet<Examen> Examen { get; set; } = default!;

	public virtual DbSet<ExamenPregunta> ExamenPregunta { get; set; } = default!;

	public virtual DbSet<Parametro> Parametro { get; set; } = default!;

	public virtual DbSet<Perfil> Perfil { get; set; } = default!;

	public virtual DbSet<Persona> Persona { get; set; } = default!;

	public virtual DbSet<Pregunta> Pregunta { get; set; } = default!;

	public virtual DbSet<SesionGrupo> SesionGrupo { get; set; } = default!;

	public virtual DbSet<SesionPersona> SesionPersona { get; set; } = default!;

	public virtual DbSet<SesionRespuesta> SesionRespuesta { get; set; } = default!;

	public virtual DbSet<Usuario> Usuario { get; set; } = default!;

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		modelBuilder.Entity<Auditoria>(entity => {
			entity.HasKey(e => e.Id).HasName("log_evento_pkey");

			entity.Property(e => e.Id).UseIdentityAlwaysColumn();
		});

		modelBuilder.Entity<Cuestionario>(entity => {
			entity.HasKey(e => e.Id).HasName("cuestionario_pkey");

			entity.Property(e => e.Id).UseIdentityAlwaysColumn();
		});

		modelBuilder.Entity<Examen>(entity => { entity.HasKey(e => e.Id).HasName("examen_pkey"); });

		modelBuilder.Entity<ExamenPregunta>(entity => {
			entity.HasKey(e => e.Id).HasName("examen_pregunta_pkey");

			entity.HasOne(d => d.Examen).WithMany(p => p.ExamenPregunta).HasConstraintName("fk_ex_examen");

			entity.HasOne(d => d.Pregunta).WithMany(p => p.ExamenPregunta).HasConstraintName("fk_ex_pregunta");
		});

		modelBuilder.Entity<Parametro>(entity => {
			entity.HasKey(e => e.Id).HasName("parametros_pkey");

			entity.Property(e => e.Id).UseIdentityAlwaysColumn();
		});

		modelBuilder.Entity<Perfil>(entity => {
			entity.HasKey(e => e.Id).HasName("rol_pkey");

			entity.Property(e => e.Id).UseIdentityAlwaysColumn();
		});

		modelBuilder.Entity<Persona>(entity => {
			entity.HasKey(e => e.Id).HasName("persona_pkey");

			entity.HasOne(d => d.Usuario).WithMany(p => p.Persona)
				.OnDelete(DeleteBehavior.SetNull)
				.HasConstraintName("fk_per_usuario");
		});

		modelBuilder.Entity<Pregunta>(entity => { entity.HasKey(e => e.Id).HasName("ejercicio_pkey"); });

		modelBuilder.Entity<SesionGrupo>(entity => {
			entity.HasKey(e => e.Id).HasName("sesion_grupo_pkey");

			entity.Property(e => e.Id).UseIdentityAlwaysColumn();
			entity.Property(e => e.CodigoAcceso).HasComment("codigo acceso ui");
			entity.Property(e => e.Token).HasComment("sync token");
			entity.Property(e => e.UrlSlug).HasComment("codigo acceso web");

			entity.HasOne(d => d.Examen).WithMany(p => p.SesionGrupo)
				.OnDelete(DeleteBehavior.Cascade)
				.HasConstraintName("fk_sesg_examen");
		});

		modelBuilder.Entity<SesionPersona>(entity => {
			entity.HasKey(e => e.Id).HasName("persona_examen_pkey");

			entity.Property(e => e.Id).UseIdentityAlwaysColumn();

			entity.HasOne(d => d.Cuestionario).WithMany(p => p.SesionPersona)
				.OnDelete(DeleteBehavior.SetNull)
				.HasConstraintName("fk_res_cuestionario");

			entity.HasOne(d => d.Examen).WithMany(p => p.SesionPersona)
				.OnDelete(DeleteBehavior.SetNull)
				.HasConstraintName("fk_ses_examen");

			entity.HasOne(d => d.Grupo).WithMany(p => p.SesionPersona)
				.OnDelete(DeleteBehavior.Cascade)
				.HasConstraintName("fk_grupo");

			entity.HasOne(d => d.Persona).WithMany(p => p.SesionPersona)
				.OnDelete(DeleteBehavior.Cascade)
				.HasConstraintName("fk_ses_persona");
		});

		modelBuilder.Entity<SesionRespuesta>(entity => {
			entity.HasKey(e => e.Id).HasName("sesion_respuesta_pkey");

			entity.Property(e => e.Id).UseIdentityAlwaysColumn();

			entity.HasOne(d => d.Pregunta).WithMany(p => p.SesionRespuesta)
				.OnDelete(DeleteBehavior.SetNull)
				.HasConstraintName("fk_ses_pregunta");

			entity.HasOne(d => d.Sesion).WithMany(p => p.SesionRespuesta).HasConstraintName("fk_ses_examen");
		});

		modelBuilder.Entity<Usuario>(entity => {
			entity.HasKey(e => e.Id).HasName("usuario_pkey");

			entity.Property(e => e.Id).UseIdentityAlwaysColumn();

			entity.HasOne(d => d.Perfil).WithMany(p => p.Usuario)
				.OnDelete(DeleteBehavior.SetNull)
				.HasConstraintName("fk_perfil");
		});

		OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);



}
