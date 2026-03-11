using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitalCare.Abp.Migrations
{
    public partial class ExtendAuditLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResourceType",
                table: "AuditLogs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataType",
                table: "AuditLogs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataId",
                table: "AuditLogs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "AuditLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessedFields",
                table: "AuditLogs",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_PatientId",
                table: "AuditLogs",
                column: "PatientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_AuditLogs_PatientId", table: "AuditLogs");
            migrationBuilder.DropColumn(name: "ResourceType", table: "AuditLogs");
            migrationBuilder.DropColumn(name: "DataType", table: "AuditLogs");
            migrationBuilder.DropColumn(name: "DataId", table: "AuditLogs");
            migrationBuilder.DropColumn(name: "PatientId", table: "AuditLogs");
            migrationBuilder.DropColumn(name: "AccessedFields", table: "AuditLogs");
        }
    }
}
