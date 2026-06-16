using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeoEducation.Api.Migrations
{
    public partial class AddSubjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Subjects')
                BEGIN
                    CREATE TABLE [Subjects] (
                        [subjectId] int NOT NULL IDENTITY,
                        [subjectName] nvarchar(150) NOT NULL,
                        [description] nvarchar(500) NULL,
                        [imageUrl] nvarchar(500) NULL,
                        [isActive] bit NOT NULL DEFAULT 1,
                        [createdAt] datetime2 NOT NULL DEFAULT (GETDATE()),
                        [updatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
                        CONSTRAINT [PK_Subjects] PRIMARY KEY ([subjectId])
                    );
                END

                IF NOT EXISTS (SELECT * FROM [Subjects])
                BEGIN
                    INSERT INTO [Subjects] ([subjectName], [description], [isActive], [createdAt], [updatedAt])
                    VALUES
                    (N'Tiếng Anh', N'Các khóa học tiếng Anh', 1, GETDATE(), GETDATE()),
                    (N'Toán', N'Các khóa học toán học', 1, GETDATE(), GETDATE()),
                    (N'Vật lý', N'Các khóa học vật lý', 1, GETDATE(), GETDATE()),
                    (N'Hóa học', N'Các khóa học hóa học', 1, GETDATE(), GETDATE()),
                    (N'Ngữ văn', N'Các khóa học ngữ văn', 1, GETDATE(), GETDATE()),
                    (N'Sinh học', N'Các khóa học sinh học', 1, GETDATE(), GETDATE());
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Courses')
                BEGIN
                    CREATE TABLE [Courses] (
                        [courseId] int NOT NULL IDENTITY,
                        [courseName] nvarchar(255) NOT NULL,
                        [description] nvarchar(max) NULL,
                        [instructorId] int NULL,
                        [price] decimal(18,2) NULL,
                        [createdAt] datetime2 NOT NULL DEFAULT (GETDATE()),
                        [updatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
                        CONSTRAINT [PK_Courses] PRIMARY KEY ([courseId])
                    );
                END

                IF NOT EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'Courses' AND COLUMN_NAME = 'subjectId'
                )
                BEGIN
                    ALTER TABLE [Courses] ADD [subjectId] int NULL;
                END

                IF NOT EXISTS (
                    SELECT * FROM sys.indexes
                    WHERE name = 'IX_Courses_subjectId' AND object_id = OBJECT_ID('[Courses]')
                )
                BEGIN
                    CREATE INDEX [IX_Courses_subjectId] ON [Courses] ([subjectId]);
                END

                IF NOT EXISTS (
                    SELECT * FROM sys.foreign_keys
                    WHERE name = 'FK_Courses_Subjects_subjectId'
                )
                BEGIN
                    ALTER TABLE [Courses] WITH CHECK ADD CONSTRAINT [FK_Courses_Subjects_subjectId]
                    FOREIGN KEY([subjectId]) REFERENCES [Subjects] ([subjectId]) ON DELETE SET NULL;
                END

                IF NOT EXISTS (
                    SELECT * FROM sys.indexes
                    WHERE name = 'IX_Courses_instructorId' AND object_id = OBJECT_ID('[Courses]')
                )
                BEGIN
                    CREATE INDEX [IX_Courses_instructorId] ON [Courses] ([instructorId]);
                END

                IF NOT EXISTS (
                    SELECT * FROM sys.foreign_keys
                    WHERE name = 'FK_Courses_Instructors_instructorId'
                )
                BEGIN
                    ALTER TABLE [Courses] WITH CHECK ADD CONSTRAINT [FK_Courses_Instructors_instructorId]
                    FOREIGN KEY([instructorId]) REFERENCES [Instructors] ([Id]) ON DELETE SET NULL;
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CourseRegistrations')
                BEGIN
                    CREATE TABLE [CourseRegistrations] (
                        [registrationId] int NOT NULL IDENTITY,
                        [fullName] nvarchar(255) NOT NULL,
                        [email] nvarchar(255) NULL,
                        [phone] nvarchar(20) NOT NULL,
                        [courseId] int NOT NULL,
                        [status] nvarchar(50) NOT NULL DEFAULT N'Má»›i',
                        [createdAt] datetime2 NOT NULL DEFAULT (GETDATE()),
                        CONSTRAINT [PK_CourseRegistrations] PRIMARY KEY ([registrationId])
                    );
                END

                IF NOT EXISTS (
                    SELECT * FROM sys.indexes
                    WHERE name = 'IX_CourseRegistrations_courseId' AND object_id = OBJECT_ID('[CourseRegistrations]')
                )
                BEGIN
                    CREATE INDEX [IX_CourseRegistrations_courseId] ON [CourseRegistrations] ([courseId]);
                END

                IF NOT EXISTS (
                    SELECT * FROM sys.foreign_keys
                    WHERE name = 'FK_Registration_Course'
                )
                BEGIN
                    ALTER TABLE [CourseRegistrations] WITH CHECK ADD CONSTRAINT [FK_Registration_Course]
                    FOREIGN KEY([courseId]) REFERENCES [Courses] ([courseId]) ON DELETE CASCADE;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Courses_Subjects_subjectId')
                    ALTER TABLE [Courses] DROP CONSTRAINT [FK_Courses_Subjects_subjectId];

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Courses_subjectId' AND object_id = OBJECT_ID('[Courses]'))
                    DROP INDEX [IX_Courses_subjectId] ON [Courses];

                IF EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'Courses' AND COLUMN_NAME = 'subjectId'
                )
                    ALTER TABLE [Courses] DROP COLUMN [subjectId];

                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Subjects')
                    DROP TABLE [Subjects];
            ");
        }
    }
}
