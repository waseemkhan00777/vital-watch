using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitalCare.Abp.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlertRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VitalType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Operator = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ThresholdMin = table.Column<decimal>(type: "numeric", nullable: true),
                    ThresholdMax = table.Column<decimal>(type: "numeric", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VitalReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    ValueSecondary = table.Column<decimal>(type: "numeric", nullable: true),
                    Unit = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VitalReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VitalReadings_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaregiverLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaregiverId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsentedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaregiverLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaregiverLinks_Users_CaregiverId",
                        column: x => x.CaregiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CaregiverLinks_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    VitalType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    State = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    ValueSecondary = table.Column<decimal>(type: "numeric", nullable: true),
                    Unit = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedById = table.Column<Guid>(type: "uuid", nullable: true),
                    EscalatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedById = table.Column<Guid>(type: "uuid", nullable: true),
                    SlaDueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClinicalNote = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_AlertRules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "AlertRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Alerts_Users_AcknowledgedById",
                        column: x => x.AcknowledgedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Alerts_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alerts_Users_ResolvedById",
                        column: x => x.ResolvedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Resource = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ResourceId = table.Column<string>(type: "text", nullable: true),
                    Details = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AcknowledgedById",
                table: "Alerts",
                column: "AcknowledgedById");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_PatientId_State",
                table: "Alerts",
                columns: new[] { "PatientId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ResolvedById",
                table: "Alerts",
                column: "ResolvedById");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_RuleId",
                table: "Alerts",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_SlaDueAt",
                table: "Alerts",
                column: "SlaDueAt");

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_VitalType_IsActive",
                table: "AlertRules",
                columns: new[] { "VitalType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CaregiverLinks_CaregiverId",
                table: "CaregiverLinks",
                column: "CaregiverId");

            migrationBuilder.CreateIndex(
                name: "IX_CaregiverLinks_PatientId_CaregiverId",
                table: "CaregiverLinks",
                columns: new[] { "PatientId", "CaregiverId" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VitalReadings_PatientId_RecordedAt",
                table: "VitalReadings",
                columns: new[] { "PatientId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Resource",
                table: "AuditLogs",
                column: "Resource");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_Timestamp",
                table: "AuditLogs",
                columns: new[] { "UserId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Alerts");
            migrationBuilder.DropTable(name: "AuditLogs");
            migrationBuilder.DropTable(name: "CaregiverLinks");
            migrationBuilder.DropTable(name: "VitalReadings");
            migrationBuilder.DropTable(name: "AlertRules");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
