using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tripcare360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimInsuredEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InsuredEmail",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsuredEmail",
                table: "Claims");
        }
    }
}
