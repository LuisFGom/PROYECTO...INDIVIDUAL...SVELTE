using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PuntoVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditoriaAccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auditorias_acciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NombreUsuario = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TipoAccion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Modulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    RegistroAfectadoId = table.Column<int>(type: "integer", nullable: true),
                    RegistroAfectadoDescripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaAccion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DireccionIP = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DatosAnteriores = table.Column<string>(type: "text", nullable: true),
                    DatosNuevos = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditorias_acciones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_auditorias_acciones_FechaAccion",
                table: "auditorias_acciones",
                column: "FechaAccion");

            migrationBuilder.CreateIndex(
                name: "IX_auditorias_acciones_Modulo",
                table: "auditorias_acciones",
                column: "Modulo");

            migrationBuilder.CreateIndex(
                name: "IX_auditorias_acciones_TipoAccion",
                table: "auditorias_acciones",
                column: "TipoAccion");

            migrationBuilder.CreateIndex(
                name: "IX_auditorias_acciones_UsuarioId",
                table: "auditorias_acciones",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditorias_acciones");
        }
    }
}
