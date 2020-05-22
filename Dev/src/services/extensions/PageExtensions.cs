// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using Models;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    /// <summary>
    /// Page extensions.
    /// </summary>
    public static class PageExtensions
    {
        /// <summary>
        /// Get page group ids.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static List<int> GroupsId(this Page page)
        {
            // Checking...
            if (page == null)
            {
                return null;
            }
            List<int> dataGroups = new List<int>();
            if (page.Group1 != -1) dataGroups.Add(page.Group1);
            if (page.Group2 != -1) dataGroups.Add(page.Group2);
            if (page.Group3 != -1) dataGroups.Add(page.Group3);
            if (page.Group4 != -1) dataGroups.Add(page.Group4);
            if (page.Group5 != -1) dataGroups.Add(page.Group5);
            if (page.Group6 != -1) dataGroups.Add(page.Group6);
            if (page.Group7 != -1) dataGroups.Add(page.Group7);
            if (page.Group8 != -1) dataGroups.Add(page.Group8);
            if (page.Group9 != -1) dataGroups.Add(page.Group9);
            if (page.Group10 != -1) dataGroups.Add(page.Group10);
            return dataGroups;
        }

        /// <summary>
        /// Get page claims.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<PageClaim> GetClaims(this Page page, string type = null)
        {
            return (string.IsNullOrWhiteSpace(type) == true)
                ? page?.PageClaims
                : page?.PageClaims?.Where(s => s.Type == type);
        }

        /// <summary>
        /// Get page claim by id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PageClaim GetClaim(this Page page, int id)
        {
            return page?.PageClaims?.FirstOrDefault(s => s.Id == id);
        }

        ///// <summary>
        ///// Get page groups.
        ///// </summary>
        ///// <param name="page"></param>
        ///// <returns></returns>
        //public static ICollection<Models.Claim> GetGroups(this Page page)
        //{
        //    return page?.PageClaims?.Where(s => s.Type == ClaimType.Group).Cast<Models.Claim>()?.ToList();
        //}

        ///// <summary>
        ///// Get page regions.
        ///// </summary>
        ///// <param name="page"></param>
        ///// <returns></returns>
        //public static ICollection<PageClaim> GetRegions(this Page page)
        //{
        //    return page?.PageClaims?.Where(s => s.Type == ClaimType.Region)?.ToList();
        //}

        ///// <summary>
        ///// Get page categories.
        ///// </summary>
        ///// <param name="page"></param>
        ///// <returns></returns>
        //public static ICollection<PageClaim> GetCategories(this Page page)
        //{
        //    return page?.PageClaims?.Where(s => s.Type == ClaimType.Categorie)?.ToList();
        //}

        /// <summary>
        /// Get page categories as a List of ID.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static List<int> GetCategoriesAsIdList(this Page page)
        {
#           if !DENORMALIZE
            return page?.PageCategorys?.Select(c => c.Id)?.ToList();
#           else
            List<int> ids = new List<int>();
            if (page != null)
            {
                if (page.Category1 != -1) ids.Add(page.Category1);
                if (page.Category2 != -1) ids.Add(page.Category2);
                if (page.Category3 != -1) ids.Add(page.Category3);
                if (page.Category4 != -1) ids.Add(page.Category4);
                if (page.Category5 != -1) ids.Add(page.Category5);
                if (page.Category6 != -1) ids.Add(page.Category6);
                if (page.Category7 != -1) ids.Add(page.Category7);
                if (page.Category8 != -1) ids.Add(page.Category8);
                if (page.Category9 != -1) ids.Add(page.Category9);
                if (page.Category10 != -1) ids.Add(page.Category10);
            }
            return ids;
#           endif
        }


        /// <summary>
        /// Get page categories and childs categories as a List of ID.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static List<int> GetCategoriesAndChilsAsIdList(this Page page, WcmsAppContext context)
        {
            List<int> ids = new List<int>();
            if (page != null)
            {
                if (page.Category1 != -1) ids.Add(page.Category1);
                if (page.Category2 != -1) ids.Add(page.Category2);
                if (page.Category3 != -1) ids.Add(page.Category3);
                if (page.Category4 != -1) ids.Add(page.Category4);
                if (page.Category5 != -1) ids.Add(page.Category5);
                if (page.Category6 != -1) ids.Add(page.Category6);
                if (page.Category7 != -1) ids.Add(page.Category7);
                if (page.Category8 != -1) ids.Add(page.Category8);
                if (page.Category9 != -1) ids.Add(page.Category9);
                if (page.Category10 != -1) ids.Add(page.Category10);
            }
            List<int> idsAndChilds = new List<int>();
            foreach(int id in ids)
            {
                idsAndChilds.Add(id);
                List<int> childIds = context?.Site?.GetCategoriesAsIdList(id, true);
                if (childIds != null && childIds.Count > 0)
                {
                    idsAndChilds.AddRange(childIds);
                }
            }
            return idsAndChilds;
        }

        /// <summary>
        /// Get page filtering settings.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static string GetPageFiltering(this Page page)
        {
            if (page != null)
            {
                List<PageClaim> fltrs = page.GetClaims(PageClaimType.PostFiltering)?.ToList();
                if ((fltrs?.Count ?? 0) >= 1)
                {
                    return fltrs[0]?.StringValue;
                }
            }
            return null;
        }

        ///// <summary>
        ///// Get page tags.
        ///// </summary>
        ///// <param name="page"></param>
        ///// <returns></returns>
        //public static ICollection<PageClaim> GetTags(this Page page)
        //{
        //    return page?.PageClaims?.Where(s => s.Type == ClaimType.Tag)?.ToList();
        //}

        /// <summary>
        /// Get page url.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="appctx"></param>
        /// <returns></returns>
        public static string GetUrl(this Page page, WcmsAppContext appctx)
        {
            if  (string.IsNullOrWhiteSpace(page.Controller) == true || string.IsNullOrWhiteSpace(page.Action) == true)
            {
                return null;
            }
            string region = appctx?.Region?.StringValue;
            string url = $"/{Framework.String.ToUrl(page.Title)}/pg{page?.Id ?? 0}";
            return (region != null)
                ? $"/{Framework.String.ToUrl/*Framework.String.RemoveAccents*/(region).ToLower()}{url}"
                : url;
        }

        /// <summary>
        /// Get category url.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="appctx"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static string GetCategoryUrl(this Page page, WcmsAppContext appctx, SiteClaim category, string extension = null)
        {
            string region = appctx?.Region?.StringValue;
            string url = $"/{Framework.String.ToUrl(category.StringValue)}/pg{page?.Id ?? 0}/ct{category?.Id ?? 0}{extension}";
            return (region != null)
                ? $"/{Framework.String.ToUrl/*Framework.String.RemoveAccents*/(region).ToLower()}{url}"
                : url;
        }
    }
}
