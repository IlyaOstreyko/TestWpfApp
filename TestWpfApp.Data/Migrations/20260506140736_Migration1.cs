using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestWpfApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class Migration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupDataModels",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NameGroup = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupDataModels", x => x.GroupId);
                });

            migrationBuilder.CreateTable(
                name: "SpecialityDataModels",
                columns: table => new
                {
                    SpecialityId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NameSpeciality = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialityDataModels", x => x.SpecialityId);
                });

            migrationBuilder.CreateTable(
                name: "ThemeDataModels",
                columns: table => new
                {
                    ThemeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NameTheme = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeDataModels", x => x.ThemeId);
                });

            migrationBuilder.CreateTable(
                name: "GroupDataModelSpecialityDataModel",
                columns: table => new
                {
                    GroupsDataModelGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    SpecialitysDataModelSpecialityId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupDataModelSpecialityDataModel", x => new { x.GroupsDataModelGroupId, x.SpecialitysDataModelSpecialityId });
                    table.ForeignKey(
                        name: "FK_GroupDataModelSpecialityDataModel_GroupDataModels_GroupsDataModelGroupId",
                        column: x => x.GroupsDataModelGroupId,
                        principalTable: "GroupDataModels",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupDataModelSpecialityDataModel_SpecialityDataModels_SpecialitysDataModelSpecialityId",
                        column: x => x.SpecialitysDataModelSpecialityId,
                        principalTable: "SpecialityDataModels",
                        principalColumn: "SpecialityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SettingsSpecialityDataModel",
                columns: table => new
                {
                    SettingsSpecialityId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ThemeDataModelThemeId = table.Column<int>(type: "INTEGER", nullable: true),
                    NumberQuestionsInTheme = table.Column<int>(type: "INTEGER", nullable: true),
                    SpecialityDataModelSpecialityId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingsSpecialityDataModel", x => x.SettingsSpecialityId);
                    table.ForeignKey(
                        name: "FK_SettingsSpecialityDataModel_SpecialityDataModels_SpecialityDataModelSpecialityId",
                        column: x => x.SpecialityDataModelSpecialityId,
                        principalTable: "SpecialityDataModels",
                        principalColumn: "SpecialityId");
                    table.ForeignKey(
                        name: "FK_SettingsSpecialityDataModel_ThemeDataModels_ThemeDataModelThemeId",
                        column: x => x.ThemeDataModelThemeId,
                        principalTable: "ThemeDataModels",
                        principalColumn: "ThemeId");
                });

            migrationBuilder.CreateTable(
                name: "SpecialityDataModelThemeDataModel",
                columns: table => new
                {
                    SpecialitysDataModelSpecialityId = table.Column<int>(type: "INTEGER", nullable: false),
                    ThemesDataModelThemeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialityDataModelThemeDataModel", x => new { x.SpecialitysDataModelSpecialityId, x.ThemesDataModelThemeId });
                    table.ForeignKey(
                        name: "FK_SpecialityDataModelThemeDataModel_SpecialityDataModels_SpecialitysDataModelSpecialityId",
                        column: x => x.SpecialitysDataModelSpecialityId,
                        principalTable: "SpecialityDataModels",
                        principalColumn: "SpecialityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpecialityDataModelThemeDataModel_ThemeDataModels_ThemesDataModelThemeId",
                        column: x => x.ThemesDataModelThemeId,
                        principalTable: "ThemeDataModels",
                        principalColumn: "ThemeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestQuestionDataModels",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpecialityId = table.Column<int>(type: "INTEGER", nullable: true),
                    ThemeId = table.Column<int>(type: "INTEGER", nullable: true),
                    AnswersId = table.Column<int>(type: "INTEGER", nullable: true),
                    ImageQuestion = table.Column<string>(type: "TEXT", nullable: true),
                    NameQuestion = table.Column<string>(type: "TEXT", nullable: true),
                    NameArticle = table.Column<string>(type: "TEXT", nullable: true),
                    NameSpeciality = table.Column<string>(type: "TEXT", nullable: true),
                    NameTheme = table.Column<string>(type: "TEXT", nullable: true),
                    NameAnswerCorrect1 = table.Column<string>(type: "TEXT", nullable: true),
                    NameAnswerIncorrect1 = table.Column<string>(type: "TEXT", nullable: true),
                    NameAnswerIncorrect2 = table.Column<string>(type: "TEXT", nullable: true),
                    NameAnswerIncorrect3 = table.Column<string>(type: "TEXT", nullable: true),
                    ThemeDataModelThemeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestQuestionDataModels", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_TestQuestionDataModels_ThemeDataModels_ThemeDataModelThemeId",
                        column: x => x.ThemeDataModelThemeId,
                        principalTable: "ThemeDataModels",
                        principalColumn: "ThemeId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupDataModelSpecialityDataModel_SpecialitysDataModelSpecialityId",
                table: "GroupDataModelSpecialityDataModel",
                column: "SpecialitysDataModelSpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_SettingsSpecialityDataModel_SpecialityDataModelSpecialityId",
                table: "SettingsSpecialityDataModel",
                column: "SpecialityDataModelSpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_SettingsSpecialityDataModel_ThemeDataModelThemeId",
                table: "SettingsSpecialityDataModel",
                column: "ThemeDataModelThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialityDataModelThemeDataModel_ThemesDataModelThemeId",
                table: "SpecialityDataModelThemeDataModel",
                column: "ThemesDataModelThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_TestQuestionDataModels_ThemeDataModelThemeId",
                table: "TestQuestionDataModels",
                column: "ThemeDataModelThemeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupDataModelSpecialityDataModel");

            migrationBuilder.DropTable(
                name: "SettingsSpecialityDataModel");

            migrationBuilder.DropTable(
                name: "SpecialityDataModelThemeDataModel");

            migrationBuilder.DropTable(
                name: "TestQuestionDataModels");

            migrationBuilder.DropTable(
                name: "GroupDataModels");

            migrationBuilder.DropTable(
                name: "SpecialityDataModels");

            migrationBuilder.DropTable(
                name: "ThemeDataModels");
        }
    }
}
