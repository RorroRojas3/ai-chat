using System;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RR.AI_Chat.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Core.Ref");

            migrationBuilder.EnsureSchema(
                name: "Core");

            migrationBuilder.CreateTable(
                name: "AIService",
                schema: "Core.Ref",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateDeactivated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIService", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IsSuperAdministrator = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateDeactivated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Model",
                schema: "Core.Ref",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AIServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Encoding = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    IsToolEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateDeactivated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Model", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Model_AIService_AIServiceId",
                        column: x => x.AIServiceId,
                        principalSchema: "Core.Ref",
                        principalTable: "AIService",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Project",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateDeactivated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Core",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProjectDocument",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateDeactivated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDocument_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "Core",
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectDocument_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Core",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Session",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    InputTokens = table.Column<long>(type: "bigint", nullable: false),
                    OutputTokens = table.Column<long>(type: "bigint", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateDeactivated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Session_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "Core",
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Session_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Core",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProjectDocumentPage",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateDeactivated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Embedding = table.Column<SqlVector<float>>(type: "vector(1536)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDocumentPage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDocumentPage_ProjectDocument_ProjectDocumentId",
                        column: x => x.ProjectDocumentId,
                        principalSchema: "Core",
                        principalTable: "ProjectDocument",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SessionDocument",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateDeactivated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionDocument_Session_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "Core",
                        principalTable: "Session",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SessionDocument_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Core",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SessionDocumentPage",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DateDeactivated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DateModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Embedding = table.Column<SqlVector<float>>(type: "vector(1536)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionDocumentPage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionDocumentPage_SessionDocument_SessionDocumentId",
                        column: x => x.SessionDocumentId,
                        principalSchema: "Core",
                        principalTable: "SessionDocument",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                schema: "Core.Ref",
                table: "AIService",
                columns: new[] { "Id", "DateCreated", "DateDeactivated", "Name" },
                values: new object[] { new Guid("3f2a91b5-9e5a-4a0a-a57a-ec70b540bbf0"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "AzureAIFoundry" });

            migrationBuilder.InsertData(
                schema: "Core.Ref",
                table: "Model",
                columns: new[] { "Id", "AIServiceId", "DateCreated", "DateDeactivated", "Encoding", "IsToolEnabled", "Name" },
                values: new object[,]
                {
                    { new Guid("0b3948f5-70df-4697-a033-ae70971e1796"), new Guid("3f2a91b5-9e5a-4a0a-a57a-ec70b540bbf0"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "o200k_harmony", true, "gpt-5-chat" },
                    { new Guid("c36e22ed-262a-47a1-b2ba-06a38355ae0f"), new Guid("3f2a91b5-9e5a-4a0a-a57a-ec70b540bbf0"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "o200k_harmony", true, "gpt-5-mini" },
                    { new Guid("fd01b615-1e9f-46af-957f-e4eaeff02766"), new Guid("3f2a91b5-9e5a-4a0a-a57a-ec70b540bbf0"), new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "o200k_harmony", true, "gpt-5-nano" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Model_AIServiceId",
                schema: "Core.Ref",
                table: "Model",
                column: "AIServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_UserId",
                schema: "Core",
                table: "Project",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocument_ProjectId",
                schema: "Core",
                table: "ProjectDocument",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocument_UserId",
                schema: "Core",
                table: "ProjectDocument",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocumentPage_ProjectDocumentId",
                schema: "Core",
                table: "ProjectDocumentPage",
                column: "ProjectDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_ProjectId",
                schema: "Core",
                table: "Session",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_UserId",
                schema: "Core",
                table: "Session",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionDocument_SessionId",
                schema: "Core",
                table: "SessionDocument",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionDocument_UserId",
                schema: "Core",
                table: "SessionDocument",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionDocumentPage_SessionDocumentId",
                schema: "Core",
                table: "SessionDocumentPage",
                column: "SessionDocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Model",
                schema: "Core.Ref");

            migrationBuilder.DropTable(
                name: "ProjectDocumentPage",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "SessionDocumentPage",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "AIService",
                schema: "Core.Ref");

            migrationBuilder.DropTable(
                name: "ProjectDocument",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "SessionDocument",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "Session",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "Project",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "User",
                schema: "Core");
        }
    }
}
