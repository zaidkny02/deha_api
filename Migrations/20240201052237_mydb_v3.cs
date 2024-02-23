using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace deha_api_exam.Migrations
{
    public partial class mydb_v3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostUserName",
                table: "Post",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostUserName",
                table: "Comment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostUserID",
                table: "Attachment",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostUserName",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "PostUserName",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "PostUserID",
                table: "Attachment");
        }
    }
}
