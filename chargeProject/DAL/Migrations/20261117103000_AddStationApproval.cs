using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddStationApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "ChargingStations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Approved");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ChargingStations",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "ChargingStations");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "ChargingStations");
        }
    }
}