using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Models;

namespace models.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20161101082929_M1")]
    partial class M1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id");

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedUserName")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Models.Page", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Action")
                        .IsRequired();

                    b.Property<int>("Category1");

                    b.Property<int>("Category10");

                    b.Property<int>("Category2");

                    b.Property<int>("Category3");

                    b.Property<int>("Category4");

                    b.Property<int>("Category5");

                    b.Property<int>("Category6");

                    b.Property<int>("Category7");

                    b.Property<int>("Category8");

                    b.Property<int>("Category9");

                    b.Property<string>("Controller")
                        .IsRequired();

                    b.Property<DateTime>("CreationDate");

                    b.Property<string>("CreatorId")
                        .IsRequired();

                    b.Property<int>("Group1");

                    b.Property<int>("Group10");

                    b.Property<int>("Group2");

                    b.Property<int>("Group3");

                    b.Property<int>("Group4");

                    b.Property<int>("Group5");

                    b.Property<int>("Group6");

                    b.Property<int>("Group7");

                    b.Property<int>("Group8");

                    b.Property<int>("Group9");

                    b.Property<DateTime?>("ModifiedDate");

                    b.Property<int?>("ParentId");

                    b.Property<int>("PositionInNavigation");

                    b.Property<bool>("Private");

                    b.Property<int>("Region1");

                    b.Property<int>("Region10");

                    b.Property<int>("Region2");

                    b.Property<int>("Region3");

                    b.Property<int>("Region4");

                    b.Property<int>("Region5");

                    b.Property<int>("Region6");

                    b.Property<int>("Region7");

                    b.Property<int>("Region8");

                    b.Property<int>("Region9");

                    b.Property<int>("SiteId");

                    b.Property<int>("State");

                    b.Property<int>("Tag1");

                    b.Property<int>("Tag10");

                    b.Property<int>("Tag2");

                    b.Property<int>("Tag3");

                    b.Property<int>("Tag4");

                    b.Property<int>("Tag5");

                    b.Property<int>("Tag6");

                    b.Property<int>("Tag7");

                    b.Property<int>("Tag8");

                    b.Property<int>("Tag9");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("ParentId");

                    b.HasIndex("SiteId");

                    b.ToTable("Pages");
                });

            modelBuilder.Entity("Models.PageClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("DateTimeValue");

                    b.Property<int>("PageId");

                    b.Property<int?>("SiteId");

                    b.Property<string>("StringValue");

                    b.Property<string>("Type")
                        .IsRequired();

                    b.Property<int?>("Value");

                    b.HasKey("Id");

                    b.HasIndex("PageId");

                    b.HasIndex("SiteId");

                    b.ToTable("PageClaims");
                });

            modelBuilder.Entity("Models.Post", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Category1");

                    b.Property<int>("Category10");

                    b.Property<int>("Category2");

                    b.Property<int>("Category3");

                    b.Property<int>("Category4");

                    b.Property<int>("Category5");

                    b.Property<int>("Category6");

                    b.Property<int>("Category7");

                    b.Property<int>("Category8");

                    b.Property<int>("Category9");

                    b.Property<string>("Cover");

                    b.Property<DateTime>("CreationDate");

                    b.Property<string>("CreatorId")
                        .IsRequired();

                    b.Property<DateTime?>("EndDate");

                    b.Property<int>("Group1");

                    b.Property<int>("Group10");

                    b.Property<int>("Group2");

                    b.Property<int>("Group3");

                    b.Property<int>("Group4");

                    b.Property<int>("Group5");

                    b.Property<int>("Group6");

                    b.Property<int>("Group7");

                    b.Property<int>("Group8");

                    b.Property<int>("Group9");

                    b.Property<bool>("Highlight");

                    b.Property<DateTime?>("ModifiedDate");

                    b.Property<bool>("Private");

                    b.Property<int>("Region1");

                    b.Property<int>("Region10");

                    b.Property<int>("Region2");

                    b.Property<int>("Region3");

                    b.Property<int>("Region4");

                    b.Property<int>("Region5");

                    b.Property<int>("Region6");

                    b.Property<int>("Region7");

                    b.Property<int>("Region8");

                    b.Property<int>("Region9");

                    b.Property<int>("SiteId");

                    b.Property<DateTime?>("StartDate");

                    b.Property<int>("State");

                    b.Property<int>("Tag1");

                    b.Property<int>("Tag10");

                    b.Property<int>("Tag2");

                    b.Property<int>("Tag3");

                    b.Property<int>("Tag4");

                    b.Property<int>("Tag5");

                    b.Property<int>("Tag6");

                    b.Property<int>("Tag7");

                    b.Property<int>("Tag8");

                    b.Property<int>("Tag9");

                    b.Property<string>("Text");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.Property<DateTime?>("ValidationDate");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("SiteId");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("Models.PostClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("DateTimeValue");

                    b.Property<int>("PostId");

                    b.Property<int?>("SiteId");

                    b.Property<string>("StringValue");

                    b.Property<string>("Type")
                        .IsRequired();

                    b.Property<int?>("Value");

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.HasIndex("SiteId");

                    b.ToTable("PostClaims");
                });

            modelBuilder.Entity("Models.PostFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreationDate");

                    b.Property<string>("CreatorId")
                        .IsRequired();

                    b.Property<DateTime?>("ModifiedDate");

                    b.Property<int?>("PostId");

                    b.Property<int?>("SiteId");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.Property<string>("Type")
                        .IsRequired();

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("PostId");

                    b.HasIndex("SiteId");

                    b.ToTable("PostFiles");
                });

            modelBuilder.Entity("Models.PostText", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Number");

                    b.Property<int>("PostId");

                    b.Property<int>("Revision");

                    b.Property<int?>("SiteId");

                    b.Property<string>("Title");

                    b.Property<string>("Type")
                        .IsRequired();

                    b.Property<string>("Value")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.HasIndex("SiteId");

                    b.ToTable("PostTexts");
                });

            modelBuilder.Entity("Models.Site", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Domain");

                    b.Property<bool>("HasRegions");

                    b.Property<bool>("Private");

                    b.Property<int>("State");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Sites");
                });

            modelBuilder.Entity("Models.SiteAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("ActionDate");

                    b.Property<string>("ActorId")
                        .IsRequired();

                    b.Property<string>("Description");

                    b.Property<int>("Element");

                    b.Property<int>("SiteId");

                    b.Property<string>("Table")
                        .IsRequired();

                    b.Property<string>("Type")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("ActorId");

                    b.HasIndex("SiteId");

                    b.ToTable("SiteActions");
                });

            modelBuilder.Entity("Models.SiteClaim", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("DateTimeValue");

                    b.Property<int?>("ParentId");

                    b.Property<int>("SiteId");

                    b.Property<string>("StringValue");

                    b.Property<string>("Type")
                        .IsRequired();

                    b.Property<int?>("Value");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.HasIndex("SiteId");

                    b.ToTable("SiteClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Models.ApplicationUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Models.Page", b =>
                {
                    b.HasOne("Models.ApplicationUser", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Models.Page", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentId");

                    b.HasOne("Models.Site", "Site")
                        .WithMany("Pages")
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Models.PageClaim", b =>
                {
                    b.HasOne("Models.Page", "Page")
                        .WithMany("PageClaims")
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Models.Site", "Site")
                        .WithMany()
                        .HasForeignKey("SiteId");
                });

            modelBuilder.Entity("Models.Post", b =>
                {
                    b.HasOne("Models.ApplicationUser", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Models.Site", "Site")
                        .WithMany("Posts")
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Models.PostClaim", b =>
                {
                    b.HasOne("Models.Post", "Post")
                        .WithMany("PostClaims")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Models.Site", "Site")
                        .WithMany()
                        .HasForeignKey("SiteId");
                });

            modelBuilder.Entity("Models.PostFile", b =>
                {
                    b.HasOne("Models.ApplicationUser", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Models.Post", "Post")
                        .WithMany("PostFiles")
                        .HasForeignKey("PostId");

                    b.HasOne("Models.Site", "Site")
                        .WithMany()
                        .HasForeignKey("SiteId");
                });

            modelBuilder.Entity("Models.PostText", b =>
                {
                    b.HasOne("Models.Post", "Post")
                        .WithMany("PostTexts")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Models.Site", "Site")
                        .WithMany()
                        .HasForeignKey("SiteId");
                });

            modelBuilder.Entity("Models.SiteAction", b =>
                {
                    b.HasOne("Models.ApplicationUser", "Actor")
                        .WithMany()
                        .HasForeignKey("ActorId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Models.Site", "Site")
                        .WithMany("SiteActions")
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Models.SiteClaim", b =>
                {
                    b.HasOne("Models.SiteClaim", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentId");

                    b.HasOne("Models.Site", "Site")
                        .WithMany("SiteClaims")
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
