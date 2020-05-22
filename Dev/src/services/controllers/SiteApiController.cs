using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Services
{
    /// <summary>
    /// Site api.
    /// </summary>
    [Authorize]
    [Route("api/site")]
    public class SiteApiController : BaseController
    {
        /// <summary>
        /// Site provider.
        /// </summary>
        private SiteProvider provider { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public SiteApiController(Services.WcmsAppContext appContext)
            : base(appContext)
        {
            provider = new SiteProvider(appContext);
        }

        /// <summary>
        /// GET: /api/site
        /// Get the current site.
        /// </summary>
        /// <returns></returns>
        [Authorize(ClaimValueRole.Administrator)]
        [HttpGet]
        public JsonSite Get()
        {
            try
            {
                return (AppContext?.Site == null)
                    ? null
                    : new JsonSite(AppContext.Site);
            }
            catch (Exception e)
            {
                AppContext?.Log?.LogError("Exception getting the current site - HttpGet:/api/site: {0}", e.Message);
                return null;
            }
        }

        /// <summary>
        /// GET: /api/site/configuration
        /// Get the current site configuration:
        ///     * Menu
        ///     * Regions
        ///     * Categories
        ///     * Tags
        ///     * Groups
        /// </summary>
        /// <returns></returns>
        [HttpGet("configuration")]
        public JsonSiteSettings Configuration()
        {
            try
            {
                JsonSiteSettings conf = null;
                if ((conf = new JsonSiteSettings()) != null)
                {
                    // Add admin menus...
                    _AddMenus(conf);
                    // Add site name...
                    conf.Name = AppContext.Module?.Name;
                    // Add site regions...
                    conf.Regions = AppContext?.Site?.GetRegions(EOrderBy.Name)?.Select(sc => new JsonSiteClaim(sc))?.ToList();
                    conf.Regions.Insert(0, new JsonSiteClaim(AppContext?.Site?.GetRegion("all")));
                    // Add site categories...
                    conf.Categories = JsonSiteClaim.ToFlatList(AppContext?.Site?.GetCategories(null, true)?.Select(cat => new JsonSiteClaim(cat))?.ToList(), new List<JsonSiteClaim>());
                    // Add site tags...
                    conf.Tags = AppContext?.Site?.GetTags()?.Select(tag => new JsonSiteClaim(tag))?.ToList();
                    // Add site groups...
                    conf.Groups = AppContext?.Site?.GetGroups(AppContext?.User?.GroupsId())?.Select(grp => new JsonSiteClaim(grp))?.ToList();
                    // Add user role...
                    if (AppContext.User != null)
                    {
                        conf.UserRoles = AppContext.User.GetRoles();
                        conf.UserName = User.Identity.Name.Replace($"@{AppContext?.Site?.Id}", string.Empty);
                        conf.UserImg = "/lib/userimg.png";
                    }
                }
                //Thread.Sleep(10 * 1000);
                return conf;
            }
            catch (Exception e)
            {
                AppContext?.Log?.LogError("Exception getting the current site settings - HttpGet:/api/site/configuration: {0}", e.Message);
                return null;
            }
        }

        /// <summary>
        /// Add menus to site configuration.
        /// </summary>
        /// <param name="conf"></param>
        private void _AddMenus(JsonSiteSettings conf)
        {
            if ((conf.Menus = new List<JsonAdminMenu>()) != null)
            {
                bool admin = AppContext.User.HasRole(ClaimValueRole.Administrator);
                bool pub = AppContext.User.HasRole(ClaimValueRole.Publicator);
                bool cont = AppContext.User.HasRole(ClaimValueRole.Contributor);
                conf.Menus.Add(new JsonAdminMenu
                {
                    Name = "Articles",
                    Icon = "home",
                    IconMat = "home",
                    Url = "home.index",
                    UrlMat = "#/posts",
                    Active = ""
                });
                conf.Menus.Add(new JsonAdminMenu
                {
                    Name = "Calendrier",
                    Icon = "calendar-note",
                    IconMat = "date_range",
                    Url = "home.calendar",
                    UrlMat = "#/posts?type=calendar",
                    Active = "",
                    DefaultFilters = new Dictionary<string, string>()
                    {
                        { QueryFilter.TopCategorie, "13" },
                        { QueryFilter.ShowEventPostsOnly, "true" },
                        { QueryFilter.EndDate, DateTime.Now.ToString() }
                        // Comment out because we'll always show items of the sub catagories...
                        //{ QueryFilter.ShowChildsCategoriesPosts, "true" }
                    }
                });
                conf.Menus.Add(new JsonAdminMenu
                {
                    Name = "Médiathèque",
                    Icon = "photo_library",
                    IconMat = "photo_library",
                    Url = "home.index",
                    UrlMat = "#/posts?type=media",
                    Active = "",
                    DefaultFilters = new Dictionary<string, string>()
                    {
                        { QueryFilter.TopCategorie, "33" },
                        // Comment out because we'll always show items of the sub catagories...
                        //{ QueryFilter.ShowChildsCategoriesPosts, "true" }
                    }
                });
#if DEBUG
                if (admin == true || pub == true)
                {
                    conf.Menus.Add(new JsonAdminMenu
                    {
                        Name = "Pages",
                        Icon = "view-compact",
                        IconMat = "view_compact",
                        Url = "home.pages",
                        UrlMat = "#/pages",
                        Active = ""
                    });
                }
#endif
                //if (admin == true || pub == true || cont == true)
                //{
                //    conf.Menus.Add(new JsonAdminMenu
                //    {
                //        Name = "Posts",
                //        Icon = "view-list",
                //        Url = "home.posts",
                //        Active = ""
                //    });
                //}
                if (admin == true || pub == true)
                {
                    conf.Menus.Add(new JsonAdminMenu
                    {
                        Name = "Utilisateurs",
                        Icon = "accounts-list",
                        IconMat = "group",
                        Url = "home.users",
                        UrlMat = "#/users",
                        Active = ""
                    });
                }
                //conf.Menus.Add(new JsonAdminMenu
                //{
                //    Name = "Forum",
                //    Icon = "tab",
                //    Url = "home.forum",
                //    Active = ""
                //});
            }
        }

#if false
        // POST api/values
        [Authorize(ClaimValueRole.Administrator)]
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [Authorize(ClaimValueRole.Administrator)]
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [Authorize(ClaimValueRole.Administrator)]
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
#endif

        ///// <summary>
        ///// GET: api/values
        ///// Get sites.
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<IEnumerable<Site>> Get()
        //{
        //    return await provider?.Get();
        //    //if (await _authz.AuthorizeAsync(User, "SalesSenior"))
        //    //{
        //    //    return View("success");
        //    //}
        //    //return new ChallengeResult();
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="list"></param>
        ///// <param name="pages"></param>
        ///// <returns></returns>
        //private List<JsonAdminMenu> toJsonAdminMenuList(List<JsonAdminMenu> list, IEnumerable<Page> pages)
        //{
        //    if (pages != null)
        //    {
        //        foreach(Page page in pages)
        //        {
        //            JsonAdminMenu mn = new JsonAdminMenu
        //            {
        //                SitePage = true,
        //                Name = page.Title,
        //                Icon = "caret-right",
        //                Url = "home.posts",
        //                Active = "",
        //            };
        //            list.Add(mn);
        //            if (page.Childs != null && page.Childs.Count() > 0)
        //            {
        //                mn.Childs = new List<JsonAdminMenu>();
        //                toJsonAdminMenuList(mn.Childs, page.Childs);
        //            }
        //        }
        //    }
        //    return list;
        //}
    }
}
