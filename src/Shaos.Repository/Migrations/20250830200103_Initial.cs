using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shaos.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogLevelSwitches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogLevelSwitches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlugIns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlugIns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlugInInformations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssemblyFileName = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    AssemblyVersion = table.Column<string>(type: "TEXT", nullable: false),
                    Directory = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    PackageFileName = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    HasConfiguration = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasLogger = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlugInId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlugInInformations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlugInInformations_PlugIns_PlugInId",
                        column: x => x.PlugInId,
                        principalTable: "PlugIns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlugInInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Configuration = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlugInId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlugInInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlugInInstances_PlugIns_PlugInId",
                        column: x => x.PlugInId,
                        principalTable: "PlugIns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Device",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BatteryLevel = table.Column<uint>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PlugInInstanceId = table.Column<int>(type: "INTEGER", nullable: true),
                    SignalLevel = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Device", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Device_PlugInInstances_PlugInInstanceId",
                        column: x => x.PlugInInstanceId,
                        principalTable: "PlugInInstances",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BaseParameter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ParameterType = table.Column<int>(type: "INTEGER", nullable: false),
                    Units = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    Value = table.Column<bool>(type: "INTEGER", nullable: true),
                    FloatParameter_Value = table.Column<float>(type: "REAL", nullable: true),
                    IntParameter_Value = table.Column<int>(type: "INTEGER", nullable: true),
                    StringParameter_Value = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UIntParameter_Value = table.Column<uint>(type: "INTEGER", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseParameter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseParameter_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceUpdate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    BatteryLevel = table.Column<uint>(type: "INTEGER", nullable: true),
                    SignalLevel = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceUpdate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceUpdate_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseParameter_DeviceId",
                table: "BaseParameter",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Device_PlugInInstanceId",
                table: "Device",
                column: "PlugInInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceUpdate_DeviceId",
                table: "DeviceUpdate",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_LogLevelSwitches_Name",
                table: "LogLevelSwitches",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlugInInformations_PlugInId",
                table: "PlugInInformations",
                column: "PlugInId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlugInInstances_Name",
                table: "PlugInInstances",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlugInInstances_PlugInId",
                table: "PlugInInstances",
                column: "PlugInId");

            migrationBuilder.CreateIndex(
                name: "IX_PlugIns_Name",
                table: "PlugIns",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaseParameter");

            migrationBuilder.DropTable(
                name: "DeviceUpdate");

            migrationBuilder.DropTable(
                name: "LogLevelSwitches");

            migrationBuilder.DropTable(
                name: "PlugInInformations");

            migrationBuilder.DropTable(
                name: "Device");

            migrationBuilder.DropTable(
                name: "PlugInInstances");

            migrationBuilder.DropTable(
                name: "PlugIns");
        }
    }
}
