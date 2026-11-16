using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class OneActiveBookingPerStation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChargeBookings_StationId",
                table: "ChargeBookings");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ChargeBookings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_OneActiveBookingPerStation",
                table: "ChargeBookings",
                column: "StationId",
                unique: true,
                filter: "[Status] IN ('Held', 'Confirmed', 'Charging')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OneActiveBookingPerStation",
                table: "ChargeBookings");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ChargeBookings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_ChargeBookings_StationId",
                table: "ChargeBookings",
                column: "StationId");
        }
    }
}
