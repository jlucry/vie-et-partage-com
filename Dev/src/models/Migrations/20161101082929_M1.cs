using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace models.Migrations
{
    public partial class M1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    SecurityStamp = table.Column<string>(nullable: true),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Domain = table.Column<string>(nullable: true),
                    HasRegions = table.Column<bool>(nullable: false),
                    Private = table.Column<bool>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Action = table.Column<string>(nullable: false),
                    Category1 = table.Column<int>(nullable: false),
                    Category10 = table.Column<int>(nullable: false),
                    Category2 = table.Column<int>(nullable: false),
                    Category3 = table.Column<int>(nullable: false),
                    Category4 = table.Column<int>(nullable: false),
                    Category5 = table.Column<int>(nullable: false),
                    Category6 = table.Column<int>(nullable: false),
                    Category7 = table.Column<int>(nullable: false),
                    Category8 = table.Column<int>(nullable: false),
                    Category9 = table.Column<int>(nullable: false),
                    Controller = table.Column<string>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    CreatorId = table.Column<string>(nullable: false),
                    Group1 = table.Column<int>(nullable: false),
                    Group10 = table.Column<int>(nullable: false),
                    Group2 = table.Column<int>(nullable: false),
                    Group3 = table.Column<int>(nullable: false),
                    Group4 = table.Column<int>(nullable: false),
                    Group5 = table.Column<int>(nullable: false),
                    Group6 = table.Column<int>(nullable: false),
                    Group7 = table.Column<int>(nullable: false),
                    Group8 = table.Column<int>(nullable: false),
                    Group9 = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    ParentId = table.Column<int>(nullable: true),
                    PositionInNavigation = table.Column<int>(nullable: false),
                    Private = table.Column<bool>(nullable: false),
                    Region1 = table.Column<int>(nullable: false),
                    Region10 = table.Column<int>(nullable: false),
                    Region2 = table.Column<int>(nullable: false),
                    Region3 = table.Column<int>(nullable: false),
                    Region4 = table.Column<int>(nullable: false),
                    Region5 = table.Column<int>(nullable: false),
                    Region6 = table.Column<int>(nullable: false),
                    Region7 = table.Column<int>(nullable: false),
                    Region8 = table.Column<int>(nullable: false),
                    Region9 = table.Column<int>(nullable: false),
                    SiteId = table.Column<int>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    Tag1 = table.Column<int>(nullable: false),
                    Tag10 = table.Column<int>(nullable: false),
                    Tag2 = table.Column<int>(nullable: false),
                    Tag3 = table.Column<int>(nullable: false),
                    Tag4 = table.Column<int>(nullable: false),
                    Tag5 = table.Column<int>(nullable: false),
                    Tag6 = table.Column<int>(nullable: false),
                    Tag7 = table.Column<int>(nullable: false),
                    Tag8 = table.Column<int>(nullable: false),
                    Tag9 = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pages_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pages_Pages_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pages_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Category1 = table.Column<int>(nullable: false),
                    Category10 = table.Column<int>(nullable: false),
                    Category2 = table.Column<int>(nullable: false),
                    Category3 = table.Column<int>(nullable: false),
                    Category4 = table.Column<int>(nullable: false),
                    Category5 = table.Column<int>(nullable: false),
                    Category6 = table.Column<int>(nullable: false),
                    Category7 = table.Column<int>(nullable: false),
                    Category8 = table.Column<int>(nullable: false),
                    Category9 = table.Column<int>(nullable: false),
                    Cover = table.Column<string>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    CreatorId = table.Column<string>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: true),
                    Group1 = table.Column<int>(nullable: false),
                    Group10 = table.Column<int>(nullable: false),
                    Group2 = table.Column<int>(nullable: false),
                    Group3 = table.Column<int>(nullable: false),
                    Group4 = table.Column<int>(nullable: false),
                    Group5 = table.Column<int>(nullable: false),
                    Group6 = table.Column<int>(nullable: false),
                    Group7 = table.Column<int>(nullable: false),
                    Group8 = table.Column<int>(nullable: false),
                    Group9 = table.Column<int>(nullable: false),
                    Highlight = table.Column<bool>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    Private = table.Column<bool>(nullable: false),
                    Region1 = table.Column<int>(nullable: false),
                    Region10 = table.Column<int>(nullable: false),
                    Region2 = table.Column<int>(nullable: false),
                    Region3 = table.Column<int>(nullable: false),
                    Region4 = table.Column<int>(nullable: false),
                    Region5 = table.Column<int>(nullable: false),
                    Region6 = table.Column<int>(nullable: false),
                    Region7 = table.Column<int>(nullable: false),
                    Region8 = table.Column<int>(nullable: false),
                    Region9 = table.Column<int>(nullable: false),
                    SiteId = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    Tag1 = table.Column<int>(nullable: false),
                    Tag10 = table.Column<int>(nullable: false),
                    Tag2 = table.Column<int>(nullable: false),
                    Tag3 = table.Column<int>(nullable: false),
                    Tag4 = table.Column<int>(nullable: false),
                    Tag5 = table.Column<int>(nullable: false),
                    Tag6 = table.Column<int>(nullable: false),
                    Tag7 = table.Column<int>(nullable: false),
                    Tag8 = table.Column<int>(nullable: false),
                    Tag9 = table.Column<int>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: false),
                    ValidationDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Posts_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteActions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ActionDate = table.Column<DateTime>(nullable: false),
                    ActorId = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Element = table.Column<int>(nullable: false),
                    SiteId = table.Column<int>(nullable: false),
                    Table = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteActions_AspNetUsers_ActorId",
                        column: x => x.ActorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SiteActions_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DateTimeValue = table.Column<DateTime>(nullable: true),
                    ParentId = table.Column<int>(nullable: true),
                    SiteId = table.Column<int>(nullable: false),
                    StringValue = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: false),
                    Value = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteClaims_SiteClaims_ParentId",
                        column: x => x.ParentId,
                        principalTable: "SiteClaims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SiteClaims_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PageClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DateTimeValue = table.Column<DateTime>(nullable: true),
                    PageId = table.Column<int>(nullable: false),
                    SiteId = table.Column<int>(nullable: true),
                    StringValue = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: false),
                    Value = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageClaims_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PageClaims_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DateTimeValue = table.Column<DateTime>(nullable: true),
                    PostId = table.Column<int>(nullable: false),
                    SiteId = table.Column<int>(nullable: true),
                    StringValue = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: false),
                    Value = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostClaims_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostClaims_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    CreatorId = table.Column<string>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    PostId = table.Column<int>(nullable: true),
                    SiteId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostFiles_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostFiles_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostFiles_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostTexts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Number = table.Column<int>(nullable: false),
                    PostId = table.Column<int>(nullable: false),
                    Revision = table.Column<int>(nullable: false),
                    SiteId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostTexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostTexts_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostTexts_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pages_CreatorId",
                table: "Pages",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_ParentId",
                table: "Pages",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_SiteId",
                table: "Pages",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_PageClaims_PageId",
                table: "PageClaims",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_PageClaims_SiteId",
                table: "PageClaims",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreatorId",
                table: "Posts",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_SiteId",
                table: "Posts",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_PostClaims_PostId",
                table: "PostClaims",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostClaims_SiteId",
                table: "PostClaims",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_PostFiles_CreatorId",
                table: "PostFiles",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_PostFiles_PostId",
                table: "PostFiles",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostFiles_SiteId",
                table: "PostFiles",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_PostTexts_PostId",
                table: "PostTexts",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostTexts_SiteId",
                table: "PostTexts",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteActions_ActorId",
                table: "SiteActions",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteActions_SiteId",
                table: "SiteActions",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteClaims_ParentId",
                table: "SiteClaims",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteClaims_SiteId",
                table: "SiteClaims",
                column: "SiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "PageClaims");

            migrationBuilder.DropTable(
                name: "PostClaims");

            migrationBuilder.DropTable(
                name: "PostFiles");

            migrationBuilder.DropTable(
                name: "PostTexts");

            migrationBuilder.DropTable(
                name: "SiteActions");

            migrationBuilder.DropTable(
                name: "SiteClaims");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Sites");
        }
    }
}
