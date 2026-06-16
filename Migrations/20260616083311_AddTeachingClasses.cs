using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeoEducation.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTeachingClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeachingClasses",
                columns: table => new
                {
                    classId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hashCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    className = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    courseId = table.Column<int>(type: "int", nullable: false),
                    subjectId = table.Column<int>(type: "int", nullable: true),
                    instructorId = table.Column<int>(type: "int", nullable: true),
                    startDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingClasses", x => x.classId);
                    table.ForeignKey(
                        name: "FK_TeachingClasses_Courses_courseId",
                        column: x => x.courseId,
                        principalTable: "Courses",
                        principalColumn: "courseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingClasses_Instructors_instructorId",
                        column: x => x.instructorId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TeachingClasses_Subjects_subjectId",
                        column: x => x.subjectId,
                        principalTable: "Subjects",
                        principalColumn: "subjectId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TeachingClassStudents",
                columns: table => new
                {
                    classId = table.Column<int>(type: "int", nullable: false),
                    registrationId = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingClassStudents", x => new { x.classId, x.registrationId });
                    table.ForeignKey(
                        name: "FK_TeachingClassStudents_CourseRegistrations_registrationId",
                        column: x => x.registrationId,
                        principalTable: "CourseRegistrations",
                        principalColumn: "registrationId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TeachingClassStudents_TeachingClasses_classId",
                        column: x => x.classId,
                        principalTable: "TeachingClasses",
                        principalColumn: "classId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingClasses_courseId",
                table: "TeachingClasses",
                column: "courseId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingClasses_instructorId",
                table: "TeachingClasses",
                column: "instructorId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingClasses_subjectId",
                table: "TeachingClasses",
                column: "subjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingClassStudents_registrationId",
                table: "TeachingClassStudents",
                column: "registrationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeachingClassStudents");

            migrationBuilder.DropTable(
                name: "TeachingClasses");
        }
    }
}
