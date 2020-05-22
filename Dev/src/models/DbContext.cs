// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using contracts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Models
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        // Test purpose...
        public static int NbCall = 0;
        //// Northwind related.
        //public static readonly string StoreName = "Northwind";

        /// <summary>
        /// Db context constructor.
        /// Migration:
        /// PM> Add-Migration M1 -Context AppDbContext -Project models
        /// PM> Remove-Migration -Context AppDbContext -Project models
        /// PM> Update-Database -Context AppDbContext -Project models
        /// 
        /// Publish error:
        /// C:\Program Files (x86)\MSBuild\Microsoft\VisualStudio\v14.0\DotNet\Microsoft.DotNet.Publishing.targets(408,5): Error : Your target project 'www' doesn't match your migrations assembly 'models'. Either change your target project or change your migrations assembly.
        /// C:\Program Files (x86)\MSBuild\Microsoft\VisualStudio\v14.0\DotNet\Microsoft.DotNet.Publishing.targets(408,5): Error : Change your migrations assembly by using DbContextOptionsBuilder. E.g. options.UseSqlServer(connection, b => b.MigrationsAssembly("www")). By default, the migrations assembly is the assembly containing the DbContext.
        /// C:\Program Files (x86)\MSBuild\Microsoft\VisualStudio\v14.0\DotNet\Microsoft.DotNet.Publishing.targets(408,5): Error : Change your target project to the migrations project by using the Package Manager Console's Default project drop-down list, or by executing "dotnet ef" from the directory containing the migrations project.
        /// </summary>
        public AppDbContext()
        {
            NbCall++;
        }

        /// <summary>
        /// Db context constructor.
        /// </summary>
        /// <param name="options"></param>
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }

        //// Concurrency model context.
        //public DbSet<Team> Teams { get; set; }
        //public DbSet<Driver> Drivers { get; set; }
        //public DbSet<Sponsor> Sponsors { get; set; }
        //public DbSet<Engine> Engines { get; set; }
        //public DbSet<EngineSupplier> EngineSuppliers { get; set; }

        //// Northwind model context.
        //public virtual DbSet<Customer> Customers { get; set; }
        //public virtual DbSet<Employee> Employees { get; set; }
        //public virtual DbSet<Order> Orders { get; set; }
        //public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        //public virtual DbSet<Product> Products { get; set; }

        // Blog model context.
        public DbSet<Site> Sites { get; set; }
        public DbSet<SiteClaim> SiteClaims { get; set; }
        public DbSet<SiteAction> SiteActions { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<PageClaim> PageClaims { get; set; }
#       if !DENORMALIZE
        public DbSet<PageRegion> PageRegions { get; set; }
        public DbSet<PageGroup> PageGroups { get; set; }
        public DbSet<PageCategory> PageCategorys { get; set; }
        public DbSet<PageTag> PageTags { get; set; }
#       endif
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostClaim> PostClaims { get; set; }
        public DbSet<PostText> PostTexts { get; set; }
        public DbSet<PostFile> PostFiles { get; set; }
#       if !DENORMALIZE
        public DbSet<PostGroup> PostGroups { get; set; }
        public DbSet<PostRegion> PostRegions { get; set; }
        public DbSet<PostCategory> PostCategorys { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
#       endif
        public DbSet<PublicEmail> PublicEmails { get; set; }

        // Test purpose...
        public TimeSpan TimeSpan { get; set; }

        /// <summary>
        /// Configure the Models.
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            if (Global.UseMySql == true)
            {
                //// InvalidOperationException: No mapping to a relational type can be found for the CLR type 'Microsoft.EntityFrameworkCore.Metadata.Internal.Property' 
                //// http://forums.mysql.com/read.php?38,649736,649928#msg-649928
                //// I have the same issue, this problem is caused by the "LockoutEnd" field. In previous version of Identity, it's a "DateTime", in asp.net core, it's "DateTimeOffset". 
                //// You can configure it with e.Property(p => p.LockoutEnd).HasColumnType("datetime") in OnModelCreating method. 
                //// But when lockout occured, it will throw an exception, mysql can not accept "DateTimeOffset"(such as "2016/8/31 4:20:59 +00:00").
                //// You can solve this by rewrite the UserStore and IdentityUser, or just wait mysql provider update.
                //builder.Entity<ApplicationUser>(b =>
                //{
                //    b.Property(p => p.LockoutEnd).HasColumnType("datetime");
                //});
                builder.Entity<Site>(b =>
                {
                    b.Property(p => p.Title).HasColumnType("text");
                });
                builder.Entity<SiteClaim>(b =>
                {
                    b.Property(p => p.StringValue).HasColumnType("text");
                });
                builder.Entity<SiteAction>(b =>
                {
                    b.Property(p => p.Description).HasColumnType("mediumtext");
                });
                builder.Entity<Page>(b =>
                {
                    b.Property(p => p.Title).HasColumnType("text");
                });
                builder.Entity<PageClaim>(b =>
                {
                    b.Property(p => p.StringValue).HasColumnType("text");
                });
                builder.Entity<Post>(b =>
                {
                    b.Property(p => p.Title).HasColumnType("text");
                    b.Property(p => p.Text).HasColumnType("mediumtext");
                });
                builder.Entity<PostText>(b =>
                {
                    b.Property(p => p.Title).HasColumnType("text");
                    b.Property(p => p.Value).HasColumnType("mediumtext");
                });
                builder.Entity<PostFile>(b =>
                {
                    b.Property(p => p.Title).HasColumnType("text");
                    b.Property(p => p.Url).HasColumnType("text");
                });
                builder.Entity<PostClaim>(b =>
                {
                    b.Property(p => p.StringValue).HasColumnType("text");
                });
                builder.Entity<IdentityUserClaim<string>>(b =>
                {
                    b.Property(p => p.ClaimValue).HasColumnType("text");
                });
            }

            //builder.Entity<Employee>().Property(i => i.EmployeeID).ValueGeneratedOnAdd();//.GenerateValuesOnAdd(generateValues: false);


            builder.Entity<PublicEmail>().ToTable("PublicEmails");

            builder.Entity<Page>().ToTable("Pages");
            builder.Entity<PageClaim>().ToTable("PageClaims");

            builder.Entity<Post>().ToTable("Posts");
            builder.Entity<PostClaim>().ToTable("PostClaims");
            builder.Entity<PostFile>().ToTable("PostFiles");
            builder.Entity<PostText>().ToTable("PostTexts");

            builder.Entity<Site>().ToTable("Sites");
            builder.Entity<SiteAction>().ToTable("SiteActions");
            builder.Entity<SiteClaim>().ToTable("SiteClaims");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder
#if DEBUG
                // Enable sensitive data in log...
                .EnableSensitiveDataLogging()
#endif
                // Disabling client evaluation...
                .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning));
    }
}
