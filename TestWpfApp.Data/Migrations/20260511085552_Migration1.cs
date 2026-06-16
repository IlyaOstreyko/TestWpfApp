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
                name: "Specialities",
                columns: table => new
                {
                    SpecialityId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialities", x => x.SpecialityId);
                });

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    ThemeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.ThemeId);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ThemeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.GroupId);
                    table.ForeignKey(
                        name: "FK_Groups_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "ThemeId");
                });

            migrationBuilder.CreateTable(
                name: "SpecialityTheme",
                columns: table => new
                {
                    SpecialitiesSpecialityId = table.Column<int>(type: "INTEGER", nullable: false),
                    ThemesThemeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialityTheme", x => new { x.SpecialitiesSpecialityId, x.ThemesThemeId });
                    table.ForeignKey(
                        name: "FK_SpecialityTheme_Specialities_SpecialitiesSpecialityId",
                        column: x => x.SpecialitiesSpecialityId,
                        principalTable: "Specialities",
                        principalColumn: "SpecialityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpecialityTheme_Themes_ThemesThemeId",
                        column: x => x.ThemesThemeId,
                        principalTable: "Themes",
                        principalColumn: "ThemeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecialityThemeSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpecialityId = table.Column<int>(type: "INTEGER", nullable: false),
                    ThemeId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionsCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialityThemeSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialityThemeSettings_Specialities_SpecialityId",
                        column: x => x.SpecialityId,
                        principalTable: "Specialities",
                        principalColumn: "SpecialityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpecialityThemeSettings_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "ThemeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestQuestions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ThemeId = table.Column<int>(type: "INTEGER", nullable: false),
                    NameTheme = table.Column<string>(type: "TEXT", nullable: true),
                    NameSpeciality = table.Column<string>(type: "TEXT", nullable: true),
                    ImageQuestion = table.Column<string>(type: "TEXT", nullable: true),
                    ImageQuestionBytes = table.Column<byte[]>(type: "BLOB", nullable: true),
                    NameQuestion = table.Column<string>(type: "TEXT", nullable: true),
                    NameArticle = table.Column<string>(type: "TEXT", nullable: true),
                    NameAnswerCorrect1 = table.Column<string>(type: "TEXT", nullable: true),
                    NameAnswerIncorrect1 = table.Column<string>(type: "TEXT", nullable: true),
                    NameAnswerIncorrect2 = table.Column<string>(type: "TEXT", nullable: true),
                    NameAnswerIncorrect3 = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestQuestions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_TestQuestions_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "ThemeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupSpeciality",
                columns: table => new
                {
                    GroupsGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    SpecialitiesSpecialityId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupSpeciality", x => new { x.GroupsGroupId, x.SpecialitiesSpecialityId });
                    table.ForeignKey(
                        name: "FK_GroupSpeciality_Groups_GroupsGroupId",
                        column: x => x.GroupsGroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupSpeciality_Specialities_SpecialitiesSpecialityId",
                        column: x => x.SpecialitiesSpecialityId,
                        principalTable: "Specialities",
                        principalColumn: "SpecialityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ThemeId",
                table: "Groups",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupSpeciality_SpecialitiesSpecialityId",
                table: "GroupSpeciality",
                column: "SpecialitiesSpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialityTheme_ThemesThemeId",
                table: "SpecialityTheme",
                column: "ThemesThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialityThemeSettings_SpecialityId",
                table: "SpecialityThemeSettings",
                column: "SpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialityThemeSettings_ThemeId",
                table: "SpecialityThemeSettings",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_TestQuestions_ThemeId",
                table: "TestQuestions",
                column: "ThemeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupSpeciality");

            migrationBuilder.DropTable(
                name: "SpecialityTheme");

            migrationBuilder.DropTable(
                name: "SpecialityThemeSettings");

            migrationBuilder.DropTable(
                name: "TestQuestions");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Specialities");

            migrationBuilder.DropTable(
                name: "Themes");
        }
    }
}
