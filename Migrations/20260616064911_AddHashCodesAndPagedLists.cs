using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeoEducation.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHashCodesAndPagedLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "hashCode",
                table: "Subjects",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hashCode",
                table: "Courses",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hashCode",
                table: "CourseRegistrations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hashCode",
                table: "Testimonials",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hashCode",
                table: "Instructors",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hashCode",
                table: "ContactRequests",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hashCode",
                table: "Blogs",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Subjects
                SET hashCode = LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', CONCAT('Subject:', subjectId)), 2))
                WHERE hashCode IS NULL;

                UPDATE Courses
                SET hashCode = LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', CONCAT('Course:', courseId)), 2))
                WHERE hashCode IS NULL;

                UPDATE CourseRegistrations
                SET hashCode = LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', CONCAT('CourseRegistration:', registrationId)), 2))
                WHERE hashCode IS NULL;

                UPDATE Testimonials
                SET hashCode = LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', CONCAT('Testimonial:', testimonialId)), 2))
                WHERE hashCode IS NULL;

                UPDATE Instructors
                SET hashCode = LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', CONCAT('Instructor:', Id)), 2))
                WHERE hashCode IS NULL;

                UPDATE ContactRequests
                SET hashCode = LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', CONCAT('ContactRequest:', Id)), 2))
                WHERE hashCode IS NULL;

                UPDATE Blogs
                SET hashCode = LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', CONCAT('Blog:', Id)), 2))
                WHERE hashCode IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "hashCode", table: "Subjects");
            migrationBuilder.DropColumn(name: "hashCode", table: "Courses");
            migrationBuilder.DropColumn(name: "hashCode", table: "CourseRegistrations");
            migrationBuilder.DropColumn(name: "hashCode", table: "Testimonials");
            migrationBuilder.DropColumn(name: "hashCode", table: "Instructors");
            migrationBuilder.DropColumn(name: "hashCode", table: "ContactRequests");
            migrationBuilder.DropColumn(name: "hashCode", table: "Blogs");
        }
    }
}
