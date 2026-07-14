using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeoEducation.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseRegistrationNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "CourseRegistrations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "note",
                table: "CourseRegistrations");
        }
    }
}
