using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeadcountDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedByToDailyHeadcount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "DailyHeadcounts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "DailyHeadcounts");
        }
    }
}
