using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication8.Data.Migrations
{
    /// <inheritdoc />
    public partial class storetokeninsqlserver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Token = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.Token);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tokens");
        }
    }
}
