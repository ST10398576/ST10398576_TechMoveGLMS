using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ST10398576_TechMoveGLMS.Migrations
{
    /// <inheritdoc />
    public partial class EditedDBTableValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ServiceRequests",
                newName: "ServiceStatus");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "ServiceRequests",
                newName: "ServiceDescription");

            migrationBuilder.RenameColumn(
                name: "Cost",
                table: "ServiceRequests",
                newName: "ServiceCost");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Contracts",
                newName: "ContractStatus");

            migrationBuilder.RenameColumn(
                name: "ServiceLevel",
                table: "Contracts",
                newName: "ContractServiceLevel");

            migrationBuilder.RenameColumn(
                name: "Region",
                table: "Clients",
                newName: "ClientRegion");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Clients",
                newName: "ClientName");

            migrationBuilder.RenameColumn(
                name: "ContactDetails",
                table: "Clients",
                newName: "ClientContactDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ServiceStatus",
                table: "ServiceRequests",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "ServiceDescription",
                table: "ServiceRequests",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "ServiceCost",
                table: "ServiceRequests",
                newName: "Cost");

            migrationBuilder.RenameColumn(
                name: "ContractStatus",
                table: "Contracts",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "ContractServiceLevel",
                table: "Contracts",
                newName: "ServiceLevel");

            migrationBuilder.RenameColumn(
                name: "ClientRegion",
                table: "Clients",
                newName: "Region");

            migrationBuilder.RenameColumn(
                name: "ClientName",
                table: "Clients",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ClientContactDetails",
                table: "Clients",
                newName: "ContactDetails");
        }
    }
}
