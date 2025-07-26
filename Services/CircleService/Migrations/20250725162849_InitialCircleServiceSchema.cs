using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CircleService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCircleServiceSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ACTIVITIES",
                columns: table => new
                {
                    ACTIVITY_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CIRCLE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TITLE = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    REWARD = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    START_TIME = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    END_TIME = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACTIVITIES", x => x.ACTIVITY_ID);
                });

            migrationBuilder.CreateTable(
                name: "ACTIVITY_PARTICIPANTS",
                columns: table => new
                {
                    ACTIVITY_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    USER_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    JOIN_TIME = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    STATUS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    REWARD_STATUS = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACTIVITY_PARTICIPANTS", x => new { x.ACTIVITY_ID, x.USER_ID });
                });

            migrationBuilder.CreateTable(
                name: "CIRCLE_MEMBERS",
                columns: table => new
                {
                    CIRCLE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    USER_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ROLE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    STATUS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    POINTS = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CIRCLE_MEMBERS", x => new { x.CIRCLE_ID, x.USER_ID });
                });

            migrationBuilder.CreateTable(
                name: "CIRCLES",
                columns: table => new
                {
                    CIRCLE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NAME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    OWNER_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CIRCLES", x => x.CIRCLE_ID);
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICATIONS",
                columns: table => new
                {
                    NOTIFICATION_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    USER_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TYPE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RELATED_ID = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CONTENT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    SENDER_ID = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    IS_READ = table.Column<bool>(type: "BOOLEAN", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICATIONS", x => x.NOTIFICATION_ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ACTIVITIES");

            migrationBuilder.DropTable(
                name: "ACTIVITY_PARTICIPANTS");

            migrationBuilder.DropTable(
                name: "CIRCLE_MEMBERS");

            migrationBuilder.DropTable(
                name: "CIRCLES");

            migrationBuilder.DropTable(
                name: "NOTIFICATIONS");
        }
    }
}
