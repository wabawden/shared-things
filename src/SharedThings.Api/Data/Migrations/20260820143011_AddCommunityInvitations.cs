using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharedThings.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommunityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityInvitations_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunityInvitations_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvitations_CommunityId",
                table: "CommunityInvitations",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvitations_CreatedByUserId",
                table: "CommunityInvitations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvitations_TokenHash",
                table: "CommunityInvitations",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityInvitations");
        }
    }
}
