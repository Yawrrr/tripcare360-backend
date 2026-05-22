using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tripcare360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuredAge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InsuredAge",
                table: "Claims",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsuredAge",
                table: "Claims");
        }
    }
}
