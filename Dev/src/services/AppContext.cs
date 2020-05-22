using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Application context.
    /// </summary>
    public class WcmsAppContext
    {
        private static int _InstNum = 0;
        private bool _InvalidUserSite = false;
        /// <summary>
        /// </summary>
        private ApplicationUser _User = null;
        /// <summary>
        /// </summary>
        private readonly PerformanceProvider _PerfProvider = null;
        /// <summary>
        /// Route controller.
        /// </summary>
        private string _Controller = null;
        /// <summary>
        /// Route action.
        /// </summary>
        private string _Action = null;

        /// <summary>
        /// The app context constructor.
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        /// <param name="conf"></param>
        /// <param name="signInManager"></param>
        /// <param name="userManager"></param>
        /// <param name="appBbContext"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="perfProvider"></param>
        public WcmsAppContext(IHostingEnvironment hostingEnvironment,
            IConfigurationRoot conf,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, 
            Models.AppDbContext appBbContext,
            ILoggerFactory loggerFactory,
            PerformanceProvider perfProvider)
        {
            _InstNum++;
            HostingEnvironment = hostingEnvironment;
            Configuration = conf;
            SignInManager = signInManager;
            UserManager = userManager;
            AppDbContext = appBbContext;
            LoggerFactory = loggerFactory;
            Log = LoggerFactory?.CreateLogger(typeof(WcmsAppContext).FullName);
            _PerfProvider = perfProvider;
            // Init...
            RouteCatId = 0;
        }

        /// <summary>
        /// Host environment.
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; private set; }

        /// <summary>
        /// Host configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; private set; }

        /// <summary>
        /// The user signin manager.
        /// </summary>
        public SignInManager<ApplicationUser> SignInManager { get; private set; }

        /// <summary>
        /// The user manager provider.
        /// </summary>
        public UserManager<ApplicationUser> UserManager { get; private set; }
        
        /// <summary>
        /// The authorization provider.
        /// </summary>
        public IAuthorizationService AuthzProvider { get; private set; }

        /// <summary>
        /// Db context.
        /// </summary>
        public Models.AppDbContext AppDbContext { get; private set; }

        /// <summary>
        /// Logger factory.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; private set; }
        /// <summary>
        /// Logger.
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        /// Request host.
        /// </summary>
        public HostString Host { get; private set; }
        
        /// <summary>
        /// Route region name.
        /// </summary>
        public string RouteRegionName { get; set; }

        /// <summary>
        /// Last execution exception.
        /// </summary>
        public string Exception { get; set; }

        /// <summary>
        /// Route page id.
        /// </summary>
        public string RoutePageId { get; private set; }

        /// <summary>
        /// Route post id.
        /// </summary>
        public string RoutePostId { get; private set; }

        /// <summary>
        /// Route cat id.
        /// </summary>
        public int RouteCatId { get; private set; }

        /// <summary>
        /// Site module.
        /// </summary>
        public IModule Module { get; private set; }
        
        /// <summary>
        /// Controller module.
        /// </summary>
        public ModuleController ModuleController { get; private set; }

        /// <summary>
        /// Principal of the context.
        /// </summary>
        public ClaimsPrincipal UserPrincipal { get; private set; }
        /// <summary>
        /// Get the user of the context.
        /// </summary>
        public ApplicationUser User
        {
            get
            {
                return _User;
            }
        }
        ///// <summary>
        ///// User Id.
        ///// </summary>
        //public string UserId { get; private set; }
        ///// <summary>
        ///// User Name.
        ///// </summary>
        //public string UserName { get; private set; }

        /// <summary>
        /// Site of the context.
        /// </summary>
        public Site Site { get; private set; }

        /// <summary>
        /// Region of the context.
        /// </summary>
        public SiteClaim Region { get; private set; }
        public int RegionCount { get; private set; }

        /// <summary>
        /// Page of the context.
        /// </summary>
        public Page Page { get; private set; }

        /// <summary>
        /// Post of the context.
        /// </summary>
        public Post Post { get; private set; }

        /// <summary>
        /// Cat of the context.
        /// </summary>
        public SiteClaim Cat { get; private set; }

        ///// <summary>
        ///// Filter: Post of all regions.
        ///// </summary>
        //public bool PostOfAllRegions { get; private set; }

        // Test purpose...
        public TimeSpan TimeSpan { get; private set; }

        /// <summary>
        /// Init the application context.
        /// Return -1: Access denied.
        /// Return  0: When the site doesn't exist.
        /// Return  1: When the site exist.
        /// Return  2: When the site exist but the region doesn't exist.
        /// Return  3: When the site module cannot be found.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="authzProvider"></param>
        /// <returns></returns>
        public async Task<int> InitSiteAsync(HttpContext context, IAuthorizationService authzProvider)
        {
            int res = -1;
            // Clean...
            _Clean();
            // Checking...
            if (context == null)
            {
                Log?.LogError("Failed to init the app context: Invalid context.");
                // Trace performance...
                AddPerfLog("AppContext::InitSiteAsync::Invalid provider");
                return res;
            }
            // Init...
            AuthzProvider = authzProvider;

            // En cours: SiteId, group and region dans les data utilisateur
            // ==> a relire de la base ==> rajouter un cache!!!
            // Init context properties...
            Host = context.Request.Host;
            UserPrincipal = context.User;
            if (UserPrincipal != null)
            {
                await GetUser();
                if (_InvalidUserSite == true)
                {
                    Log?.LogError("Failed to init the app context: Invalid user site.");
                    // Trace performance...
                    AddPerfLog("AppContext::InitSiteAsync::Invalid user site");
                    return -1;
                }
                // Trace performance...
                AddPerfLog("AppContext::InitSiteAsync::User information");
            }
            AppDbContext.TimeSpan = TimeSpan = DateTime.Now.TimeOfDay;
            SiteProvider siteProvider = new SiteProvider(this);

            // Get site module...
            if ((Module = Factory.GetModule(context.Request.Host.Value)) == null)
            {
                // The site module cannot be found.
                res = 3;
            }
            // Load site from domain...
            else if ((Site = await siteProvider?.Get(Module.Domain)) != null)
            {
                // The site exist and the user can view it.
                res = 1;
            }
            else if (siteProvider.Exist != 1)
            {
                // The site doesn't exist.
                res = 0;
            }
            // The site cannot be viewed by the user.
            // Check right depending on the site url...
            else if ((Site = await siteProvider?.Get(Module.Domain, context)) != null)
            {
                // The asked url can be viewed.
                res = 1;
            }

            // Only in case where the site is accessible to the user...
            if (res == 1)
            {
                Log?.LogDebug("Site: {0}, Module: {1}.", Site.Title, Module.Name);

                // Total of regions...
                RegionCount = (Site.HasRegions == false) ? 0 : (Site?.GetRegions()?.Count ?? 0);
                // In case of an API, try to get the region from the context...
                // Because IRouter is not handling web api since the 1.0.0RTM version:
                //  * https://github.com/aspnet/Announcements/issues/193
                //  * https://github.com/aspnet/Routing/issues/321
                if ((context.Request?.Path.ToString()?.StartsWith("/api") ?? false) == true)
                {
                    _GetRegion(context, ref res);
                }
            }

            // Trace performance...
            AddPerfLog("AppContext::InitSiteAsync");
            return res;
        }

        /// <summary>
        /// Init the application context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> InitRouteAsync(RouteContext context)
        {
            bool res = true;
            int resInt = 0;

            // Checking...
            if (context == null || context.RouteData == null)
            {
                Log?.LogError("Failed to init the route: Invalid context.");
                // Trace performance...
                AddPerfLog("AppContext::InitRouteAsync::Invalid context");
                return false;
            }
            else if (Site == null)
            {
                Log?.LogError("Failed to init the route: Invalid site.");
                // Trace performance...
                AddPerfLog("AppContext::InitRouteAsync::Invalid site");
                return false;
            }
            Log?.LogInformation("Init site route...");

            // Related to region...
            RouteRegionName = (string)context.RouteData.Values[CRoute.RegionTagName];
            _GetRegion(context.HttpContext, ref resInt);
            if (resInt != 0) { res = false; }

            // Related to the current page...
            RoutePageId = (string)context.RouteData.Values[CRoute.PageIdTagName];
            if (RoutePageId != null && RoutePageId != "0")
            {
                // Load the page...
                if ((Page = await (new PageProvider(this))?.Get(RoutePageId)) == null)
                {
                    Log?.LogError("Failed to get page {0}.", RoutePageId);
                    res = false;
                }
            }

            // Related to the current post...
            if (await LoadPost((string)context.RouteData.Values[CRoute.PostIdTagName]) == false)
            {
                res = false;
            }

            // Related to the current cat...
            int routeCatId = 0;
            string routeCatIdStr = (string)context.RouteData.Values[CRoute.CatIdTagName];
            if (routeCatIdStr != null)
            {
                if (int.TryParse(routeCatIdStr, out routeCatId) == true)
                {
                    RouteCatId = routeCatId;
                    // Load the post...
                    if ((Cat = Site.GetCategory(RouteCatId)) == null)
                    {
                        Log?.LogError("Failed to get cat {0}.", RouteCatId);
                        res = false;
                    }
                }
            }

            // Save route controller and action...
            _Controller = (string)context.RouteData.Values["controller"];
            _Action = (string)context.RouteData.Values["action"];

            // Trace performance...
            AddPerfLog("AppContext::InitRouteAsync");
            return res;
        }

        /// <summary>
        /// Init the module.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="moduleController"></param>
        /// <returns></returns>
        public bool InitModule(RouteContext context, ModuleController moduleController)
        {
            // Checking...
            if (context == null)
            {
                Log?.LogError("Failed to init the app module: Invalid context.");
                return false;
            }
            else if (Site == null)
            {
                Log?.LogError("Failed to init the app module: Invalid site.");
                return false;
            }
            Log?.LogInformation("Init site module...");

            // Save module information into the context.
            ModuleController = moduleController;
            // Init the module.
            // TODO: Init the module.
            //Module.Init(context, this);

            // Trace performance...
            AddPerfLog("AppContext::InitModule");
            return true;
        }

        /// <summary>
        /// Signout the user.
        /// </summary>
        /// <param name="context"></param>
        public async Task<bool> SignOut(HttpContext context)
        {
            bool res = false;
            if (SignInManager != null)
            {
                // Sign out...
                await SignInManager.SignOutAsync();
                //await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                res = true;
            }
            UserPrincipal = null;
            _User = null;
            // context.User is not deleted after SignOutAsync, so do it manually... 
            context.User = null;
            return res;
        }

        /// <summary>
        /// Do we have a valid context.
        /// </summary>
        /// <param name="checkRegion"></param>
        /// <returns></returns>
        public bool IsValid(bool checkRegion = true)
        {
            if (AppDbContext == null)
            {
                Log?.LogError("Invalid db context.");
                return false;
            }
            else if (Site == null)
            {
                Log?.LogError("Invalid site.");
                return false;
            }
            //else if (checkRegion == true && Site.HasRegions == true && Region == null)
            //{
            //    Log?.LogWarning("Invalid region.");
            //    return false;
            //}
            return true;
        }

        /// <summary>
        /// Get current page path.
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetNavPath()
        {
            // Checking...
            if (IsValid() == false
                || Page == null)
            {
                return null;
            }
            Page page = Page;
            SiteClaim cat = Cat;
            bool addPage = false;
            PageProvider provider = new PageProvider(this);
            Dictionary<string, string> path = new Dictionary<string, string>()
            {
            };
            // Url extension...
            string urlExtension = (_Controller == "Post" && _Action == "Calendar") ? "/cldr" : null;
            // Add the current category...
            if (Post != null && cat != null)
            {
                path.Add(cat.StringValue, page.GetCategoryUrl(this, cat, urlExtension));
            }
            // Add cats...
            if (cat != null && _Controller != null && _Action != null)
            {
                // Add the current cat...
                //path.Add(cat.StringValue, page.GetCategoryUrl(this, cat, urlExtension));
                // Add the parent cats...
                while (cat?.ParentId != null
                    && (cat = Site?.GetCategory(cat.ParentId.Value)) != null)
                {
                    // Add the parent cat (do not add the page category)...
                    if (cat.Id == page.Category1 ||
                        cat.Id == page.Category2 ||
                        cat.Id == page.Category3 ||
                        cat.Id == page.Category4 ||
                        cat.Id == page.Category5 ||
                        cat.Id == page.Category6 ||
                        cat.Id == page.Category7 ||
                        cat.Id == page.Category8 ||
                        cat.Id == page.Category9 ||
                        cat.Id == page.Category10)
                    {
                        break;
                    }
                    path.Add(cat.StringValue, page.GetCategoryUrl(this, cat, urlExtension));
                }
                // Add the current page...
                addPage = true;
            }
            else if (Post != null) {
                // Add the current page...
                addPage = true;
            }
            // Add the current page...
            if (page != null && addPage == true)
            {
                path.Add(page.Title, page.GetUrl(this));
            }
            // Add the parent pages...
            while (page?.ParentId != null
                && (page = await provider?.Get(page.ParentId.Value)) != null)
            {
                // Add the parent page...
                path.Add(page.Title, page.GetUrl(this));
            }
            //if (Site.HasRegions == true)
            //{
            //    path.Add(Region?.StringValue, $"/{Region?.StringValue.ToLower()}");
            //}
            //else
            //{
            //    path.Add("Home", "/");
            //}

            // Trace performance...
            AddPerfLog("AppContext::GetNavPath");
            return path;
        }

        /// <summary>
        /// Get the user of the context.
        /// </summary>
        /// <returns></returns>
        public async Task<ApplicationUser> GetUser()
        {
            if (_InvalidUserSite == true)
            {
                return null;
            }
            else if (_User == null)
            {
                if (UserPrincipal != null
                    && SignInManager.IsSignedIn(UserPrincipal) == true)
                {
#if !DEBUG
                    //GetUserFromTheUserProvider;
                    //UserCacheForUser;
#endif
                    _User = await UserManager.GetUserAsync(UserPrincipal);
                    if (_User != null)
                    {
                        await AppDbContext.UserClaims
                            .Where(uc => uc.UserId == _User.Id)
                            .LoadAsync();
                    }
                }
                //// Check user site...
                //if (_User != null && 
                //    (Site == null || _User.SiteId != Site.Id))
                //{
                //    // Invalid user...
                //    _InvalidUserSite = true;
                //    _User = null;
                //    return null;
                //}
            }
            return _User;
        }

        /// <summary>
        /// Load the context post.
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task<bool> LoadPost(string postId)
        {
            if ((RoutePostId = postId) != null)
            {
                // Load the post...
                if ((Post = await (new PostProvider(this, null))?.Get(RoutePostId)) == null)
                {
                    Log?.LogError("Failed to get post {0}.", RoutePostId);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public long ElapsedMilliseconds()
        {
            return _PerfProvider?.ElapsedMilliseconds ?? -1;
        }

        /// <summary>
        /// Start performance log.
        /// </summary>
        /// <param name="function"></param>
        public void StartPerfLog()
        {
            _PerfProvider?.Start();
        }

        /// <summary>
        /// Add a performance log.
        /// </summary>
        /// <param name="function"></param>
        public void AddPerfLog(string function)
        {
            _PerfProvider?.Add(function);
        }

        /// <summary>
        /// Write performance logs to file.
        /// </summary>
        /// <param name="path"></param>
        public void WritePerfLog(string path)
        {
            _PerfProvider?.Write(path);
        }

        /// <summary>
        /// Flush performance log file.
        /// </summary>
        public void FlushPerfLog()
        {
            _PerfProvider?.Flush();
        }

        ///// <summary>
        ///// Initialization for unit test.
        ///// </summary>
        ///// <param name="dbctx"></param>
        //public void UnitTestInit(AppDbContext dbctx)
        //{
        //    if (dbctx != null)
        //    {
        //        AppDbContext = dbctx;
        //    }
        //}

        /// <summary>
        /// Clean the context.
        /// </summary>
        private void _Clean()
        {
            AuthzProvider = null;
            Host = new HostString(null);
            UserPrincipal = null;
            _User = null;
            _InvalidUserSite = false;
            Site = null;
            Module = null;
            ModuleController = null;
            RouteRegionName = null;
            Region = null;
            RoutePageId = null;
            Page = null;
            RoutePostId = null;
            Post = null;
        }

        /// <summary>
        /// Load the region.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        private void _GetRegion(HttpContext httpContext, ref int res)
        {
            if (httpContext != null 
                && Site != null && Site.HasRegions == true
                && Region == null)
            {
                if (string.IsNullOrEmpty(RouteRegionName) == true)
                {
                    // If not region route, try to get from the query...
                    RouteRegionName = httpContext.Request.Query["region"].ToString();
                }
                // Related to the current region...
                if (string.IsNullOrEmpty(RouteRegionName) == false)
                {
                    // Load region...
                    if ((Region = Site.GetRegion(RouteRegionName)) == null)
                    {
                        Log?.LogError("Failed to get region {0}.", RouteRegionName);
                        res = 2;
                    }
                }
                //// Get the "All region" filter from cookie...
                //StringValues allregion = context.HttpContext.Request.Cookies["allregion"];
                //PostOfAllRegions = (string.IsNullOrWhiteSpace(allregion) == true || allregion == "off") ? false : true;
            }
        }
    }
}
