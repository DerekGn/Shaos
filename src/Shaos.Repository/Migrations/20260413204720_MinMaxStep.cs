using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shaos.Repository.Migrations
{
    /// <inheritdoc />
    public partial class MinMaxStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IntParameter_Step",
                table: "BaseParameters",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Step",
                table: "BaseParameters",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UIntParameter_Step",
                table: "BaseParameters",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntParameter_Step",
                table: "BaseParameters");

            migrationBuilder.DropColumn(
                name: "Step",
                table: "BaseParameters");

            migrationBuilder.DropColumn(
                name: "UIntParameter_Step",
                table: "BaseParameters");
        }
    }
}
