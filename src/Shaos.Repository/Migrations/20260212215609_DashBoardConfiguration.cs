using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shaos.Repository.Migrations
{
    /// <inheritdoc />
    public partial class DashBoardConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParameterId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardConfigurations_BaseParameter_ParameterId",
                        column: x => x.ParameterId,
                        principalTable: "BaseParameter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardConfigurations_ParameterId",
                table: "DashboardConfigurations",
                column: "ParameterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardConfigurations");
        }
    }
}
