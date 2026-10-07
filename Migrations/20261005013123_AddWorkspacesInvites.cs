using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollabSpace.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspacesInvites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceInvite_AspNetUsers_ReceiverUserId",
                table: "WorkspaceInvite");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceInvite_AspNetUsers_SenderUserId",
                table: "WorkspaceInvite");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceInvite_Workspace_WorkspaceId",
                table: "WorkspaceInvite");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceMember_AspNetUsers_UserId",
                table: "WorkspaceMember");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceMember_Workspace_WorkspaceId",
                table: "WorkspaceMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkspaceMember",
                table: "WorkspaceMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkspaceInvite",
                table: "WorkspaceInvite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Workspace",
                table: "Workspace");

            migrationBuilder.RenameTable(
                name: "WorkspaceMember",
                newName: "WorkspaceMembers");

            migrationBuilder.RenameTable(
                name: "WorkspaceInvite",
                newName: "WorkspaceInvites");

            migrationBuilder.RenameTable(
                name: "Workspace",
                newName: "Workspaces");

            migrationBuilder.RenameIndex(
                name: "IX_WorkspaceMember_UserId",
                table: "WorkspaceMembers",
                newName: "IX_WorkspaceMembers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkspaceInvite_WorkspaceId",
                table: "WorkspaceInvites",
                newName: "IX_WorkspaceInvites_WorkspaceId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkspaceInvite_SenderUserId",
                table: "WorkspaceInvites",
                newName: "IX_WorkspaceInvites_SenderUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkspaceInvite_ReceiverUserId",
                table: "WorkspaceInvites",
                newName: "IX_WorkspaceInvites_ReceiverUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkspaceMembers",
                table: "WorkspaceMembers",
                columns: new[] { "WorkspaceId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkspaceInvites",
                table: "WorkspaceInvites",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Workspaces",
                table: "Workspaces",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceInvites_AspNetUsers_ReceiverUserId",
                table: "WorkspaceInvites",
                column: "ReceiverUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceInvites_AspNetUsers_SenderUserId",
                table: "WorkspaceInvites",
                column: "SenderUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceInvites_Workspaces_WorkspaceId",
                table: "WorkspaceInvites",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceMembers_AspNetUsers_UserId",
                table: "WorkspaceMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceMembers_Workspaces_WorkspaceId",
                table: "WorkspaceMembers",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceInvites_AspNetUsers_ReceiverUserId",
                table: "WorkspaceInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceInvites_AspNetUsers_SenderUserId",
                table: "WorkspaceInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceInvites_Workspaces_WorkspaceId",
                table: "WorkspaceInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceMembers_AspNetUsers_UserId",
                table: "WorkspaceMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceMembers_Workspaces_WorkspaceId",
                table: "WorkspaceMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Workspaces",
                table: "Workspaces");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkspaceMembers",
                table: "WorkspaceMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkspaceInvites",
                table: "WorkspaceInvites");

            migrationBuilder.RenameTable(
                name: "Workspaces",
                newName: "Workspace");

            migrationBuilder.RenameTable(
                name: "WorkspaceMembers",
                newName: "WorkspaceMember");

            migrationBuilder.RenameTable(
                name: "WorkspaceInvites",
                newName: "WorkspaceInvite");

            migrationBuilder.RenameIndex(
                name: "IX_WorkspaceMembers_UserId",
                table: "WorkspaceMember",
                newName: "IX_WorkspaceMember_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkspaceInvites_WorkspaceId",
                table: "WorkspaceInvite",
                newName: "IX_WorkspaceInvite_WorkspaceId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkspaceInvites_SenderUserId",
                table: "WorkspaceInvite",
                newName: "IX_WorkspaceInvite_SenderUserId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkspaceInvites_ReceiverUserId",
                table: "WorkspaceInvite",
                newName: "IX_WorkspaceInvite_ReceiverUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Workspace",
                table: "Workspace",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkspaceMember",
                table: "WorkspaceMember",
                columns: new[] { "WorkspaceId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkspaceInvite",
                table: "WorkspaceInvite",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceInvite_AspNetUsers_ReceiverUserId",
                table: "WorkspaceInvite",
                column: "ReceiverUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceInvite_AspNetUsers_SenderUserId",
                table: "WorkspaceInvite",
                column: "SenderUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceInvite_Workspace_WorkspaceId",
                table: "WorkspaceInvite",
                column: "WorkspaceId",
                principalTable: "Workspace",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceMember_AspNetUsers_UserId",
                table: "WorkspaceMember",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceMember_Workspace_WorkspaceId",
                table: "WorkspaceMember",
                column: "WorkspaceId",
                principalTable: "Workspace",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
