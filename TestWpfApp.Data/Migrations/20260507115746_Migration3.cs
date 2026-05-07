using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWpfApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class Migration3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswersId",
                table: "TestQuestionDataModels");

            migrationBuilder.DropColumn(
                name: "SpecialityId",
                table: "TestQuestionDataModels");

            migrationBuilder.AddColumn<int>(
                name: "ThemeDataModelThemeId",
                table: "GroupDataModels",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupDataModels_ThemeDataModelThemeId",
                table: "GroupDataModels",
                column: "ThemeDataModelThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupDataModels_ThemeDataModels_ThemeDataModelThemeId",
                table: "GroupDataModels",
                column: "ThemeDataModelThemeId",
                principalTable: "ThemeDataModels",
                principalColumn: "ThemeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupDataModels_ThemeDataModels_ThemeDataModelThemeId",
                table: "GroupDataModels");

            migrationBuilder.DropIndex(
                name: "IX_GroupDataModels_ThemeDataModelThemeId",
                table: "GroupDataModels");

            migrationBuilder.DropColumn(
                name: "ThemeDataModelThemeId",
                table: "GroupDataModels");

            migrationBuilder.AddColumn<int>(
                name: "AnswersId",
                table: "TestQuestionDataModels",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpecialityId",
                table: "TestQuestionDataModels",
                type: "INTEGER",
                nullable: true);
        }
    }
}
