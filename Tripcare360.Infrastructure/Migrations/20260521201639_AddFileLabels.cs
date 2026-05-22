using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tripcare360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFileLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileLabels",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileLabels",
                table: "Claims");
        }
    }
}
