using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shaos.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlugIns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PrimaryKey_PlugInId", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NuGetFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlugInId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PrimaryKey_NuGetId", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NuGetFiles_PlugIns_PlugInId",
                        column: x => x.PlugInId,
                        principalTable: "PlugIns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlugInInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlugInId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PrimaryKey_PlugInInstanceId", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlugInInstances_PlugIns_PlugInId",
                        column: x => x.PlugInId,
                        principalTable: "PlugIns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NuGetFiles_PlugInId",
                table: "NuGetFiles",
                column: "PlugInId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlugInInstance_Name_Ascending",
                table: "PlugInInstances",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PlugInInstances_PlugInId",
                table: "PlugInInstances",
                column: "PlugInId");

            migrationBuilder.CreateIndex(
                name: "IX_PlugIn_Name_Ascending",
                table: "PlugIns",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NuGetFiles");

            migrationBuilder.DropTable(
                name: "PlugInInstances");

            migrationBuilder.DropTable(
                name: "PlugIns");
        }
    }
}
