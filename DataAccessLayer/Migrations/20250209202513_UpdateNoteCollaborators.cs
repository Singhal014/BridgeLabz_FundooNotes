using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    public partial class UpdateNoteCollaborators : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NoteCollaborators",
                table: "NoteCollaborators");

            migrationBuilder.DropIndex(
                name: "IX_NoteCollaborators_NoteId",
                table: "NoteCollaborators");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NoteCollaborators",
                table: "NoteCollaborators",
                columns: new[] { "NoteId", "CollaboratorId" });

            migrationBuilder.CreateIndex(
                name: "IX_NoteCollaborators_CollaboratorId",
                table: "NoteCollaborators",
                column: "CollaboratorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NoteCollaborators",
                table: "NoteCollaborators");

            migrationBuilder.DropIndex(
                name: "IX_NoteCollaborators_CollaboratorId",
                table: "NoteCollaborators");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NoteCollaborators",
                table: "NoteCollaborators",
                columns: new[] { "CollaboratorId", "NoteId" });

            migrationBuilder.CreateIndex(
                name: "IX_NoteCollaborators_NoteId",
                table: "NoteCollaborators",
                column: "NoteId");
        }
    }
}
