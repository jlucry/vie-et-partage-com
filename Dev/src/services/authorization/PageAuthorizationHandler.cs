// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE
// !!!To increase perf, do not retrieve others table when getting a list of page!!! ///
#define PERF_ISSUE

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Authorization handler for page data.
    /// </summary>
    public class PageAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Page>
    {
        public static bool LogDisabled = false;

        /// <summary>
        /// Application context.
        /// </summary>
        private readonly WcmsAppContext _AppContext;

        /// <summary>
        /// </summary>
        private readonly PerformanceProvider _PerfProvider;

        /// <summary>
        /// Logger.
        /// </summary>
        private ILogger _Log { get; set; }

        /// <summary>
        /// Page authorization handler constructor.
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="perfProvider"></param>
        /// <param name="loggerFactory"></param>
        public PageAuthorizationHandler(WcmsAppContext appContext,
            PerformanceProvider perfProvider,
            ILoggerFactory loggerFactory)
        {
            _AppContext = appContext;
            _PerfProvider = perfProvider;
            // Trace...
            if (LogDisabled == false)
            {
                _Log = loggerFactory?.CreateLogger("Authorization"/*typeof(PageAuthorizationHandler).FullName*/);
            }
        }

        /// <summary>
        /// Handle the authorization policy for page.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <param name="page"></param>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Page page)
        {
            bool isSignedIn = (context?.User == null) ? false : (_AppContext?.SignInManager?.IsSignedIn(context.User) ?? false);
            string userName = (context?.User == null) ? null : _AppContext?.UserManager?.GetUserName(context.User);
            // Special case...
            if (requirement?.Name == null)
            {
                _Log?.LogCritical("Access denied to page \"{0}\": Null requirement.", page?.Title);
                context.Fail();
            }
            else if (requirement.Name != AuthorizationRequirement.Read
                && requirement.Name != AuthorizationRequirement.Add
                && requirement.Name != AuthorizationRequirement.Update)
            {
                _Log?.LogWarning("{0} access denied to page \"{1}\": Invalid requirement.", requirement.Name, page?.Title);
                context.Fail();
            }
            else if (page == null)
            {
                _Log?.LogInformation("{0} access granted to null page.", requirement.Name);
                context.Succeed(requirement);
            }
            else if (_AppContext == null)
            {
                _Log?.LogCritical("{0} access denied to page \"{1}\": Invalid application context.", requirement.Name, page?.Title);
                context.Fail();
            }
            else if (_AppContext.SignInManager == null)
            {
                _Log?.LogCritical("{0} access denied to page \"{1}\": Invalid SignIn manager.", requirement.Name, page?.Title);
                context.Fail();
            }
            else if (context.User == null)
            {
                _Log?.LogCritical("{0} access denied to page \"{1}\": Invalid User data.", requirement.Name, page?.Title);
                context.Fail();
            }
            // Check inputs...
            else if (page.RequestSite == null)
            {
                _Log?.LogCritical("{0} access denied to page \"{1}\": Invalid page.", requirement.Name, page?.Title);
                context.Fail();
            }
            // Check inconsistency...
            else if (page.SiteId != page.RequestSite.Id)
            {
                _Log?.LogCritical("{0} access denied to page \"{1}\": Site inconsistency.", requirement.Name, page?.Title);
                context.Fail();
            }
            else if (page.RequestSite.Private == true
                && page.Private == false)
            {
                _Log?.LogCritical("{0} access denied to page \"{1}\": Privacy inconsistency.", requirement.Name, page?.Title);
                context.Fail();
            }
            // Check user inconsistency...
            else if (isSignedIn == true && _AppContext.User == null)
            {
                // Signed user in the http context, but no user in the application context.
                _Log?.LogCritical("{0} access denied to page \"{1}\": User inconsistency (http but not in app).", requirement.Name, page?.Title);
                context.Fail();
            }
            else if (isSignedIn == false && _AppContext.User != null)
            {
                // No Signed user in the http context, but user in the application context.
                _Log?.LogCritical("{0} access denied to page \"{1}\": User inconsistency (not in http but in app).", requirement.Name, page?.Title);
                context.Fail();
            }
            else if ((isSignedIn == true && _AppContext.User != null)
                && (userName == null || userName != _AppContext.User.UserName))
            {
                // Different user signed in http context and in application context.
                _Log?.LogCritical($"{requirement.Name} access denied to page \"{page.Title}\": \"{userName}\" not matching to context user \"{_AppContext.User.UserName}\".");
                context.Fail();
            }
            else if (_AppContext.User != null && _AppContext.User.SiteId != page.SiteId)
            {
                // Invalid user site...
                _Log?.LogCritical($"{requirement.Name} access denied to page \"{page.Title}\": \"{context?.User?.Identity?.Name}\" not allowed on this site.");
                context.Fail();
            }
            // Check access right...
            else if (_AppContext.User == null)
            {
                // Read published and public page is granted to anonymous...
                if (requirement.Name == AuthorizationRequirement.Read
                    && page.Private == false
                    && page.State == State.Valided)
                {
                    _Log?.LogInformation("{0} access granted to page \"{1}\": Published public page allowed to anonymous.", requirement.Name, page?.Title);
                    context.Succeed(requirement);
                }
                else
                {
                    _Log?.LogWarning("{0} access denied to page \"{page.Title}\": Anonymous user.", requirement.Name, page?.Title);
                    context.Fail();
                }
            }
            // If the page is private, check user groups:
            //  For read: user need to have at least one group of the page.
            //  For other requirement: user need to have all groups of the page.
            else if (page.Private == true
                && requirement.Name == AuthorizationRequirement.Read
                && _AppContext.User.MemberOf(page) == false)
            {
                _Log?.LogWarning($"{requirement.Name} access denied to page \"{page.Title}\": \"{context?.User?.Identity?.Name}\" not in allowed groups.");
                context.Fail();
            }
            else if (page.Private == true
                && requirement.Name != AuthorizationRequirement.Read
                && _AppContext.User.MemberOfAll(page) == false)
            {
                _Log?.LogWarning($"{requirement.Name} access denied to page \"{page.Title}\": \"{context?.User?.Identity?.Name}\" not in all allowed groups.");
                context.Fail();
            }
            // Administrator and publicator have all rights...
            else if (_AppContext.User.HasRole(ClaimValueRole.Administrator) == true
                    || _AppContext.User.HasRole(ClaimValueRole.Publicator) == true)
            {
                _Log?.LogInformation($"{requirement.Name} access granted to page \"{page.Title}\": \"{context?.User?.Identity?.Name}\" as {ClaimValueRole.Administrator}\\{ClaimValueRole.Publicator}.");
                context.Succeed(requirement);
            }
            // Contributeur and reader can only read published page...
            else if (_AppContext.User.HasRole(ClaimValueRole.Contributor) == true
                    || _AppContext.User.HasRole(ClaimValueRole.Reader) == true)
            {
                if (requirement.Name == AuthorizationRequirement.Read
                    && page.State == State.Valided)
                {
                    _Log?.LogInformation($"{requirement.Name} access granted to page \"{page.Title}\": \"{context?.User?.Identity?.Name}\" can read published page as {ClaimValueRole.Contributor}\\{ClaimValueRole.Reader}.");
                    context.Succeed(requirement);
                }
                else
                {
                    _Log?.LogWarning($"{requirement.Name} access denied to page \"{page.Title}\": \"{context?.User?.Identity?.Name}\" as {ClaimValueRole.Contributor} or {ClaimValueRole.Reader} not allowed.");
                    context.Fail();
                }
            }
            // User without roles...
            else if (requirement.Name == AuthorizationRequirement.Read
                    && page.State == State.Valided)
            {
                _Log?.LogInformation($"{requirement.Name} access granted to page \"{page.Title}\": \"{context?.User?.Identity?.Name}\" can read published page without role.");
                context.Succeed(requirement);
            }
            else
            {
                _Log?.LogWarning($"{requirement.Name} access denied to page \"{page.Title}\": \"{context?.User?.Identity?.Name}\" not allowed.");
                context.Fail();
            }
            // Trace performance...
            _PerfProvider?.Add("PageAuthorizationHandler::Handle");
            // Return...
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get pages.
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="onlyInMenu"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Page>> Get(Services.WcmsAppContext appContext, bool onlyInMenu, int? parent = null)
        {
            // Checking...
            if ((appContext?.IsValid() ?? false) == false)
            {
                // Trace performance...
                appContext?.AddPerfLog("PageAuthorizationHandler::Get::Invalid context");
                return null;
            }

            // User role and groups...
            string userRole = appContext.User?.HigherRole() ?? null;
            List<int> userGroupIds = appContext.User?.GroupsId();
            bool haveGroup = (userGroupIds != null && userGroupIds.Count() != 0);

            // Base query...
            var query = appContext.AppDbContext.Pages?.Where(p => p.SiteId == appContext.Site.Id);
            // Menu filter...
            if (onlyInMenu == true)
            {
                query = query?.Where(p => p.PositionInNavigation != 0);
            }
            // Parent filter...
            if (parent == null)
            {
                query = query?.Where(p => p.ParentId == null);
            }
            else if (parent != -1)
            {
                query = query?.Where(p => p.ParentId == parent.Value);
            }

            // Filtering based on the region...
            if (appContext.Site.HasRegions == true && (appContext.Region?.Id ?? -1) != -1)
            {
                query = query?
                    .Where(p => p.Region1 == 0 || p.Region1 == appContext.Region.Id
                        || p.Region2 == 0 || p.Region2 == appContext.Region.Id
                        || p.Region3 == 0 || p.Region3 == appContext.Region.Id
                        || p.Region4 == 0 || p.Region4 == appContext.Region.Id
                        || p.Region5 == 0 || p.Region5 == appContext.Region.Id
                        || p.Region6 == 0 || p.Region6 == appContext.Region.Id
                        || p.Region7 == 0 || p.Region7 == appContext.Region.Id
                        || p.Region8 == 0 || p.Region8 == appContext.Region.Id
                        || p.Region9 == 0 || p.Region9 == appContext.Region.Id
                        || p.Region10 == 0 || p.Region10 == appContext.Region.Id);
            }

            // Authorization filtering...
            // Administrator and publicator have read rights on all pages of the same groups...
            if ((userRole == ClaimValueRole.Administrator || userRole == ClaimValueRole.Publicator)
                && haveGroup == true)
            {
                query = query?
                    .Where(p => (p.Private == false
                        || (p.Group1 != -1 && (appContext.User.Group1 == p.Group1 || appContext.User.Group2 == p.Group1 || appContext.User.Group3 == p.Group1 || appContext.User.Group4 == p.Group1 || appContext.User.Group5 == p.Group1 || appContext.User.Group6 == p.Group1 || appContext.User.Group7 == p.Group1 || appContext.User.Group8 == p.Group1 || appContext.User.Group9 == p.Group1 || appContext.User.Group10 == p.Group1))
                        || (p.Group2 != -1 && (appContext.User.Group1 == p.Group2 || appContext.User.Group2 == p.Group2 || appContext.User.Group3 == p.Group2 || appContext.User.Group4 == p.Group2 || appContext.User.Group5 == p.Group2 || appContext.User.Group6 == p.Group2 || appContext.User.Group7 == p.Group2 || appContext.User.Group8 == p.Group2 || appContext.User.Group9 == p.Group2 || appContext.User.Group10 == p.Group2))
                        || (p.Group3 != -1 && (appContext.User.Group1 == p.Group3 || appContext.User.Group2 == p.Group3 || appContext.User.Group3 == p.Group3 || appContext.User.Group4 == p.Group3 || appContext.User.Group5 == p.Group3 || appContext.User.Group6 == p.Group3 || appContext.User.Group7 == p.Group3 || appContext.User.Group8 == p.Group3 || appContext.User.Group9 == p.Group3 || appContext.User.Group10 == p.Group3))
                        || (p.Group4 != -1 && (appContext.User.Group1 == p.Group4 || appContext.User.Group2 == p.Group4 || appContext.User.Group3 == p.Group4 || appContext.User.Group4 == p.Group4 || appContext.User.Group5 == p.Group4 || appContext.User.Group6 == p.Group4 || appContext.User.Group7 == p.Group4 || appContext.User.Group8 == p.Group4 || appContext.User.Group9 == p.Group4 || appContext.User.Group10 == p.Group4))
                        || (p.Group5 != -1 && (appContext.User.Group1 == p.Group5 || appContext.User.Group2 == p.Group5 || appContext.User.Group3 == p.Group5 || appContext.User.Group4 == p.Group5 || appContext.User.Group5 == p.Group5 || appContext.User.Group6 == p.Group5 || appContext.User.Group7 == p.Group5 || appContext.User.Group8 == p.Group5 || appContext.User.Group9 == p.Group5 || appContext.User.Group10 == p.Group5))
                        || (p.Group6 != -1 && (appContext.User.Group1 == p.Group6 || appContext.User.Group2 == p.Group6 || appContext.User.Group3 == p.Group6 || appContext.User.Group4 == p.Group6 || appContext.User.Group5 == p.Group6 || appContext.User.Group6 == p.Group6 || appContext.User.Group7 == p.Group6 || appContext.User.Group8 == p.Group6 || appContext.User.Group9 == p.Group6 || appContext.User.Group10 == p.Group6))
                        || (p.Group7 != -1 && (appContext.User.Group1 == p.Group7 || appContext.User.Group2 == p.Group7 || appContext.User.Group3 == p.Group7 || appContext.User.Group4 == p.Group7 || appContext.User.Group5 == p.Group7 || appContext.User.Group6 == p.Group7 || appContext.User.Group7 == p.Group7 || appContext.User.Group8 == p.Group7 || appContext.User.Group9 == p.Group7 || appContext.User.Group10 == p.Group7))
                        || (p.Group8 != -1 && (appContext.User.Group1 == p.Group8 || appContext.User.Group2 == p.Group8 || appContext.User.Group3 == p.Group8 || appContext.User.Group4 == p.Group8 || appContext.User.Group5 == p.Group8 || appContext.User.Group6 == p.Group8 || appContext.User.Group7 == p.Group8 || appContext.User.Group8 == p.Group8 || appContext.User.Group9 == p.Group8 || appContext.User.Group10 == p.Group8))
                        || (p.Group9 != -1 && (appContext.User.Group1 == p.Group9 || appContext.User.Group2 == p.Group9 || appContext.User.Group3 == p.Group9 || appContext.User.Group4 == p.Group9 || appContext.User.Group5 == p.Group9 || appContext.User.Group6 == p.Group9 || appContext.User.Group7 == p.Group9 || appContext.User.Group8 == p.Group9 || appContext.User.Group9 == p.Group9 || appContext.User.Group10 == p.Group9))
                        || (p.Group10!= -1 && (appContext.User.Group1 == p.Group10|| appContext.User.Group2 == p.Group10|| appContext.User.Group3 == p.Group10|| appContext.User.Group4 == p.Group10|| appContext.User.Group5 == p.Group10|| appContext.User.Group6 == p.Group10|| appContext.User.Group7 == p.Group10|| appContext.User.Group8 == p.Group10|| appContext.User.Group9 == p.Group10|| appContext.User.Group10 == p.Group10))));
            }
            // Contributeur and reader can only read published page of the same groups...
            else if ((userRole == ClaimValueRole.Contributor || userRole == ClaimValueRole.Reader)
                && haveGroup == true)
            {
                query = query?
                    .Where(p => p.State == State.Valided
                        && (p.Private == false
                        || (p.Group1 != -1 && (appContext.User.Group1 == p.Group1 || appContext.User.Group2 == p.Group1 || appContext.User.Group3 == p.Group1 || appContext.User.Group4 == p.Group1 || appContext.User.Group5 == p.Group1 || appContext.User.Group6 == p.Group1 || appContext.User.Group7 == p.Group1 || appContext.User.Group8 == p.Group1 || appContext.User.Group9 == p.Group1 || appContext.User.Group10 == p.Group1))
                        || (p.Group2 != -1 && (appContext.User.Group1 == p.Group2 || appContext.User.Group2 == p.Group2 || appContext.User.Group3 == p.Group2 || appContext.User.Group4 == p.Group2 || appContext.User.Group5 == p.Group2 || appContext.User.Group6 == p.Group2 || appContext.User.Group7 == p.Group2 || appContext.User.Group8 == p.Group2 || appContext.User.Group9 == p.Group2 || appContext.User.Group10 == p.Group2))
                        || (p.Group3 != -1 && (appContext.User.Group1 == p.Group3 || appContext.User.Group2 == p.Group3 || appContext.User.Group3 == p.Group3 || appContext.User.Group4 == p.Group3 || appContext.User.Group5 == p.Group3 || appContext.User.Group6 == p.Group3 || appContext.User.Group7 == p.Group3 || appContext.User.Group8 == p.Group3 || appContext.User.Group9 == p.Group3 || appContext.User.Group10 == p.Group3))
                        || (p.Group4 != -1 && (appContext.User.Group1 == p.Group4 || appContext.User.Group2 == p.Group4 || appContext.User.Group3 == p.Group4 || appContext.User.Group4 == p.Group4 || appContext.User.Group5 == p.Group4 || appContext.User.Group6 == p.Group4 || appContext.User.Group7 == p.Group4 || appContext.User.Group8 == p.Group4 || appContext.User.Group9 == p.Group4 || appContext.User.Group10 == p.Group4))
                        || (p.Group5 != -1 && (appContext.User.Group1 == p.Group5 || appContext.User.Group2 == p.Group5 || appContext.User.Group3 == p.Group5 || appContext.User.Group4 == p.Group5 || appContext.User.Group5 == p.Group5 || appContext.User.Group6 == p.Group5 || appContext.User.Group7 == p.Group5 || appContext.User.Group8 == p.Group5 || appContext.User.Group9 == p.Group5 || appContext.User.Group10 == p.Group5))
                        || (p.Group6 != -1 && (appContext.User.Group1 == p.Group6 || appContext.User.Group2 == p.Group6 || appContext.User.Group3 == p.Group6 || appContext.User.Group4 == p.Group6 || appContext.User.Group5 == p.Group6 || appContext.User.Group6 == p.Group6 || appContext.User.Group7 == p.Group6 || appContext.User.Group8 == p.Group6 || appContext.User.Group9 == p.Group6 || appContext.User.Group10 == p.Group6))
                        || (p.Group7 != -1 && (appContext.User.Group1 == p.Group7 || appContext.User.Group2 == p.Group7 || appContext.User.Group3 == p.Group7 || appContext.User.Group4 == p.Group7 || appContext.User.Group5 == p.Group7 || appContext.User.Group6 == p.Group7 || appContext.User.Group7 == p.Group7 || appContext.User.Group8 == p.Group7 || appContext.User.Group9 == p.Group7 || appContext.User.Group10 == p.Group7))
                        || (p.Group8 != -1 && (appContext.User.Group1 == p.Group8 || appContext.User.Group2 == p.Group8 || appContext.User.Group3 == p.Group8 || appContext.User.Group4 == p.Group8 || appContext.User.Group5 == p.Group8 || appContext.User.Group6 == p.Group8 || appContext.User.Group7 == p.Group8 || appContext.User.Group8 == p.Group8 || appContext.User.Group9 == p.Group8 || appContext.User.Group10 == p.Group8))
                        || (p.Group9 != -1 && (appContext.User.Group1 == p.Group9 || appContext.User.Group2 == p.Group9 || appContext.User.Group3 == p.Group9 || appContext.User.Group4 == p.Group9 || appContext.User.Group5 == p.Group9 || appContext.User.Group6 == p.Group9 || appContext.User.Group7 == p.Group9 || appContext.User.Group8 == p.Group9 || appContext.User.Group9 == p.Group9 || appContext.User.Group10 == p.Group9))
                        || (p.Group10!= -1 && (appContext.User.Group1 == p.Group10|| appContext.User.Group2 == p.Group10|| appContext.User.Group3 == p.Group10|| appContext.User.Group4 == p.Group10|| appContext.User.Group5 == p.Group10|| appContext.User.Group6 == p.Group10|| appContext.User.Group7 == p.Group10|| appContext.User.Group8 == p.Group10|| appContext.User.Group9 == p.Group10|| appContext.User.Group10 == p.Group10))));
            }
            // Have role but no group...
            else if (userRole == ClaimValueRole.Administrator || userRole == ClaimValueRole.Publicator)
            {
                query = query?.Where(p => p.Private == false);
            }
            else if (userRole == ClaimValueRole.Contributor || userRole == ClaimValueRole.Reader)
            {
                query = query?.Where(p => p.State == State.Valided && p.Private == false);
            }
            // No role and no group...
            else
            {
                query = query?.Where(p => p.State == State.Valided && p.Private == false);
            }

            if (query == null)
            {
                // Trace performance...
                appContext?.AddPerfLog("PageAuthorizationHandler::Get::Query build failed");
                return null;
            }
            // Trace performance...
            appContext?.AddPerfLog("PageAuthorizationHandler::Get::Query built");
            List<Page> pages = await query
                .OrderBy(p => p.PositionInNavigation)
#if !PERF_ISSUE
                //TODO: Add a way to customize fields we want to retrieve with the request...
#if !DENORMALIZE
                .Include(p => p.PageGroups)
                .Include(p => p.PageRegions)
                .Include(p => p.PageCategorys)
                .Include(p => p.PageTags)
#endif
                .Include(p => p.PageClaims)
#endif
                .AsNoTracking()
                .ToListAsync();
            // Trace performance...
            appContext?.AddPerfLog("PageAuthorizationHandler::Get::Query executed");
            return pages;
        }
    }
}