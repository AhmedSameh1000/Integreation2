using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Integration.data.Migrations
{
    /// <inheritdoc />
    public partial class Removelocal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLocal",
                table: "dataBases");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLocal",
                table: "dataBases",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
