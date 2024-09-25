using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WPass.Core.Migrations
{
    /// <inheritdoc />
    public partial class OneShot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BrowserElements",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrowserElements", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "CHAR", maxLength: 36, nullable: false),
                    Username = table.Column<string>(type: "NVARCHAR", maxLength: 50, nullable: false),
                    EncryptedPassword = table.Column<string>(type: "TEXT", nullable: false),
                    IsDefault = table.Column<bool>(type: "BIT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "VARCHAR", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Websites",
                columns: table => new
                {
                    Id = table.Column<string>(type: "CHAR", maxLength: 36, nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    EntryId = table.Column<string>(type: "CHAR", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Websites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Websites_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Websites_EntryId",
                table: "Websites",
                column: "EntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrowserElements");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Websites");

            migrationBuilder.DropTable(
                name: "Entries");
        }
    }
}
