// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using contracts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Models;
//using MySQL.Data.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Services;

namespace UnitTests.Core
{
    /// <summary>
    /// Utils for data base context creation.
    /// </summary>
    public static class DbUtil
    {
        public static readonly string ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=wwwut;Trusted_Connection=True;MultipleActiveResultSets=true";
        public static readonly string ConnectionStringWww = "Server=(localdb)\\mssqllocaldb;Database=www;Trusted_Connection=True;MultipleActiveResultSets=true";
        public static readonly string ConnectionStringMy = "server=localhost;user id=root;password=root;port=3306;database=wwwut;";
        public static readonly string ConnectionStringWwwMy = "server=localhost;user id=root;password=root;port=3306;database=www;";
        /// <summary>
        /// Create a new data base context.
        /// </summary>
        /// <returns></returns>
        public static AppDbContext CreateDbContext(bool prod = false)
        {
            return CreateDbContext<AppDbContext>(prod);
        }
        
        /// <summary>
        /// Is the specified user member of the specified side.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool IsSiteUser(int siteId, ApplicationUser user)
        {
            if (user == null)
            {
                return false;
            }
            return user.SiteId == siteId;
        }

        /// <summary>
        /// Get site by Id.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static async Task<Site> GetSite(int siteId, AppDbContext dbContext = null)
        {
            Site site = null;
            // Get site...
            if (dbContext != null)
            {
                site = await _GetSite(siteId, dbContext);
            }
            else
            {
                using (AppDbContext dbctx = CreateDbContext(false))
                {
                    site = await _GetSite(siteId, dbctx);
                }
            }
            Assert.NotEqual(null, site);
            Assert.Equal(siteId, site.Id);
            return site;
        }

        /// <summary>
        /// Get site by domain.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static async Task<Site> GetSite(string domain, AppDbContext dbContext = null)
        {
            Site site = null;
            // Get site...
            if (dbContext != null)
            {
                site = await _GetSite(domain, dbContext);
            }
            else
            {
                using (AppDbContext dbctx = CreateDbContext(false))
                {
                    site = await _GetSite(domain, dbctx);
                }
            }
            Assert.NotEqual(null, site);
            Assert.Equal(domain, site.Domain);
            return site;
        }

        /// <summary>
        /// Get site regions.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static async Task<List<SiteClaim>> GetRegions(int siteId, AppDbContext dbContext = null)
        {
            List<SiteClaim> regions = null;
            // Get regions...
            if (dbContext != null)
            {
                regions = await _GetRegions(siteId, dbContext);
            }
            else
            {
                using (AppDbContext dbctx = CreateDbContext(false))
                {
                    regions = await _GetRegions(siteId, dbctx);
                }
            }
            Assert.NotEqual(null, regions);
            Assert.NotEqual(0, regions.Count);
            return regions;
        }

        /// <summary>
        /// Get site users.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static async Task<List<ApplicationUser>> GetUsers(int siteId, AppDbContext dbContext = null)
        {
            List<ApplicationUser> users = null;
            // Get users...
            if (dbContext != null)
            {
                users = await _GetUsers(siteId, dbContext);
            }
            else
            {
                using (AppDbContext dbctx = CreateDbContext(false))
                {
                    users = await _GetUsers(siteId, dbctx);
                }
            }
            Assert.NotEqual(null, users);
            Assert.NotEqual(0, users.Count);
            // Add anonymous user.
            users.Add(null);
            return users;
        }

        /// <summary>
        /// Get site data.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="allUsers"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static async Task<SiteData> GetSiteData(int siteId, bool allUsers = false, AppDbContext dbContext = null)
        {
            SiteData siteData = new SiteData();
            siteData.site = await DbUtil.GetSite(siteId, dbContext);
            Assert.NotEqual(null, siteData.site);
            siteData.nullSiteCats = new List<SiteClaim>
            {
                new SiteClaim
                {
                    Id = 1,
                }
            };
            siteData.nullSiteTags = new List<SiteClaim>
            {
                new SiteClaim
                {
                    Id = 1,
                }
            };
            siteData.regions = await DbUtil.GetRegions(siteId, dbContext);
            Assert.NotEqual(null, siteData.regions);
            siteData.users = await DbUtil.GetUsers((allUsers == false) ? siteId : -1, dbContext);
            Assert.NotEqual(null, siteData.users);

            return siteData;
        }

        /// <summary>
        /// Get user groups and add a null group to the list (id = -123).
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<int> GetUserGroupsFilter(ApplicationUser user)
        {
            List<int> groupsId = null;
            if ((groupsId = user?.GroupsId()) == null)
            {
                groupsId = new List<int>();
            }
            // Add the null group to the list...
            groupsId?.Add(-123);
            return groupsId;
        }

        private static TContext CreateDbContext<TContext>(bool prod) where TContext : Microsoft.EntityFrameworkCore.DbContext, new()
        {
            var serviceProvider = ConfigureDbServices<TContext>(prod)?.BuildServiceProvider();
            return serviceProvider?.GetRequiredService<TContext>();
        }
        
        private static IServiceCollection ConfigureDbServices<TContext>(bool prod, IServiceCollection services = null) where TContext : Microsoft.EntityFrameworkCore.DbContext
        {
            if (services == null)
            {
                services = new ServiceCollection();
            }
            // Add framework services.
            if (Global.UseMySql == true)
            {
                services.AddDbContext<TContext>(options =>
                    options.UseMySql((prod == true) ? ConnectionStringWwwMy : ConnectionStringMy));
            }
            else
            {
                services.AddDbContext<TContext>(options =>
                        options.UseSqlServer((prod == true) ? ConnectionStringWww : ConnectionString));
            }
            return services;
        }

        private static async Task<Site> _GetSite(int siteId, AppDbContext dbctx)
        {
            return await dbctx.Sites
                .Where(st => st.Id == siteId)
                .FirstAsync();
        }

        private static async Task<Site> _GetSite(string domain, AppDbContext dbctx)
        {
            return await dbctx.Sites
                .Where(st => st.Domain == domain)
                .FirstAsync();
        }

        private static async Task<List<SiteClaim>> _GetRegions(int siteId, AppDbContext dbctx)
        {
            return await dbctx.SiteClaims
                .Where(st => st.Type == SiteClaimType.Region && st.SiteId == siteId)
                .ToListAsync();
        }

        private static async Task<List<ApplicationUser>> _GetUsers(int siteId, AppDbContext dbctx)
        {
            if (siteId == -1)
            {
                return await dbctx.Users
                    .Include(u => u.Claims)
                    .ToListAsync();
            }
            else
            {
                return await dbctx.Users
                    .Where(u => u.SiteId == siteId)
                    .Include(u => u.Claims)
                    .ToListAsync();
            }
        }
    }
}
