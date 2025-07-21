using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

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
                name: "AI.Ref");

            migrationBuilder.EnsureSchema(
                name: "AI");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "AIService",
                schema: "AI.Ref",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateDeactivated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIService", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Session",
                schema: "AI",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateDeactivated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Model",
                schema: "AI.Ref",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AIServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    IsToolEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateDeactivated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Model", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Model_AIService_AIServiceId",
                        column: x => x.AIServiceId,
                        principalSchema: "AI.Ref",
                        principalTable: "AIService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Document",
                schema: "AI",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Extension = table.Column<string>(type: "text", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateDeactivated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Document_Session_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "AI",
                        principalTable: "Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionDetail",
                schema: "AI",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateDeactivated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionDetail_Model_ModelId",
                        column: x => x.ModelId,
                        principalSchema: "AI.Ref",
                        principalTable: "Model",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionDetail_Session_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "AI",
                        principalTable: "Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentPage",
                schema: "AI",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(768)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateDeactivated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentPage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentPage_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "AI",
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "AI.Ref",
                table: "AIService",
                columns: new[] { "Id", "DateCreated", "DateDeactivated", "Name" },
                values: new object[,]
                {
                    { new Guid("3ad5a77e-515a-4b72-920b-7e4f1d183dfe"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "OpenAI" },
                    { new Guid("89440e45-346f-453b-8e31-a249e4c6c0c5"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ollama" },
                    { new Guid("9f29b328-8e63-4b87-a78d-51e96a660135"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "AzureOpenAI" }
                });

            migrationBuilder.InsertData(
                schema: "AI.Ref",
                table: "Model",
                columns: new[] { "Id", "AIServiceId", "DateCreated", "DateDeactivated", "IsToolEnabled", "Name" },
                values: new object[,]
                {
                    { new Guid("157b91cf-1880-4977-9b7a-7f80f548df04"), new Guid("89440e45-346f-453b-8e31-a249e4c6c0c5"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "llama3.2" },
                    { new Guid("1983e31e-627d-4617-9320-17ded79efa2b"), new Guid("3ad5a77e-515a-4b72-920b-7e4f1d183dfe"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "gpt-4.1-nano" },
                    { new Guid("1fe5381b-0262-469a-b63e-f4d0c4807a98"), new Guid("89440e45-346f-453b-8e31-a249e4c6c0c5"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "gemma3" },
                    { new Guid("9910ba5f-faca-4790-88a4-352e71e14724"), new Guid("89440e45-346f-453b-8e31-a249e4c6c0c5"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, "mistral" },
                    { new Guid("e9bc0791-2d15-43c8-9299-5c86039786f9"), new Guid("3ad5a77e-515a-4b72-920b-7e4f1d183dfe"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "gpt-4.1-mini" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Document_SessionId",
                schema: "AI",
                table: "Document",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPage_DocumentId",
                schema: "AI",
                table: "DocumentPage",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPage_Embedding",
                schema: "AI",
                table: "DocumentPage",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" })
                .Annotation("Npgsql:StorageParameter:ef_construction", 64)
                .Annotation("Npgsql:StorageParameter:m", 16);

            migrationBuilder.CreateIndex(
                name: "IX_Model_AIServiceId",
                schema: "AI.Ref",
                table: "Model",
                column: "AIServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionDetail_ModelId",
                schema: "AI",
                table: "SessionDetail",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionDetail_SessionId",
                schema: "AI",
                table: "SessionDetail",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentPage",
                schema: "AI");

            migrationBuilder.DropTable(
                name: "SessionDetail",
                schema: "AI");

            migrationBuilder.DropTable(
                name: "Document",
                schema: "AI");

            migrationBuilder.DropTable(
                name: "Model",
                schema: "AI.Ref");

            migrationBuilder.DropTable(
                name: "Session",
                schema: "AI");

            migrationBuilder.DropTable(
                name: "AIService",
                schema: "AI.Ref");
        }
    }
}
