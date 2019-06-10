using Microsoft.EntityFrameworkCore.Migrations;

namespace Markov_Jokes.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jokes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Contents = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jokes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(nullable: false),
                    StartCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Id);
                    table.UniqueConstraint("AK_Words_Content", x => x.Content);
                });

            migrationBuilder.CreateTable(
                name: "Weights",
                columns: table => new
                {
                    WordId = table.Column<long>(nullable: false),
                    FollowingWordId = table.Column<long>(nullable: false),
                    WordId1 = table.Column<int>(nullable: false),
                    FollowingWordId1 = table.Column<int>(nullable: false),
                    Occurrences = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weights", x => new { x.WordId, x.FollowingWordId });
                    table.ForeignKey(
                        name: "FK_Weights_Words_FollowingWordId1",
                        column: x => x.FollowingWordId1,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Weights_Words_WordId1",
                        column: x => x.WordId1,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Weights_FollowingWordId1",
                table: "Weights",
                column: "FollowingWordId1");

            migrationBuilder.CreateIndex(
                name: "IX_Weights_WordId1",
                table: "Weights",
                column: "WordId1");

            migrationBuilder.CreateIndex(
                name: "IX_Words_Content",
                table: "Words",
                column: "Content",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jokes");

            migrationBuilder.DropTable(
                name: "Weights");

            migrationBuilder.DropTable(
                name: "Words");
        }
    }
}
