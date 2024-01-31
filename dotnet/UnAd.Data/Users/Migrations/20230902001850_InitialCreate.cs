using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnAd.Data.Users {
    /// <inheritdoc />
    public partial class InitialCreate : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("role_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new {
                    id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("user_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_role",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_role_id",
                table: "user",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "role");
        }
    }
}


