using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWpfApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class Migration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageQuestionBytes",
                table: "TestQuestionDataModels",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageQuestionBytes",
                table: "TestQuestionDataModels");
        }
    }
}
