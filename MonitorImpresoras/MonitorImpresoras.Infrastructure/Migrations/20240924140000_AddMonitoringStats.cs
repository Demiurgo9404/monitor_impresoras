using Microsoft.EntityFrameworkCore.Migrations;

namespace MonitorImpresoras.Infrastructure.Migrations
{
    public partial class AddMonitoringStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitoringStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TotalPrints = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalCopies = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalScans = table.Column<int>(type: "INTEGER", nullable: false),
                    TonerLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringStats", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonitoringStats");
        }
    }
}