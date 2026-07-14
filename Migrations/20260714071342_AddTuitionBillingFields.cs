using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeoEducation.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTuitionBillingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "billingType",
                table: "Courses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "FullCourse");

            migrationBuilder.AddColumn<DateTime>(
                name: "endDate",
                table: "Courses",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "startDate",
                table: "Courses",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "lastPaymentAt",
                table: "CourseRegistrations",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "paidAmount",
                table: "CourseRegistrations",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "paymentMode",
                table: "CourseRegistrations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tuitionNote",
                table: "CourseRegistrations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "billingType",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "endDate",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "startDate",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "lastPaymentAt",
                table: "CourseRegistrations");

            migrationBuilder.DropColumn(
                name: "paidAmount",
                table: "CourseRegistrations");

            migrationBuilder.DropColumn(
                name: "paymentMode",
                table: "CourseRegistrations");

            migrationBuilder.DropColumn(
                name: "tuitionNote",
                table: "CourseRegistrations");
        }
    }
}
