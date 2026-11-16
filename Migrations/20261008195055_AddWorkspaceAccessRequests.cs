using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollabSpace.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceAccessRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkspaceAccessRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    RequesterUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceAccessRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspaceAccessRequests_AspNetUsers_RequesterUserId",
                        column: x => x.RequesterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkspaceAccessRequests_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceAccessRequests_RequesterUserId",
                table: "WorkspaceAccessRequests",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceAccessRequests_WorkspaceId",
                table: "WorkspaceAccessRequests",
                column: "WorkspaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkspaceAccessRequests");
        }
    }
}
