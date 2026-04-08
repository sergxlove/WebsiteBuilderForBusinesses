using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebsiteBuilderForBusinesses.DataAccess.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DateOpen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TextHtml = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Login = table.Column<string>(type: "text", nullable: false),
                    HashPassword = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "HashPassword", "Login", "Role" },
                values: new object[,]
                {
                    { new Guid("438e452d-a8db-4c7a-b4b8-4ada7cda7d76"), "$2a$11$cXzJITgtUiw/4cWi1y.XH.xHG01Bwyj53m3w2HOU4nWIrOk24AgXG", "admin@mail.ru", "admin" },
                    { new Guid("c93c0103-14f1-43b0-9a0c-5862624bbd9b"), "$2a$11$zD.sI1v4tUcoNEHXYH/vduhMU8kiFMXJh5yURxIkE5S2emBiS156i", "user@mail.ru", "user" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
