using Contracts;
using Microsoft.Extensions.FileProviders;
using Models;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    /// <summary>
    /// Site extensions.
    /// </summary>
    public static class SiteExtensions
    {
        /// <summary>
        /// Get site root url.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="appctx"></param>
        /// <returns></returns>
        public static string GetRootUrl(this Site site)
        {
            return $"http://{site.Domain}";
        }

        /// <summary>
        /// Get site claims.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<SiteClaim> GetClaims(this Site site, string type = null)
        {
            return (string.IsNullOrWhiteSpace(type) == true)
                ? site?.SiteClaims?.OrderBy(sc => sc.StringValue)
                : site?.SiteClaims?.Where(s => s.Type == type)?.OrderBy(sc => sc.StringValue);
        }

        /// <summary>
        /// Get site claim by id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SiteClaim GetClaim(this Site site, int id)
        {
            return site?.SiteClaims?.FirstOrDefault(s => s.Id == id);
        }

        /// <summary>
        /// Get all group Id.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static int? GetAllGroupId(this Site site)
        {
            if (site?.SiteClaims != null)
            {
                foreach (SiteClaim stClaim in site?.SiteClaims)
                {
                    if (stClaim.Type == SiteClaimType.Group)
                    {
                        if (stClaim.StringValue == "All")
                        {
                            return stClaim.Id;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get site groups.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static ICollection<Models.SiteClaim> GetGroups(this Site site)
        {
            return site?.SiteClaims?.Where(s => s.Type == SiteClaimType.Group)?.OrderBy(sc => sc.StringValue)?.ToList();
        }

        /// <summary>
        /// Get site groups filtered by groups input.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static ICollection<Models.SiteClaim> GetGroups(this Site site, List<int> groups)
        {
            if (groups == null || groups.Count == 0)
            {
                return null;
            }
            // Check for All group...
            int? allGroup = site.GetAllGroupId();
            if (allGroup != null)
            {
                return site.GetGroups();
            }
            // Return filtered group based on groups.
            return site?.SiteClaims?.Where(s => s.Type == SiteClaimType.Group && groups.Contains(s.Id))?.OrderBy(sc => sc.StringValue)?.ToList();
        }

        /// <summary>
        /// Get site group by id.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SiteClaim GetGroup(this Site site, int id)
        {
            return site?.SiteClaims?.FirstOrDefault(s => s.Type == SiteClaimType.Group && s.Id == id);
        }

        /// <summary>
        /// Get site group by name.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SiteClaim GetGroup(this Site site, string name)
        {
            return (string.IsNullOrWhiteSpace(name) == true)
                ? null
                : site?.SiteClaims?.FirstOrDefault(s => s.Type == SiteClaimType.Group && s.StringValue.ToLower() == name.ToLower());
        }

        /// <summary>
        /// Get site regions.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static ICollection<SiteClaim> GetRegions(this Site site, EOrderBy orderby = EOrderBy.Id)
        {
            if (orderby == EOrderBy.Name)
                return site?.SiteClaims?.Where(sc => sc.Type == SiteClaimType.Region).OrderBy(sc => sc.StringValue)?.ToList();
            return site?.SiteClaims?.Where(s => s.Type == SiteClaimType.Region).OrderBy(sc => sc.Id)?.ToList();
        }

        /// <summary>
        /// Get site regions as string.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static string GetRegionsAsString(this Site site)
        {
            return _RegionsToString(site?.SiteClaims);
        }
        
        /// <summary>
        /// Get site region by ids.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static ICollection<SiteClaim> GetRegions(this Site site, List<int> ids)
        {
            return (ids == null || ids.Count == 0) 
                ? null
                : site?.SiteClaims?.Where(s => s.Type == SiteClaimType.Region && ids.Contains(s.Id))?.ToList();
        }

        /// <summary>
        /// Get site regions as string.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public static string GetRegionsAsString(this Site site, List<int> ids)
        {
            return _RegionsToString(site.GetRegions(ids));
        }

        /// <summary>
        /// Get site region by id.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SiteClaim GetRegion(this Site site, int id)
        {
            return site?.SiteClaims?.FirstOrDefault(s => s.Type == SiteClaimType.Region && s.Id == id);
        }

        /// <summary>
        /// Get site region by name.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SiteClaim GetRegion(this Site site, string name)
        {
            if (string.IsNullOrWhiteSpace(name) == true)
            {
                return null;
            }
            else if (name.ToLower() == "all")
            {
                return new SiteClaim()
                {
                    Id = -1,
                    Type = SiteClaimType.Region,
                    StringValue = "All",
                };
            }
            else if (name.ToLower() == "default")
            {
                SiteClaim[] regions = site?.SiteClaims?.Where(s => s.Type == SiteClaimType.Region).ToArray();
                if (regions != null && regions.Count() > 0)
                {
                    return regions[0];
                }
                return null;
            }
            else {
                return site?.SiteClaims?.FirstOrDefault(s => s.Type == SiteClaimType.Region && s.StringValue.ToLower() == name.ToLower());
            }
        }

        /// <summary>
        /// Get all site categories.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static ICollection<SiteClaim> GetCategories(this Site site)
        {
            return site.GetCategories(-1);
        }

        /// <summary>
        /// Get site categories.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static ICollection<SiteClaim> GetCategories(this Site site, int? parent, bool recursive = false)
        {
            ICollection<SiteClaim> cats = null;
            if (parent == null)
            {
                cats = site?.SiteClaims?.Where(s => s.Type == SiteClaimType.Categorie && s.ParentId == null)?.OrderBy(sc => sc.StringValue)?.ToList();
            }
            else if (parent.Value == -1)
            {
                cats = site?.SiteClaims?.Where(s => s.Type == SiteClaimType.Categorie)?.OrderBy(sc => sc.StringValue)?.ToList();
                recursive = false;
            }
            else
            {
                cats = site?.SiteClaims?.Where(s => s.Type == SiteClaimType.Categorie && s.ParentId != null && s.ParentId == parent.Value)?.OrderBy(sc => sc.StringValue)?.ToList();
            }
            if (recursive == true
                && cats != null && cats.Count() != 0)
            {
                foreach (SiteClaim cat in cats)
                {
                    cat.Childs = GetCategories(site, cat.Id, true);
                }
                //await _GetChilds(onlyInMenu, pages);
            }
            return cats;
        }

        /// <summary>
        /// Get site categories as an id list.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static List<int> GetCategoriesAsIdList(this Site site, int? parent, bool recursive = false)
        {
            ICollection<SiteClaim> cats = site.GetCategories(parent, recursive);
            return cats?.Select(c => c.Id)?.ToList();
        }

        /// <summary>
        /// Get site category by id.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SiteClaim GetCategory(this Site site, int id)
        {
            return site?.SiteClaims?.FirstOrDefault(s => s.Type == SiteClaimType.Categorie && s.Id == id);
        }

        /// <summary>
        /// Get site tags.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static ICollection<SiteClaim> GetTags(this Site site)
        {
            return site?.SiteClaims?.Where(s => s.Type == SiteClaimType.Tag)?.OrderBy(sc => sc.StringValue)?.ToList();
        }

        /// <summary>
        /// Get site tag by id.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SiteClaim GetTag(this Site site, int id)
        {
            return site?.SiteClaims?.FirstOrDefault(s => s.Type == SiteClaimType.Tag && s.Id == id);
        }

        /// <summary>
        /// Get site root path.
        /// </summary>
        /// <param name=""></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetRootPath(this Site site, WcmsAppContext appctx)
        {
            IFileInfo filInf = appctx?.HostingEnvironment?.WebRootFileProvider?.GetFileInfo($"/{site.Id}");
            return filInf?.PhysicalPath;
        }

        /// <summary>
        /// Get meta title.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static string GetMetaTitle(this Site site, WcmsAppContext appctx, string title)
        {
            string metaTitle = (appctx?.Region?.StringValue != null)
                ? Framework.String.RemoveAccents(title) /*+ ((ForAllRegion == true) ? "@" : "")*/ + " - " + appctx.Region.StringValue.ToLower()
                : Framework.String.RemoveAccents(title) /*+ ((ForAllRegion == true) ? "@" : "")*/;
            return Framework.String.reSize(metaTitle, 83)?.Replace("...", string.Empty);
        }

        /// <summary>
        /// Get meta description.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static string GetMetaDescription(this Site site, WcmsAppContext appctx, string title)
        {
            string metaDescription = (appctx?.Region?.StringValue != null)
                ? $"{title?.ToLower()} - {appctx?.Region.StringValue?.ToLower()}"
                : title?.ToLower();
            return Framework.String.reSize(metaDescription, 83).Replace("...", string.Empty);
        }

        /// <summary>
        /// Get meta keyword.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static string GetMetaKeywords(this Site site, WcmsAppContext appctx, string title)
        {
            string keyword = appctx.Module?.LongName?.ToLower() + ", ";
            if (appctx?.Region?.StringValue != null)
            {
                keyword += appctx.Region.StringValue.ToLower() + ", ";
            }
            keyword += Framework.String.AsKeyWord(title);
            return keyword;
        }

        /// <summary>
        /// Region list to string
        /// </summary>
        /// <param name="siteClaims"></param>
        /// <returns></returns>
        private static string _RegionsToString(ICollection<SiteClaim> siteClaims)
        {
            string asString = string.Empty;
            if (siteClaims != null && siteClaims.Count > 0)
            {
                bool first = true;
                foreach (SiteClaim claim in siteClaims)
                {
                    if (claim.Type == SiteClaimType.Region
                        && string.IsNullOrWhiteSpace(claim.StringValue) == false)
                    {
                        if (first == false)
                        {
                            asString += ", ";
                        }
                        first = false;
                        asString += claim.StringValue;
                    }
                }
            }
            return asString;
        }
    }
}
