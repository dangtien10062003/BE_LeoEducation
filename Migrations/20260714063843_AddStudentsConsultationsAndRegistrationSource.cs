using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LeoEducation.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentsConsultationsAndRegistrationSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "CourseRegistrations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Website");

            migrationBuilder.AddColumn<int>(
                name: "studentId",
                table: "CourseRegistrations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConsultationLogs",
                columns: table => new
                {
                    consultationLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    registrationId = table.Column<int>(type: "integer", nullable: false),
                    contactedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    staffName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    result = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationLogs", x => x.consultationLogId);
                    table.ForeignKey(
                        name: "FK_ConsultationLogs_CourseRegistrations_registrationId",
                        column: x => x.registrationId,
                        principalTable: "CourseRegistrations",
                        principalColumn: "registrationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    studentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hashCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    fullName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    createdAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.studentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistrations_studentId",
                table: "CourseRegistrations",
                column: "studentId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationLogs_registrationId",
                table: "ConsultationLogs",
                column: "registrationId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseRegistrations_Students_studentId",
                table: "CourseRegistrations",
                column: "studentId",
                principalTable: "Students",
                principalColumn: "studentId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseRegistrations_Students_studentId",
                table: "CourseRegistrations");

            migrationBuilder.DropTable(
                name: "ConsultationLogs");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropIndex(
                name: "IX_CourseRegistrations_studentId",
                table: "CourseRegistrations");

            migrationBuilder.DropColumn(
                name: "source",
                table: "CourseRegistrations");

            migrationBuilder.DropColumn(
                name: "studentId",
                table: "CourseRegistrations");
        }
    }
}
