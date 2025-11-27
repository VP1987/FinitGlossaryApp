using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinitiGlossary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================
            // tblUsers
            // ============================
            migrationBuilder.CreateTable(
                name: "tblUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "User"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetTokenExpires = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    MustUpdateProfile = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblUsers", x => x.Id);
                });

            // ============================
            // tblRefreshTokens
            // ============================
            migrationBuilder.CreateTable(
                name: "tblRefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblRefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblRefreshTokens_tblUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblRefreshTokens_UserId",
                table: "tblRefreshTokens",
                column: "UserId");

            // ============================
            // tblGlossaryTerms
            // ============================
            migrationBuilder.CreateTable(
                name: "tblGlossaryTerms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Term = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Definition = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    StableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblGlossaryTerms", x => x.Id);
                });

            // ============================
            // tblArchivedGlossaryTerms
            // ============================
            migrationBuilder.CreateTable(
                name: "tblArchivedGlossaryTerms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalTermId = table.Column<int>(type: "int", nullable: false),
                    Term = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Definition = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArchivedById = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(1000)", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    RestoredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RestoredById = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    StableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblArchivedGlossaryTerms", x => x.Id);
                });

            // ============================
            // tblTestEntities
            // ============================
            migrationBuilder.CreateTable(
                name: "tblTestEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblTestEntities", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "tblArchivedGlossaryTerms");
            migrationBuilder.DropTable(name: "tblGlossaryTerms");
            migrationBuilder.DropTable(name: "tblTestEntities");
            migrationBuilder.DropTable(name: "tblRefreshTokens");
            migrationBuilder.DropTable(name: "tblUsers");
        }
    }
}
