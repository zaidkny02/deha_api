using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deha_api_exam.Migrations
{
    public partial class mydb_v2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Users_UserID",
                table: "Comment");

            migrationBuilder.CreateTable(
                name: "Vote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "varchar(50)", nullable: false),
                    PostID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vote_Post_PostID",
                        column: x => x.PostID,
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vote_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vote_PostID",
                table: "Vote",
                column: "PostID");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_UserID",
                table: "Vote",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Users_UserID",
                table: "Comment",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Users_UserID",
                table: "Comment");

            migrationBuilder.DropTable(
                name: "Vote");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Users_UserID",
                table: "Comment",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
