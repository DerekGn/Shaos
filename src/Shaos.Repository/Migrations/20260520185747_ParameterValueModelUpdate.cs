using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shaos.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ParameterValueModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseParameterValues_BaseParameters_FloatParameterValue_ParameterId",
                table: "BaseParameterValues");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseParameterValues_BaseParameters_IntParameterValue_ParameterId",
                table: "BaseParameterValues");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseParameterValues_BaseParameters_StringParameterValue_ParameterId",
                table: "BaseParameterValues");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseParameterValues_BaseParameters_UIntParameterValue_ParameterId",
                table: "BaseParameterValues");

            migrationBuilder.DropIndex(
                name: "IX_BaseParameterValues_FloatParameterValue_ParameterId",
                table: "BaseParameterValues");

            migrationBuilder.DropIndex(
                name: "IX_BaseParameterValues_IntParameterValue_ParameterId",
                table: "BaseParameterValues");

            migrationBuilder.DropIndex(
                name: "IX_BaseParameterValues_StringParameterValue_ParameterId",
                table: "BaseParameterValues");

            migrationBuilder.DropIndex(
                name: "IX_BaseParameterValues_UIntParameterValue_ParameterId",
                table: "BaseParameterValues");

            migrationBuilder.DropColumn(
                name: "FloatParameterValue_ParameterId",
                table: "BaseParameterValues");

            migrationBuilder.DropColumn(
                name: "IntParameterValue_ParameterId",
                table: "BaseParameterValues");

            migrationBuilder.DropColumn(
                name: "StringParameterValue_ParameterId",
                table: "BaseParameterValues");

            migrationBuilder.DropColumn(
                name: "UIntParameterValue_ParameterId",
                table: "BaseParameterValues");

            migrationBuilder.AlterColumn<int>(
                name: "ParameterId",
                table: "BaseParameterValues",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ParameterId",
                table: "BaseParameterValues",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "FloatParameterValue_ParameterId",
                table: "BaseParameterValues",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntParameterValue_ParameterId",
                table: "BaseParameterValues",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StringParameterValue_ParameterId",
                table: "BaseParameterValues",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UIntParameterValue_ParameterId",
                table: "BaseParameterValues",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseParameterValues_FloatParameterValue_ParameterId",
                table: "BaseParameterValues",
                column: "FloatParameterValue_ParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseParameterValues_IntParameterValue_ParameterId",
                table: "BaseParameterValues",
                column: "IntParameterValue_ParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseParameterValues_StringParameterValue_ParameterId",
                table: "BaseParameterValues",
                column: "StringParameterValue_ParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseParameterValues_UIntParameterValue_ParameterId",
                table: "BaseParameterValues",
                column: "UIntParameterValue_ParameterId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseParameterValues_BaseParameters_FloatParameterValue_ParameterId",
                table: "BaseParameterValues",
                column: "FloatParameterValue_ParameterId",
                principalTable: "BaseParameters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseParameterValues_BaseParameters_IntParameterValue_ParameterId",
                table: "BaseParameterValues",
                column: "IntParameterValue_ParameterId",
                principalTable: "BaseParameters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseParameterValues_BaseParameters_StringParameterValue_ParameterId",
                table: "BaseParameterValues",
                column: "StringParameterValue_ParameterId",
                principalTable: "BaseParameters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseParameterValues_BaseParameters_UIntParameterValue_ParameterId",
                table: "BaseParameterValues",
                column: "UIntParameterValue_ParameterId",
                principalTable: "BaseParameters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
