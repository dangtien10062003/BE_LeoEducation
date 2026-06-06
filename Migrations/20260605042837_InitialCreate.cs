using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeoEducation.Api.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only create tables if they don't exist (DB already has data)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
                BEGIN
                    CREATE TABLE [Users] (
                        [userId] int NOT NULL IDENTITY,
                        [fullName] nvarchar(100) NOT NULL,
                        [email] nvarchar(256) NOT NULL,
                        [phone] nvarchar(20) NULL,
                        [passwordHash] nvarchar(max) NOT NULL,
                        [avatarURL] nvarchar(500) NULL,
                        [status] nvarchar(20) NOT NULL DEFAULT N'Active',
                        [createdAt] datetime2 NOT NULL DEFAULT (GETDATE()),
                        [updatedAt] datetime2 NULL,
                        [LastLoginAt] datetime2 NULL,
                        CONSTRAINT [PK_Users] PRIMARY KEY ([userId])
                    );
                    CREATE UNIQUE INDEX [IX_Users_email] ON [Users] ([email]);
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Instructors')
                BEGIN
                    CREATE TABLE [Instructors] (
                        [Id] int NOT NULL IDENTITY,
                        [FullName] nvarchar(100) NOT NULL,
                        [Role] nvarchar(100) NULL,
                        [Bio] nvarchar(max) NULL,
                        [AvatarUrl] nvarchar(500) NULL,
                        [Rating] decimal(3,2) NOT NULL DEFAULT 5.0,
                        [Experience] nvarchar(50) NULL,
                        [IsActive] bit NOT NULL DEFAULT 1,
                        CONSTRAINT [PK_Instructors] PRIMARY KEY ([Id])
                    );
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Testimonials')
                BEGIN
                    CREATE TABLE [Testimonials] (
                        [testimonialId] int NOT NULL IDENTITY,
                        [studentName] nvarchar(255) NOT NULL,
                        [jobTitle] nvarchar(255) NULL,
                        [content] nvarchar(max) NOT NULL,
                        [rating] int NOT NULL DEFAULT 5,
                        [avatarURL] nvarchar(500) NULL,
                        [isActive] bit NOT NULL DEFAULT 1,
                        [createdAt] datetime2 NOT NULL DEFAULT (GETDATE()),
                        CONSTRAINT [PK_Testimonials] PRIMARY KEY ([testimonialId])
                    );
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ContactRequests')
                BEGIN
                    CREATE TABLE [ContactRequests] (
                        [Id] int NOT NULL IDENTITY,
                        [FullName] nvarchar(100) NOT NULL,
                        [Email] nvarchar(100) NOT NULL,
                        [Phone] nvarchar(20) NOT NULL,
                        [Message] nvarchar(max) NULL,
                        [Status] nvarchar(50) NOT NULL DEFAULT N'New',
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
                        [UpdatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
                        CONSTRAINT [PK_ContactRequests] PRIMARY KEY ([Id])
                    );
                END

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Blogs')
                BEGIN
                    CREATE TABLE [Blogs] (
                        [Id] int NOT NULL IDENTITY,
                        [Title] nvarchar(250) NOT NULL,
                        [Summary] nvarchar(500) NULL,
                        [Content] nvarchar(max) NULL,
                        [ImageUrl] nvarchar(500) NULL,
                        [Author] nvarchar(100) NULL,
                        [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
                        CONSTRAINT [PK_Blogs] PRIMARY KEY ([Id])
                    );
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: don't drop existing tables
        }
    }
}
