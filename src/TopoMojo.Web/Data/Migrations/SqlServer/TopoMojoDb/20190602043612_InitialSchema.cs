// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TopoMojo.Web.Data.Migrations.SqlServer.TopoMojoDb
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "History",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ActorId = table.Column<int>(nullable: false),
                    AssetId = table.Column<int>(nullable: false),
                    Action = table.Column<int>(nullable: false),
                    At = table.Column<DateTime>(nullable: false),
                    Actor = table.Column<string>(nullable: true),
                    Asset = table.Column<string>(nullable: true),
                    Annotation = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_History", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RoomId = table.Column<string>(nullable: true),
                    Text = table.Column<string>(nullable: true),
                    Edited = table.Column<bool>(nullable: false),
                    WhenCreated = table.Column<DateTime>(nullable: false),
                    AuthorId = table.Column<int>(nullable: false),
                    AuthorName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GlobalId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    WhenCreated = table.Column<DateTime>(nullable: false),
                    IsAdmin = table.Column<bool>(nullable: false),
                    WorkspaceLimit = table.Column<int>(nullable: false),
                    Role = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                    table.UniqueConstraint("AK_Profiles_GlobalId", x => x.GlobalId);
                });

            migrationBuilder.CreateTable(
                name: "Topologies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GlobalId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    WhenCreated = table.Column<DateTime>(nullable: false),
                    WhenPublished = table.Column<DateTime>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    DocumentUrl = table.Column<string>(nullable: true),
                    ShareCode = table.Column<string>(nullable: true),
                    Author = table.Column<string>(nullable: true),
                    IsPublished = table.Column<bool>(nullable: false),
                    IsLocked = table.Column<bool>(nullable: false),
                    TemplateLimit = table.Column<int>(nullable: false),
                    UseUplinkSwitch = table.Column<bool>(nullable: false),
                    LaunchCount = table.Column<int>(nullable: false),
                    LastLaunch = table.Column<DateTime>(nullable: false),
                    IsEvergreen = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topologies", x => x.Id);
                    table.UniqueConstraint("AK_Topologies_GlobalId", x => x.GlobalId);
                });

            migrationBuilder.CreateTable(
                name: "Gamespaces",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GlobalId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    WhenCreated = table.Column<DateTime>(nullable: false),
                    ShareCode = table.Column<string>(nullable: true),
                    TopologyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gamespaces", x => x.Id);
                    table.UniqueConstraint("AK_Gamespaces_GlobalId", x => x.GlobalId);
                    table.ForeignKey(
                        name: "FK_Gamespaces_Topologies_TopologyId",
                        column: x => x.TopologyId,
                        principalTable: "Topologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GlobalId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    WhenCreated = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Iso = table.Column<string>(nullable: true),
                    Networks = table.Column<string>(nullable: true),
                    IsHidden = table.Column<bool>(nullable: false),
                    IsPublished = table.Column<bool>(nullable: false),
                    Detail = table.Column<string>(nullable: true),
                    ParentId = table.Column<int>(nullable: true),
                    TopologyId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                    table.UniqueConstraint("AK_Templates_GlobalId", x => x.GlobalId);
                    table.ForeignKey(
                        name: "FK_Templates_Templates_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Templates_Topologies_TopologyId",
                        column: x => x.TopologyId,
                        principalTable: "Topologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TopologyId = table.Column<int>(nullable: false),
                    PersonId = table.Column<int>(nullable: false),
                    Permission = table.Column<int>(nullable: false),
                    LastSeen = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workers_Profiles_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Workers_Topologies_TopologyId",
                        column: x => x.TopologyId,
                        principalTable: "Topologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GamespaceId = table.Column<int>(nullable: false),
                    PersonId = table.Column<int>(nullable: false),
                    Permission = table.Column<int>(nullable: false),
                    LastSeen = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Gamespaces_GamespaceId",
                        column: x => x.GamespaceId,
                        principalTable: "Gamespaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Players_Profiles_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gamespaces_TopologyId",
                table: "Gamespaces",
                column: "TopologyId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RoomId",
                table: "Messages",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_GamespaceId",
                table: "Players",
                column: "GamespaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_PersonId",
                table: "Players",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_ParentId",
                table: "Templates",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_TopologyId",
                table: "Templates",
                column: "TopologyId");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_PersonId",
                table: "Workers",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_TopologyId",
                table: "Workers",
                column: "TopologyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "History");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "Workers");

            migrationBuilder.DropTable(
                name: "Gamespaces");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "Topologies");
        }
    }
}
