using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebUI.Migrations
{
    /// <inheritdoc />
    public partial class MiMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "codigo_clase",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_codigo_clase", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "perfil",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    apellido = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    profesion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    carrera = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    genero = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    edad = table.Column<int>(type: "integer", nullable: true),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_perfil", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prueba_perfil",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    perfil_id = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    tiempo_total = table.Column<float>(type: "real", nullable: true),
                    tiempo_promedio = table.Column<float>(type: "real", nullable: true),
                    tasa_exito = table.Column<int>(type: "integer", nullable: true),
                    tasa_error = table.Column<int>(type: "integer", nullable: true),
                    cuestionario = table.Column<string>(type: "json", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prueba_perfil", x => x.id);
                    table.ForeignKey(
                        name: "fk_prueba_perfil_perfil_perfil_id",
                        column: x => x.perfil_id,
                        principalTable: "perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "respuesta",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prueba_id = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: true),
                    orden = table.Column<int>(type: "integer", nullable: true),
                    focus = table.Column<List<List<string>>>(type: "json", nullable: true),
                    click = table.Column<List<List<string>>>(type: "json", nullable: true),
                    tiempo = table.Column<float>(type: "real", nullable: true),
                    comentario = table.Column<string>(type: "text", nullable: true),
                    texto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    tipo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_respuesta", x => x.id);
                    table.ForeignKey(
                        name: "fk_respuesta_prueba_perfil_prueba_id",
                        column: x => x.prueba_id,
                        principalTable: "prueba_perfil",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_prueba_perfil_perfil_id",
                table: "prueba_perfil",
                column: "perfil_id");

            migrationBuilder.CreateIndex(
                name: "ix_respuesta_prueba_id",
                table: "respuesta",
                column: "prueba_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "codigo_clase");

            migrationBuilder.DropTable(
                name: "respuesta");

            migrationBuilder.DropTable(
                name: "prueba_perfil");

            migrationBuilder.DropTable(
                name: "perfil");
        }
    }
}
