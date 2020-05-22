// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using Contracts;
using Framework;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace Services
{
    /// <summary>
    /// User extensions.
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// Get user groups.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="appctx"></param>
        /// <returns></returns>
        public static ICollection<UserGroup> UserGroups(this ApplicationUser user, WcmsAppContext appctx)
        {
            ICollection<SiteClaim> claims = null;
            ICollection<UserGroup> groups = new List<UserGroup>();
            if ((claims = appctx?.Site?.GetGroups()) != null)
            {
                foreach (SiteClaim claim in claims)
                {
                    if (claim.Id == user.Group1 || claim.Id == user.Group2
                        || claim.Id == user.Group3 || claim.Id == user.Group4
                        || claim.Id == user.Group5 || claim.Id == user.Group6
                        || claim.Id == user.Group7 || claim.Id == user.Group8
                        || claim.Id == user.Group9 || claim.Id == user.Group10)
                    {
                        groups.Add(new UserGroup { GroupId = claim.Id, User = user });
                    }
                }
            }
            return groups;
        }

        /// <summary>
        /// Set user tags.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static void UserGroups(this ApplicationUser user, IEnumerable<int> groups)
        {
            int length = (groups == null) ? 0 : groups.Count();
            user.Group1 = (length >= 1) ? groups.ElementAt(0) : -1;
            user.Group2 = (length >= 2) ? groups.ElementAt(1) : -1;
            user.Group3 = (length >= 3) ? groups.ElementAt(2) : -1;
            user.Group4 = (length >= 4) ? groups.ElementAt(3) : -1;
            user.Group5 = (length >= 5) ? groups.ElementAt(4) : -1;
            user.Group6 = (length >= 6) ? groups.ElementAt(5) : -1;
            user.Group7 = (length >= 7) ? groups.ElementAt(6) : -1;
            user.Group8 = (length >= 8) ? groups.ElementAt(7) : -1;
            user.Group9 = (length >= 9) ? groups.ElementAt(8) : -1;
            user.Group10 = (length >= 10) ? groups.ElementAt(9) : -1;
        }

        /// <summary>
        /// Get user regions.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="appctx"></param>
        /// <returns></returns>
        public static ICollection<UserRegion> UserRegions(this ApplicationUser user, WcmsAppContext appctx)
        {
            ICollection<SiteClaim> claims = null;
            ICollection<UserRegion> regions = new List<UserRegion>();
            if ((claims = appctx?.Site?.GetRegions()) != null)
            {
                foreach (SiteClaim claim in claims)
                {
                    if (user.Region1 == 0
                        || claim.Id == user.Region1 || claim.Id == user.Region2
                        || claim.Id == user.Region3 || claim.Id == user.Region4
                        || claim.Id == user.Region5 || claim.Id == user.Region6
                        || claim.Id == user.Region7 || claim.Id == user.Region8
                        || claim.Id == user.Region9 || claim.Id == user.Region10)
                    {
                        regions.Add(new UserRegion { RegionId = claim.Id, User = user });
                    }
                }
            }
            return regions;
        }

        /// <summary>
        /// Set user regions.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="regions"></param>
        /// <returns></returns>
        public static void UserRegions(this ApplicationUser user, IEnumerable<int> regions)
        {
            int length = (regions == null) ? 0 : regions.Count();
            user.Region1 = (length >= 1) ? regions.ElementAt(0) : -1;
            user.Region2 = (length >= 2) ? regions.ElementAt(1) : -1;
            user.Region3 = (length >= 3) ? regions.ElementAt(2) : -1;
            user.Region4 = (length >= 4) ? regions.ElementAt(3) : -1;
            user.Region5 = (length >= 5) ? regions.ElementAt(4) : -1;
            user.Region6 = (length >= 6) ? regions.ElementAt(5) : -1;
            user.Region7 = (length >= 7) ? regions.ElementAt(6) : -1;
            user.Region8 = (length >= 8) ? regions.ElementAt(7) : -1;
            user.Region9 = (length >= 9) ? regions.ElementAt(8) : -1;
            user.Region10 = (length >= 10) ? regions.ElementAt(9) : -1;
        }

        /// <summary>
        /// Get user file url
        /// </summary>
        /// <param name="user"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileUrl(this ApplicationUser user, string fileName)
        {
            if (user == null || fileName == null)
            {
                return string.Empty;
            }
            else if (fileName.StartsWith("http:") == true)
            {
                return fileName;
            }
            return $"{CRoute.RouteStaticFile_User}{user.Id}/{fileName}";
        }

        /// <summary>
        /// Get user cover url.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetCoverUrl(this ApplicationUser user, bool crop = false)
        {
            if (user?.Cover == null)
            {
                return string.Empty;
            }
            string cover = (crop == true) ? "cover.crop" : "cover";
            return (user.Cover.StartsWith("http:") == true)
                ? user.Cover
                : user.GetFileUrl($"{cover}{user.Cover}");
        }

        /// <summary>
        /// Get user root path.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="appctx"></param>
        /// <returns></returns>
        public static string GetRootPath(this ApplicationUser user, WcmsAppContext appctx)
        {
            return user?.GetFilePath(appctx, string.Empty);
        }

        /// <summary>
        /// Get user file path.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="appctx"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFilePath(this ApplicationUser user, WcmsAppContext appctx, string fileName)
        {
            string fileUrl = null;
            if (user.SiteId == 0 || user.SiteId != (appctx?.Site?.Id ?? 0) || (fileUrl = user.GetFileUrl(fileName)) == null)
            {
                return null;
            }
            IFileInfo filInf = appctx?.HostingEnvironment?.WebRootFileProvider?.GetFileInfo($"/{appctx.Site.Id}/{fileUrl}");
            appctx?.Log?.LogDebug("GetFilePath: {0} to {1}", fileName, filInf?.PhysicalPath);
            return filInf?.PhysicalPath;
        }

        /// <summary>
        /// Get user cover path.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="appctx"></param>
        /// <returns></returns>
        public static string GetCoverPath(this ApplicationUser user, WcmsAppContext appctx)
        {
            if (user?.Cover == null)
            {
                return null;
            }
            return (user.Cover.StartsWith("http:") == true)
                ? null
                : user.GetFilePath(appctx, $"cover{user.Cover}");
        }

        /// <summary>
        /// Init user folder.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="appctx"></param>
        public static bool InitDirectory(this ApplicationUser user, WcmsAppContext appctx)
        {
            string userPath = null;
            if ((userPath = user.GetRootPath(appctx)) == null)
            {
                appctx?.Log?.LogDebug("userPath=null");
                return false;
            }
            appctx?.Log?.LogDebug("userPath={0}", userPath);
            if (Directory.Exists(userPath) == false)
            {
                Directory.CreateDirectory(userPath);
            }
            return true;
        }
    }
}
