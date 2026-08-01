using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "medio_pago",
                columns: table => new
                {
                    id_medio_pago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_medio_pago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medio_pago", x => x.id_medio_pago);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id_role = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.id_role);
                });

            migrationBuilder.CreateTable(
                name: "sede_evento",
                columns: table => new
                {
                    id_sede_evento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_sede_evento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ubicacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sede_evento", x => x.id_sede_evento);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    apellido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    correo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: ""),
                    id_role = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_usuario_role",
                        column: x => x.id_role,
                        principalTable: "role",
                        principalColumn: "id_role");
                });

            migrationBuilder.CreateTable(
                name: "evento",
                columns: table => new
                {
                    id_evento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_evento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    fecha_evento = table.Column<DateTime>(type: "datetime", nullable: false),
                    hora_evento = table.Column<TimeOnly>(type: "time", nullable: false),
                    IdSede = table.Column<int>(type: "int", nullable: true),
                    SedeNavigationIdSedeEvento = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evento", x => x.id_evento);
                    table.ForeignKey(
                        name: "FK_evento_sede_evento_SedeNavigationIdSedeEvento",
                        column: x => x.SedeNavigationIdSedeEvento,
                        principalTable: "sede_evento",
                        principalColumn: "id_sede_evento");
                });

            migrationBuilder.CreateTable(
                name: "localidad",
                columns: table => new
                {
                    id_localidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_localidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    id_sede_evento = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_localidad", x => x.id_localidad);
                    table.ForeignKey(
                        name: "FK_localidad_sede_evento",
                        column: x => x.id_sede_evento,
                        principalTable: "sede_evento",
                        principalColumn: "id_sede_evento");
                });

            migrationBuilder.CreateTable(
                name: "factura",
                columns: table => new
                {
                    id_factura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    id_medio_pago = table.Column<int>(type: "int", nullable: false),
                    fecha_factura = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    numero_factura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    total = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_factura", x => x.id_factura);
                    table.ForeignKey(
                        name: "FK_factura_medio_pago",
                        column: x => x.id_medio_pago,
                        principalTable: "medio_pago",
                        principalColumn: "id_medio_pago");
                    table.ForeignKey(
                        name: "FK_factura_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "evento_localidad",
                columns: table => new
                {
                    id_evento_localidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_evento = table.Column<int>(type: "int", nullable: false),
                    id_localidad = table.Column<int>(type: "int", nullable: false),
                    precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    capacidad_disponible = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evento_localidad", x => x.id_evento_localidad);
                    table.ForeignKey(
                        name: "FK_evento",
                        column: x => x.id_evento,
                        principalTable: "evento",
                        principalColumn: "id_evento");
                    table.ForeignKey(
                        name: "FK_localidad",
                        column: x => x.id_localidad,
                        principalTable: "localidad",
                        principalColumn: "id_localidad");
                });

            migrationBuilder.CreateTable(
                name: "boleto",
                columns: table => new
                {
                    id_boleto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_evento_localidad = table.Column<int>(type: "int", nullable: false),
                    id_factura = table.Column<int>(type: "int", nullable: false),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    num_boleto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    fecha_compra = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boleto", x => x.id_boleto);
                    table.ForeignKey(
                        name: "FK_evento_localidad",
                        column: x => x.id_evento_localidad,
                        principalTable: "evento_localidad",
                        principalColumn: "id_evento_localidad");
                    table.ForeignKey(
                        name: "FK_factura",
                        column: x => x.id_factura,
                        principalTable: "factura",
                        principalColumn: "id_factura");
                    table.ForeignKey(
                        name: "FK_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateIndex(
                name: "IX_boleto_id_evento_localidad",
                table: "boleto",
                column: "id_evento_localidad");

            migrationBuilder.CreateIndex(
                name: "IX_boleto_id_factura",
                table: "boleto",
                column: "id_factura");

            migrationBuilder.CreateIndex(
                name: "IX_boleto_id_usuario",
                table: "boleto",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "UQ__boleto__5F58F54E312BB92F",
                table: "boleto",
                column: "num_boleto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_evento_SedeNavigationIdSedeEvento",
                table: "evento",
                column: "SedeNavigationIdSedeEvento");

            migrationBuilder.CreateIndex(
                name: "IX_evento_localidad_id_evento",
                table: "evento_localidad",
                column: "id_evento");

            migrationBuilder.CreateIndex(
                name: "IX_evento_localidad_id_localidad",
                table: "evento_localidad",
                column: "id_localidad");

            migrationBuilder.CreateIndex(
                name: "IX_factura_id_medio_pago",
                table: "factura",
                column: "id_medio_pago");

            migrationBuilder.CreateIndex(
                name: "IX_factura_id_usuario",
                table: "factura",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "UQ__factura__3DC4B241A96C28B6",
                table: "factura",
                column: "numero_factura",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_localidad_id_sede_evento",
                table: "localidad",
                column: "id_sede_evento");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_id_role",
                table: "usuario",
                column: "id_role");

            migrationBuilder.CreateIndex(
                name: "UQ__usuario__2A586E0B492B5F82",
                table: "usuario",
                column: "correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "boleto");

            migrationBuilder.DropTable(
                name: "evento_localidad");

            migrationBuilder.DropTable(
                name: "factura");

            migrationBuilder.DropTable(
                name: "evento");

            migrationBuilder.DropTable(
                name: "localidad");

            migrationBuilder.DropTable(
                name: "medio_pago");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "sede_evento");

            migrationBuilder.DropTable(
                name: "role");
        }
    }
}
