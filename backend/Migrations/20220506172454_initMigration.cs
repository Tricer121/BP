using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    public partial class initMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoordinateIndexes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoordinateIndexes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaticRegions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Index1 = table.Column<double>(type: "float", nullable: false),
                    Index2 = table.Column<double>(type: "float", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaticRegions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AccessExpiresDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RouteCoordinates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserRouteId = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Centered = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteCoordinates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteCoordinates_UserRoutes_UserRouteId",
                        column: x => x.UserRouteId,
                        principalTable: "UserRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivitiesCloseBy",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitiesCloseBy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivitiesCloseBy_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CenteredRouteId = table.Column<int>(type: "int", nullable: true),
                    AveragedRouteId = table.Column<int>(type: "int", nullable: true),
                    RawRouteId = table.Column<int>(type: "int", nullable: true),
                    StravaId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Distance = table.Column<float>(type: "real", nullable: false),
                    ElevationGain = table.Column<float>(type: "real", nullable: false),
                    ElapsedTime = table.Column<float>(type: "real", nullable: false),
                    MaxSpeed = table.Column<float>(type: "real", nullable: false),
                    AverageSpeed = table.Column<float>(type: "real", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivityStatus = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivities_UserRoutes_AveragedRouteId",
                        column: x => x.AveragedRouteId,
                        principalTable: "UserRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserActivities_UserRoutes_CenteredRouteId",
                        column: x => x.CenteredRouteId,
                        principalTable: "UserRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserActivities_UserRoutes_RawRouteId",
                        column: x => x.RawRouteId,
                        principalTable: "UserRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoordinateIndexRouteCoordinate",
                columns: table => new
                {
                    CoordinateIndexesId = table.Column<int>(type: "int", nullable: false),
                    RouteCoordinateId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoordinateIndexRouteCoordinate", x => new { x.CoordinateIndexesId, x.RouteCoordinateId });
                    table.ForeignKey(
                        name: "FK_CoordinateIndexRouteCoordinate_CoordinateIndexes_CoordinateIndexesId",
                        column: x => x.CoordinateIndexesId,
                        principalTable: "CoordinateIndexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoordinateIndexRouteCoordinate_RouteCoordinates_RouteCoordinateId",
                        column: x => x.RouteCoordinateId,
                        principalTable: "RouteCoordinates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityCloseBy",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<long>(type: "bigint", nullable: false),
                    UserActivitiesCloseById = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCloseBy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityCloseBy_ActivitiesCloseBy_UserActivitiesCloseById",
                        column: x => x.UserActivitiesCloseById,
                        principalTable: "ActivitiesCloseBy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaticRegionUserActivity",
                columns: table => new
                {
                    RegionsId = table.Column<int>(type: "int", nullable: false),
                    UserActivitiesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaticRegionUserActivity", x => new { x.RegionsId, x.UserActivitiesId });
                    table.ForeignKey(
                        name: "FK_StaticRegionUserActivity_StaticRegions_RegionsId",
                        column: x => x.RegionsId,
                        principalTable: "StaticRegions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaticRegionUserActivity_UserActivities_UserActivitiesId",
                        column: x => x.UserActivitiesId,
                        principalTable: "UserActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivitiesCloseBy_UserId",
                table: "ActivitiesCloseBy",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCloseBy_UserActivitiesCloseById",
                table: "ActivityCloseBy",
                column: "UserActivitiesCloseById");

            migrationBuilder.CreateIndex(
                name: "IX_CoordinateIndexRouteCoordinate_RouteCoordinateId",
                table: "CoordinateIndexRouteCoordinate",
                column: "RouteCoordinateId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteCoordinates_UserRouteId",
                table: "RouteCoordinates",
                column: "UserRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_StaticRegions_Index1_Index2",
                table: "StaticRegions",
                columns: new[] { "Index1", "Index2" });

            migrationBuilder.CreateIndex(
                name: "IX_StaticRegionUserActivity_UserActivitiesId",
                table: "StaticRegionUserActivity",
                column: "UserActivitiesId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_AveragedRouteId",
                table: "UserActivities",
                column: "AveragedRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_CenteredRouteId",
                table: "UserActivities",
                column: "CenteredRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_RawRouteId",
                table: "UserActivities",
                column: "RawRouteId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_UserId",
                table: "UserActivities",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityCloseBy");

            migrationBuilder.DropTable(
                name: "CoordinateIndexRouteCoordinate");

            migrationBuilder.DropTable(
                name: "StaticRegionUserActivity");

            migrationBuilder.DropTable(
                name: "ActivitiesCloseBy");

            migrationBuilder.DropTable(
                name: "CoordinateIndexes");

            migrationBuilder.DropTable(
                name: "RouteCoordinates");

            migrationBuilder.DropTable(
                name: "StaticRegions");

            migrationBuilder.DropTable(
                name: "UserActivities");

            migrationBuilder.DropTable(
                name: "UserRoutes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
