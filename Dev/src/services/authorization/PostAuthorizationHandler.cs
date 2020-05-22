// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE
// !!!To increase perf, do not retrieve others table when getting a list of post!!! ///
#define PERF_ISSUE

#define TEST_INCLUDE
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Authorization handler for post data.
    /// </summary>
    public class PostAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Post>
    {
        private const string allstate = "allstate";
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
        /// Post authorization handler constructor.
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="perfProvider"></param>
        /// <param name="loggerFactory"></param>
        public PostAuthorizationHandler(WcmsAppContext appContext,
            PerformanceProvider perfProvider,
            ILoggerFactory loggerFactory)
        {
            _AppContext = appContext;
            _PerfProvider = perfProvider;
            // Trace...
            if (LogDisabled == false)
            {
                _Log = loggerFactory?.CreateLogger("Authorization"/*typeof(PostAuthorizationHandler).FullName*/);
            }
        }

        /// <summary>
        /// Handle the authorization policy for post.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <param name="post"></param>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Post post)
        {
            bool isSignedIn = (context?.User == null) ? false : (_AppContext?.SignInManager?.IsSignedIn(context.User) ?? false);
            string userName = (context?.User == null) ? null : _AppContext?.UserManager?.GetUserName(context.User);
            // Special case...
            if (requirement?.Name == null)
            {
                _Log?.LogCritical($"Access denied to post \"{post?.Title}\": Null requirement.");
                context.Fail();
            }
            else if (requirement.Name != AuthorizationRequirement.Read
                && requirement.Name != AuthorizationRequirement.Add
                && requirement.Name != AuthorizationRequirement.Update
                && requirement.Name != AuthorizationRequirement.Publish)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to post \"{post?.Title}\": Invalid requirement.");
                context.Fail();
            }
            else if (post == null)
            {
                _Log?.LogInformation($"{requirement.Name} access granted to null post.");
                context.Succeed(requirement);
            }
            else if (_AppContext == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to post \"{post.Title}\": Invalid application context.");
                context.Fail();
            }
            else if (_AppContext.SignInManager == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to post \"{post.Title}\": Invalid SignIn manager.");
                context.Fail();
            }
            else if (_AppContext.UserManager == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to post \"{post.Title}\": Invalid User manager.");
                context.Fail();
            }
            else if (context.User == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to post \"{post.Title}\": Invalid User data.");
                context.Fail();
            }
            // Check inputs...
            else if (post.RequestSite == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to post \"{post.Title}\": Invalid post.");
                context.Fail();
            }
            // Check inconsistency...
            else if (post.SiteId != post.RequestSite.Id)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to post \"{post.Title}\": Site inconsistency.");
                context.Fail();
            }
            else if (post.RequestSite.Private == true
                && post.Private == false)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to post \"{post.Title}\": Privacy inconsistency.");
                context.Fail();
            }
            // Check user inconsistency...
            else if (isSignedIn == true && _AppContext.User == null)
            {
                // Signed user in the http context, but no user in the application context.
                _Log?.LogCritical($"{requirement.Name} access denied to post \"{post.Title}\": User inconsistency (http but not in app).");
                context.Fail();
            }
            else if (isSignedIn == false && _AppContext.User != null)
            {
                // No Signed user in the http context, but user in the application context.
                _Log?.LogCritical($"{requirement.Name} access denied to post \"{post.Title}\": User inconsistency (not in http but in app).");
                context.Fail();
            }
            else if ((isSignedIn == true && _AppContext.User != null)
                && (userName == null || userName != _AppContext.User.UserName))
            {
                // Different user signed in http context and in application context.
                _Log?.LogCritical($"{requirement.Name} access denied to post \"{post.Title}\": \"{userName}\" not matching to context user \"{_AppContext.User.UserName}\".");
                context.Fail();
            }
            else if (_AppContext.User != null && _AppContext.User.SiteId != post.SiteId)
            {
                // Invalid user site...
                _Log?.LogWarning($"{requirement.Name} access denied to post \"{post.Title}\": \"{context?.User?.Identity?.Name}\" not allowed on this site.");
                context.Fail();
            }
            // Check access right...
            else if (_AppContext.User == null)
            {
                // Read published and public post is granted to anonymous...
                if (requirement.Name == AuthorizationRequirement.Read
                    && post.Private == false
                    && post.State == State.Valided)
                {
                    _Log?.LogInformation($"{requirement.Name} access granted to post \"{post.Title}\": Published public post allowed to anonymous.");
                    context.Succeed(requirement);
                }
                else
                {
                    _Log?.LogWarning($"{requirement.Name} access denied to post \"{post.Title}\": Anonymous user.");
                    context.Fail();
                }
            }
            // Read published and public post is granted to all...
            else if (requirement.Name == AuthorizationRequirement.Read
                && post.Private == false
                && post.State == State.Valided)
            {
                _Log?.LogInformation($"{requirement.Name} access granted to post \"{post.Title}\": Published public post allowed to all.");
                context.Succeed(requirement);
            }
            // If the post is private, check user groups:
            //  For read: user need to have at least one group of the page.
            //  For other requirement: user need to have all groups of the page.
            else if (post.Private == true
                && requirement.Name == AuthorizationRequirement.Read
                && _AppContext.User.MemberOf(post) == false)
            {
                _Log?.LogWarning($"{requirement.Name} access denied to post \"{post.Title}\": \"{context?.User?.Identity?.Name}\" not in allowed groups.");
                context.Fail();
            }
            else if (post.Private == true
                && requirement.Name != AuthorizationRequirement.Read
                && _AppContext.User.MemberOfAll(post) == false)
            {
                _Log?.LogWarning($"{requirement.Name} access denied to post \"{post.Title}\": \"{context?.User?.Identity?.Name}\" not in all allowed groups.");
                context.Fail();
            }
            // Administrator and publicator have all rights...
            else if (_AppContext.User.HasRole(ClaimValueRole.Administrator) == true
                    || _AppContext.User.HasRole(ClaimValueRole.Publicator) == true)
            {
                _Log?.LogInformation($"{requirement.Name} access granted to post \"{post.Title}\": \"{context?.User?.Identity?.Name}\" as {ClaimValueRole.Administrator}\\{ClaimValueRole.Publicator}.");
                context.Succeed(requirement);
            }
            // Contributeur case...
            else if (_AppContext.User.HasRole(ClaimValueRole.Contributor) == true)
            {
                // Contributeur can read their post and published posts...
                if (requirement.Name == AuthorizationRequirement.Read)
                {
                    if (post.State == State.Valided
                        || post.CreatorId == _AppContext.User.Id)
                    {
                        _Log?.LogInformation($"{requirement.Name} access granted to post \"{post.Title}\": \"{context?.User?.Identity?.Name}\" as {ClaimValueRole.Contributor} can read their post\\published post.");
                        context.Succeed(requirement);
                    }
                    else
                    {
                        _Log?.LogWarning($"{requirement.Name} access denied to post \"{post.Title}\": \"{context?.User?.Identity?.Name}\" as {ClaimValueRole.Contributor} cannot read post.");
                        context.Fail();
                    }
                }
                // Contributeur can add posts...
                else if (requirement.Name == AuthorizationRequirement.Add)
                {
                    _Log?.LogInformation($"{requirement.Name} access granted to post \"{post.Title}\": \"{context?.User?.Identity?.Name}\" as {ClaimValueRole.Contributor} can add post.");
                    context.Succeed(requirement);
                }
                // Contributeur can update their posts...
                else if (requirement.Name == AuthorizationRequirement.Update
                    && post.CreatorId == _AppContext.User.Id)
                {
                    _Log?.LogInformation($"{requirement.Name} access granted to post \"{post.Title}\": \"{context?.User?.Identity?.Name}\" as {ClaimValueRole.Contributor} can update their post.");
                    context.Succeed(requirement);
                }
                else
                {
                    _Log?.LogWarning($"{requirement.Name} access denied to post \"{post.Title}\": \"{context?.User?.Identity?.Name}\" as {ClaimValueRole.Contributor} is not allowed.");
                    context.Fail();
                }
            }
            // Reader can only read published post...
            else if (_AppContext.User.HasRole(ClaimValueRole.Reader) == true)
            {
                if (requirement.Name == AuthorizationRequirement.Read
                    && post.State == State.Valided)
                {
                    _Log?.LogInformation($"{requirement.Name} access granted to post \"{post.Title}\": \"{context?.User?.Identity?.Name}\" as {ClaimValueRole.Reader} can read published post.");
                    context.Succeed(requirement);
                }
                else
                {
                    _Log?.LogWarning($"{requirement.Name} access denied to post \"{post.Title}\": \"{context?.User?.Identity?.Name}\" as {ClaimValueRole.Reader} is not allowed.");
                    context.Fail();
                }
            }
            // User without roles...
            else if (requirement.Name == AuthorizationRequirement.Read
                    && post.State == State.Valided)
            {
                _Log?.LogInformation($"{requirement.Name} access granted to page \"{post.Title}\": \"{context?.User?.Identity?.Name}\" can read published page without role.");
                context.Succeed(requirement);
            }
            else
            {
                _Log?.LogWarning($"{requirement.Name} access denied to post \"{post.Title}\": \"{context?.User?.Identity?.Name}\" not allowed.");
                context.Fail();
            }
            // Trace performance...
            _PerfProvider?.Add("PostAuthorizationHandler::Handle");
            // Return...
            return Task.CompletedTask;
        }

        /// <summary>
        /// Count posts.
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static async Task<int> Count(Services.WcmsAppContext appContext, Dictionary<string, object> filters)
        {
            DataSorting orderBy = DataSorting.Validation;
            // Build the query...
            var query = _GetQuery(appContext, filters, ref orderBy);
            if (query == null)
            {
                return 0;
            }
            else
            {
                // Trace performance...
                appContext?.AddPerfLog("PostAuthorizationHandler::Count::Query built");
                // Execute the query...
                int count = await query.CountAsync();
                // Trace performance...
                appContext?.AddPerfLog("PostAuthorizationHandler::Count::Query executed");
                // Exit...
                return count;
            }
        }

        /// <summary>
        /// Get posts.
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="filters"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="allFields"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Post>> Get(Services.WcmsAppContext appContext, Dictionary<string, object> filters, int skip, int take, bool allFields)
        {
            DataSorting orderBy = DataSorting.Validation;
            // Checking...
            if (skip < 0)
            {
                skip = 0;
            }
            else if (take <= 0)
            {
                take = 20;
            }
            else if (take > 200)
            {
                take = 200;
            }
            // Build the query...
            var query = _GetQuery(appContext, filters, ref orderBy);
            if (query == null)
            {
                return null;
            }
            // Sorting...
            if (orderBy == DataSorting.CreationDate)
            {
                query = query.OrderByDescending(p => p.CreationDate);
            }
            else if (orderBy == DataSorting.ModifiedDate)
            {
                query = query.OrderByDescending(p => p.ModifiedDate);
            }
            else if (orderBy == DataSorting.Validation)
            {
                query = query.OrderByDescending(p => p.ValidationDate);
            }
            else if (orderBy == DataSorting.StartDate)
            {
                query = query.OrderBy(p => p.StartDate);
            }
            else if (orderBy == DataSorting.StartDateDesc)
            {
                query = query.OrderByDescending(p => p.StartDate);
            }
            // Fields included...
            if (allFields == true)
            {
#               if !PERF_ISSUE
                query = query
#                   if !DENORMALIZE
                    .Include(p => p.PostGroups)
                    .Include(p => p.PostRegions)
                    .Include(p => p.PostCategorys)
                    .Include(p => p.PostTags)
#                   endif
                    .Include(p => p.PostTexts)
                    .Include(p => p.PostFiles)
                    .Include(p => p.PostClaims);
                    //.Include(p => p.Creator); //TODO: Only need for Unit test.
#                   endif
            }
            // Trace performance...
            appContext?.AddPerfLog("PostAuthorizationHandler::Get::Query built");
            // Execute the query...
            IEnumerable<Post> items = await query
                    .Skip(skip * take).Take(take)
                    .AsNoTracking()
                    .ToListAsync();
            // Trace performance...
            appContext?.AddPerfLog("PostAuthorizationHandler::Get::Query executed");
            // Exit...
            return items;
        }

        /// <summary>
        /// Convert filter got from a request.
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ConvertFilter(Services.WcmsAppContext appContext, Dictionary<string, string> filters)
        {
            Dictionary<string, object> fltrs = new Dictionary<string, object>();
            if (filters != null)
            {
                int topCatId = 0;
                bool showChildsCategoriesPosts = false;
                foreach (KeyValuePair<string, string> filter in filters)
                {
                    if (filter.Key != null && filter.Value != null)
                    {
                        if (filter.Key == QueryFilter.Highlight)
                        {
                            fltrs.Add(QueryFilter.Highlight, (filter.Value == "true") ? true : false);
                        }
                        else if (filter.Key == QueryFilter.State)
                        {
                            int state = 0;
                            if (int.TryParse(filter.Value, out state) == true)
                            {
                                if (state != -1)
                                {
                                    fltrs.Add(QueryFilter.State, (State)state);
                                }
                                else
                                {
                                    fltrs.Add(allstate, true);
                                }
                            }
                        }
                        else if (filter.Key == QueryFilter.Title)
                        {
                            fltrs.Add(QueryFilter.Title, WebUtility.HtmlEncode(filter.Value));
                        }
                        else if (filter.Key == QueryFilter.Mine)
                        {
                            if (filter.Value != null && filter.Value == "true")
                            {
                                fltrs.Add(QueryFilter.Mine, true);
                            }
                        }
                        else if (filter.Key == QueryFilter.MineToo)
                        {
                            if (filter.Value != null && filter.Value == "true")
                            {
                                fltrs.Add(QueryFilter.MineToo, true);
                            }
                        }
                        else if (filter.Key.StartsWith(QueryFilter.Categorie) == true)
                        {
                            int catId = 0;
                            if (int.TryParse(filter.Value, out catId) == true)
                            {
                                fltrs.Add(QueryFilter.CategorieSingle, catId);
                            }
                        }
                        else if (filter.Key.StartsWith(QueryFilter.Tag) == true)
                        {
                            int tagId = 0;
                            if (int.TryParse(filter.Value, out tagId) == true)
                            {
                                fltrs.Add(QueryFilter.TagSingle, tagId);
                            }
                        }
                        else if (filter.Key.StartsWith(QueryFilter.Group) == true)
                        {
                            int groupId = 0;
                            if (int.TryParse(filter.Value, out groupId) == true)
                            {
                                fltrs.Add(QueryFilter.Group, groupId);
                            }
                        }
                        else if (filter.Key.StartsWith(QueryFilter.StartDate) == true)
                        {
                            DateTime startDate;
                            if (DateTime.TryParse(filter.Value, out startDate) == true)
                            {
                                fltrs.Add(QueryFilter.StartDate, startDate);
                            }
                        }
                        else if (filter.Key.StartsWith(QueryFilter.EndDate) == true)
                        {
                            DateTime endDate;
                            if (DateTime.TryParse(filter.Value, out endDate) == true)
                            {
                                fltrs.Add(QueryFilter.EndDate, endDate);
                            }
                        }
                        else if (filter.Key == QueryFilter.TopCategorie)
                        {
                            int.TryParse(filter.Value, out topCatId);
                        }
                        else if (filter.Key == QueryFilter.ShowChildsCategoriesPosts)
                        {
                            if (filter.Value != null && filter.Value == "true")
                            {
                                showChildsCategoriesPosts = true;
                            }
                        }
                        else if (filter.Key == QueryFilter.ShowEventPostsOnly)
                        {
                            if (filter.Value != null && filter.Value == "true")
                            {
                                fltrs.Add(QueryFilter.ShowEventPostsOnly, true);
                            }
                        }
                        else if (filter.Key == QueryFilter.EndDate)
                        {
                            fltrs.Add(QueryFilter.EndDate, DateTime.Now);
                        }
                    }
                }
                // Manage top category...
                if (topCatId != 0 && fltrs.ContainsKey(QueryFilter.CategorieSingle) == false)
                {
                    fltrs.Add(QueryFilter.CategorieSingle, topCatId);
                }
                // Manage childs category post...
                if (showChildsCategoriesPosts == true && fltrs.ContainsKey(QueryFilter.CategorieSingle) == true)
                {
                    int catId = (int)fltrs[QueryFilter.CategorieSingle];
                    // Get childs categories...
                    List<int> idList = appContext?.Site?.GetCategoriesAsIdList(catId, true);
                    if ((idList?.Count ?? 0) > 0)
                    {
                        idList.Add(catId);
                        fltrs.Remove(QueryFilter.CategorieSingle);
                        fltrs.Add(QueryFilter.Categorie, idList);
                    }
                }
            }
            return fltrs;
        }

        /// <summary>
        /// Get post query.
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="filters"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        private static IQueryable<Post> _GetQuery(Services.WcmsAppContext appContext, Dictionary<string, object> filters, ref DataSorting orderBy)
        {
            ApplicationUser userGroups = null;
            // Checking...
            if ((appContext?.IsValid() ?? false) == false)
            {
                return null;
            }
            // Init...
            orderBy = DataSorting.Validation;

            // User role and groups...
            string userRole = appContext.User?.HigherRole() ?? null;
            List<int> userGroupIds = appContext.User?.GroupsId();
            if (userGroupIds != null && userGroupIds.Count > 0)
            {
                userGroups = new ApplicationUser();
                userGroups.Group1 = appContext.User.Group1;
                userGroups.Group2 = appContext.User.Group2;
                userGroups.Group3 = appContext.User.Group3;
                userGroups.Group4 = appContext.User.Group4;
                userGroups.Group5 = appContext.User.Group5;
                userGroups.Group6 = appContext.User.Group6;
                userGroups.Group7 = appContext.User.Group7;
                userGroups.Group8 = appContext.User.Group8;
                userGroups.Group9 = appContext.User.Group9;
                userGroups.Group10= appContext.User.Group10;
            }

            // Get filter to apply on the post table...
            bool mine = false;
            bool mineToo = false;
            bool? highlight = null;
            State? state = null;
            string title = null;
            DateTime? endDate = null;
            bool filterOngroup = false;
            if (filters != null)
            {
                foreach (KeyValuePair<string, object> filter in filters)
                {
                    if (filter.Key != null && filter.Value != null)
                    {
                        if (filter.Key == QueryFilter.Highlight
                            && filter.Value != null)
                        {
                            highlight = (bool)filter.Value;
                        }
                        else if (filter.Key == QueryFilter.State
                            && (int)filter.Value != -1)
                        {
                            state = (State)filter.Value;
                            // Set order...
                            if (state != null)
                            {
                                switch (state)
                                {
                                    default:
                                    case State.Trashed:
                                    case State.NotValided:
                                        orderBy = DataSorting.ModifiedDate;
                                        break;
                                    case State.Valided:
                                        orderBy = DataSorting.Validation;
                                        break;
                                }
                            }
                        }
                        else if (filter.Key == allstate)
                        {
                            // Set order...
                            orderBy = DataSorting.ModifiedDate;
                        }
                        else if (filter.Key == QueryFilter.Title
                            && string.IsNullOrWhiteSpace((string)filter.Value) == false)
                        {
                            title = (string)filter.Value;
                        }
                        else if (filter.Key == QueryFilter.Mine
                            && filter.Value != null && (bool)filter.Value == true)
                        {
                            mine = true;
                        }
                        else if (filter.Key == QueryFilter.MineToo
                            && filter.Value != null && (bool)filter.Value == true)
                        {
                            mineToo = true;
                        }
                        else if (filter.Key.StartsWith(QueryFilter.EndDate) == true)
                        {
                            endDate = (DateTime)filter.Value;
                            orderBy = DataSorting.StartDate;
                        }
                        else if (filter.Key == QueryFilter.Group)
                        {
                            filterOngroup = true;
                            if (userGroupIds != null && userGroupIds.Count > 0)
                            {
                                if (userGroupIds.Contains((int)filter.Value) == true)
                                {
                                    userGroups.Group1 = (int)filter.Value;
                                    userGroups.Group2 = -1;
                                    userGroups.Group3 = -1;
                                    userGroups.Group4 = -1;
                                    userGroups.Group5 = -1;
                                    userGroups.Group6 = -1;
                                    userGroups.Group7 = -1;
                                    userGroups.Group8 = -1;
                                    userGroups.Group9 = -1;
                                    userGroups.Group10= -1;
                                }
                                else
                                {
                                    // The asked group is not part of the user groups...so return an empty list...
                                    return null;
                                }
                            }
                            else
                            {
                                // User is not part of any group...so return an empty list...
                                return null;
                            }
                        }
                        else if (filter.Key == QueryFilter.ShowEventPostsOnly)
                        {
                            orderBy = DataSorting.StartDateDesc;
                        }
                    }
                }
            }
            // When mineToo is enabled always sort by modified date...
            if (mineToo == true && orderBy != DataSorting.StartDate && orderBy != DataSorting.StartDateDesc)
            {
                orderBy = DataSorting.ModifiedDate;
            }

            // Base query...
            var query = appContext.AppDbContext.Posts?.Where(p => p.SiteId == appContext.Site.Id);
            // Highlight filter...
            if (highlight != null)
            {
                query = query?.Where(p => p.Highlight == highlight);
            }
            // Title filter...
            if (title != null)
            {
                query = query?.Where(p => p.Title.Contains(title));
            }
            // Filter on start date...
            if (endDate != null)
            {
                query = query?
                    .Where(p => p.EndDate >= endDate);
            }
            // Mine filter...
            if (mine == true)
            {
                if (appContext?.User == null
                    || (userRole == null || userRole == ClaimValueRole.Reader))
                {
                    return null;
                }
                query = query?.Where(p => p.CreatorId == appContext.User.Id);
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

            // Apply filters to the query...
            if (filters != null)
            {
                foreach (KeyValuePair<string, object> filter in filters)
                {
                    if (filter.Key != null && filter.Value != null)
                    {
                        if (filter.Key == QueryFilter.CategorieSingle)
                        {
                            int catId = (int)filter.Value;
                            if (catId > 0)
                            {
                                query = query?
                                    .Where(p => p.Category1 == catId || p.Category2 == catId
                                        || p.Category3 == catId || p.Category4 == catId
                                        || p.Category5 == catId || p.Category6 == catId
                                        || p.Category7 == catId || p.Category8 == catId
                                        || p.Category9 == catId || p.Category10 == catId);
                            }
                        }
                        else if (filter.Key == QueryFilter.TagSingle)
                        {
                            int tagId = (int)filter.Value;
                            if (tagId > 0)
                            {
                                query = query?
                                    .Where(p => p.Tag1 == tagId || p.Tag2 == tagId
                                        || p.Tag3 == tagId || p.Tag4 == tagId
                                        || p.Tag5 == tagId || p.Tag6 == tagId
                                        || p.Tag7 == tagId || p.Tag8 == tagId
                                        || p.Tag9 == tagId || p.Tag10 == tagId);
                            }
                        }
                        else if (filter.Key.StartsWith(QueryFilter.Categorie) == true)
                        {
                            List<int> idList = (List<int>)filter.Value;
                            if (idList != null && idList.Count > 0)
                            {
                                if (filter.Key[filter.Key.Length - 1] == '!')
                                {
                                    query = query?
                                        .Where(p => idList.Contains(p.Category1) == false
                                            && idList.Contains(p.Category2) == false
                                            && idList.Contains(p.Category3) == false
                                            && idList.Contains(p.Category4) == false
                                            && idList.Contains(p.Category5) == false
                                            && idList.Contains(p.Category6) == false
                                            && idList.Contains(p.Category7) == false
                                            && idList.Contains(p.Category8) == false
                                            && idList.Contains(p.Category9) == false
                                            && idList.Contains(p.Category10) == false);
                                }
                                else
                                {
                                    query = query?
                                        .Where(p => (p.Category1 != -1 && idList.Contains(p.Category1) == true)
                                            || (p.Category2 != -1 && idList.Contains(p.Category2) == true)
                                            || (p.Category3 != -1 && idList.Contains(p.Category3) == true)
                                            || (p.Category4 != -1 && idList.Contains(p.Category4) == true)
                                            || (p.Category5 != -1 && idList.Contains(p.Category5) == true)
                                            || (p.Category6 != -1 && idList.Contains(p.Category6) == true)
                                            || (p.Category7 != -1 && idList.Contains(p.Category7) == true)
                                            || (p.Category8 != -1 && idList.Contains(p.Category8) == true)
                                            || (p.Category9 != -1 && idList.Contains(p.Category9) == true)
                                            || (p.Category10 != -1 && idList.Contains(p.Category10) == true));
                                }
                            }
                        }
                        else if (filter.Key == QueryFilter.ShowEventPostsOnly)
                        {
                            query = query?
                                .Where(p => p.StartDate != null);
                        }

                        else if (filter.Key == QueryFilter.ExcludePostsEvent)
                        {
                            query = query?
                                .Where(p => p.StartDate == null);
                        }
                    }
                }
            }

            // Administrator and publicator have read rights on all posts of the same groups...
            if ((userRole == ClaimValueRole.Administrator || userRole == ClaimValueRole.Publicator)
                && userGroups != null)
            {
                int queryState = (state != null) ? (int)state.Value : -1;
                query = query?
                    .Where(p => (queryState == -1 || p.State == (State)queryState) && ((p.Private == false && filterOngroup == false)
                            || (p.Group1 != -1 && (userGroups.Group1 == p.Group1 || userGroups.Group2 == p.Group1 || userGroups.Group3 == p.Group1 || userGroups.Group4 == p.Group1 || userGroups.Group5 == p.Group1 || userGroups.Group6 == p.Group1 || userGroups.Group7 == p.Group1 || userGroups.Group8 == p.Group1 || userGroups.Group9 == p.Group1 || userGroups.Group10 == p.Group1))
                            || (p.Group2 != -1 && (userGroups.Group1 == p.Group2 || userGroups.Group2 == p.Group2 || userGroups.Group3 == p.Group2 || userGroups.Group4 == p.Group2 || userGroups.Group5 == p.Group2 || userGroups.Group6 == p.Group2 || userGroups.Group7 == p.Group2 || userGroups.Group8 == p.Group2 || userGroups.Group9 == p.Group2 || userGroups.Group10 == p.Group2))
                            || (p.Group3 != -1 && (userGroups.Group1 == p.Group3 || userGroups.Group2 == p.Group3 || userGroups.Group3 == p.Group3 || userGroups.Group4 == p.Group3 || userGroups.Group5 == p.Group3 || userGroups.Group6 == p.Group3 || userGroups.Group7 == p.Group3 || userGroups.Group8 == p.Group3 || userGroups.Group9 == p.Group3 || userGroups.Group10 == p.Group3))
                            || (p.Group4 != -1 && (userGroups.Group1 == p.Group4 || userGroups.Group2 == p.Group4 || userGroups.Group3 == p.Group4 || userGroups.Group4 == p.Group4 || userGroups.Group5 == p.Group4 || userGroups.Group6 == p.Group4 || userGroups.Group7 == p.Group4 || userGroups.Group8 == p.Group4 || userGroups.Group9 == p.Group4 || userGroups.Group10 == p.Group4))
                            || (p.Group5 != -1 && (userGroups.Group1 == p.Group5 || userGroups.Group2 == p.Group5 || userGroups.Group3 == p.Group5 || userGroups.Group4 == p.Group5 || userGroups.Group5 == p.Group5 || userGroups.Group6 == p.Group5 || userGroups.Group7 == p.Group5 || userGroups.Group8 == p.Group5 || userGroups.Group9 == p.Group5 || userGroups.Group10 == p.Group5))
                            || (p.Group6 != -1 && (userGroups.Group1 == p.Group6 || userGroups.Group2 == p.Group6 || userGroups.Group3 == p.Group6 || userGroups.Group4 == p.Group6 || userGroups.Group5 == p.Group6 || userGroups.Group6 == p.Group6 || userGroups.Group7 == p.Group6 || userGroups.Group8 == p.Group6 || userGroups.Group9 == p.Group6 || userGroups.Group10 == p.Group6))
                            || (p.Group7 != -1 && (userGroups.Group1 == p.Group7 || userGroups.Group2 == p.Group7 || userGroups.Group3 == p.Group7 || userGroups.Group4 == p.Group7 || userGroups.Group5 == p.Group7 || userGroups.Group6 == p.Group7 || userGroups.Group7 == p.Group7 || userGroups.Group8 == p.Group7 || userGroups.Group9 == p.Group7 || userGroups.Group10 == p.Group7))
                            || (p.Group8 != -1 && (userGroups.Group1 == p.Group8 || userGroups.Group2 == p.Group8 || userGroups.Group3 == p.Group8 || userGroups.Group4 == p.Group8 || userGroups.Group5 == p.Group8 || userGroups.Group6 == p.Group8 || userGroups.Group7 == p.Group8 || userGroups.Group8 == p.Group8 || userGroups.Group9 == p.Group8 || userGroups.Group10 == p.Group8))
                            || (p.Group9 != -1 && (userGroups.Group1 == p.Group9 || userGroups.Group2 == p.Group9 || userGroups.Group3 == p.Group9 || userGroups.Group4 == p.Group9 || userGroups.Group5 == p.Group9 || userGroups.Group6 == p.Group9 || userGroups.Group7 == p.Group9 || userGroups.Group8 == p.Group9 || userGroups.Group9 == p.Group9 || userGroups.Group10 == p.Group9))
                            || (p.Group10!= -1 && (userGroups.Group1 == p.Group10|| userGroups.Group2 == p.Group10|| userGroups.Group3 == p.Group10|| userGroups.Group4 == p.Group10|| userGroups.Group5 == p.Group10|| userGroups.Group6 == p.Group10|| userGroups.Group7 == p.Group10|| userGroups.Group8 == p.Group10|| userGroups.Group9 == p.Group10|| userGroups.Group10 == p.Group10))));
            }
            // Contributeur can read their post and published posts of the same groups...
            else if (userRole == ClaimValueRole.Contributor
                && userGroups != null)
            {
                if (state != null && state.Value != State.Valided)
                    return null;
                query = query?
                    .Where(p => (p.CreatorId == appContext.User.Id || p.State == State.Valided)
                        && ((p.Private == false && filterOngroup == false)
                            || (p.Group1 != -1 && (userGroups.Group1 == p.Group1 || userGroups.Group2 == p.Group1 || userGroups.Group3 == p.Group1 || userGroups.Group4 == p.Group1 || userGroups.Group5 == p.Group1 || userGroups.Group6 == p.Group1 || userGroups.Group7 == p.Group1 || userGroups.Group8 == p.Group1 || userGroups.Group9 == p.Group1 || userGroups.Group10 == p.Group1))
                            || (p.Group2 != -1 && (userGroups.Group1 == p.Group2 || userGroups.Group2 == p.Group2 || userGroups.Group3 == p.Group2 || userGroups.Group4 == p.Group2 || userGroups.Group5 == p.Group2 || userGroups.Group6 == p.Group2 || userGroups.Group7 == p.Group2 || userGroups.Group8 == p.Group2 || userGroups.Group9 == p.Group2 || userGroups.Group10 == p.Group2))
                            || (p.Group3 != -1 && (userGroups.Group1 == p.Group3 || userGroups.Group2 == p.Group3 || userGroups.Group3 == p.Group3 || userGroups.Group4 == p.Group3 || userGroups.Group5 == p.Group3 || userGroups.Group6 == p.Group3 || userGroups.Group7 == p.Group3 || userGroups.Group8 == p.Group3 || userGroups.Group9 == p.Group3 || userGroups.Group10 == p.Group3))
                            || (p.Group4 != -1 && (userGroups.Group1 == p.Group4 || userGroups.Group2 == p.Group4 || userGroups.Group3 == p.Group4 || userGroups.Group4 == p.Group4 || userGroups.Group5 == p.Group4 || userGroups.Group6 == p.Group4 || userGroups.Group7 == p.Group4 || userGroups.Group8 == p.Group4 || userGroups.Group9 == p.Group4 || userGroups.Group10 == p.Group4))
                            || (p.Group5 != -1 && (userGroups.Group1 == p.Group5 || userGroups.Group2 == p.Group5 || userGroups.Group3 == p.Group5 || userGroups.Group4 == p.Group5 || userGroups.Group5 == p.Group5 || userGroups.Group6 == p.Group5 || userGroups.Group7 == p.Group5 || userGroups.Group8 == p.Group5 || userGroups.Group9 == p.Group5 || userGroups.Group10 == p.Group5))
                            || (p.Group6 != -1 && (userGroups.Group1 == p.Group6 || userGroups.Group2 == p.Group6 || userGroups.Group3 == p.Group6 || userGroups.Group4 == p.Group6 || userGroups.Group5 == p.Group6 || userGroups.Group6 == p.Group6 || userGroups.Group7 == p.Group6 || userGroups.Group8 == p.Group6 || userGroups.Group9 == p.Group6 || userGroups.Group10 == p.Group6))
                            || (p.Group7 != -1 && (userGroups.Group1 == p.Group7 || userGroups.Group2 == p.Group7 || userGroups.Group3 == p.Group7 || userGroups.Group4 == p.Group7 || userGroups.Group5 == p.Group7 || userGroups.Group6 == p.Group7 || userGroups.Group7 == p.Group7 || userGroups.Group8 == p.Group7 || userGroups.Group9 == p.Group7 || userGroups.Group10 == p.Group7))
                            || (p.Group8 != -1 && (userGroups.Group1 == p.Group8 || userGroups.Group2 == p.Group8 || userGroups.Group3 == p.Group8 || userGroups.Group4 == p.Group8 || userGroups.Group5 == p.Group8 || userGroups.Group6 == p.Group8 || userGroups.Group7 == p.Group8 || userGroups.Group8 == p.Group8 || userGroups.Group9 == p.Group8 || userGroups.Group10 == p.Group8))
                            || (p.Group9 != -1 && (userGroups.Group1 == p.Group9 || userGroups.Group2 == p.Group9 || userGroups.Group3 == p.Group9 || userGroups.Group4 == p.Group9 || userGroups.Group5 == p.Group9 || userGroups.Group6 == p.Group9 || userGroups.Group7 == p.Group9 || userGroups.Group8 == p.Group9 || userGroups.Group9 == p.Group9 || userGroups.Group10 == p.Group9))
                            || (p.Group10!= -1 && (userGroups.Group1 == p.Group10|| userGroups.Group2 == p.Group10|| userGroups.Group3 == p.Group10|| userGroups.Group4 == p.Group10|| userGroups.Group5 == p.Group10|| userGroups.Group6 == p.Group10|| userGroups.Group7 == p.Group10|| userGroups.Group8 == p.Group10|| userGroups.Group9 == p.Group10|| userGroups.Group10 == p.Group10))));
            }
            // Reader can only read published post of the same groups...
            // (or user don't have role but have group)
            else if ((userRole == null || userRole == ClaimValueRole.Reader)
                && userGroups != null)
            {
                if (state != null && state.Value != State.Valided)
                    return null;
                query = query?
                    .Where(p => p.State == State.Valided
                        && ((p.Private == false && filterOngroup == false)
                            || (p.Group1 != -1 && (userGroups.Group1 == p.Group1 || userGroups.Group2 == p.Group1 || userGroups.Group3 == p.Group1 || userGroups.Group4 == p.Group1 || userGroups.Group5 == p.Group1 || userGroups.Group6 == p.Group1 || userGroups.Group7 == p.Group1 || userGroups.Group8 == p.Group1 || userGroups.Group9 == p.Group1 || userGroups.Group10 == p.Group1))
                            || (p.Group2 != -1 && (userGroups.Group1 == p.Group2 || userGroups.Group2 == p.Group2 || userGroups.Group3 == p.Group2 || userGroups.Group4 == p.Group2 || userGroups.Group5 == p.Group2 || userGroups.Group6 == p.Group2 || userGroups.Group7 == p.Group2 || userGroups.Group8 == p.Group2 || userGroups.Group9 == p.Group2 || userGroups.Group10 == p.Group2))
                            || (p.Group3 != -1 && (userGroups.Group1 == p.Group3 || userGroups.Group2 == p.Group3 || userGroups.Group3 == p.Group3 || userGroups.Group4 == p.Group3 || userGroups.Group5 == p.Group3 || userGroups.Group6 == p.Group3 || userGroups.Group7 == p.Group3 || userGroups.Group8 == p.Group3 || userGroups.Group9 == p.Group3 || userGroups.Group10 == p.Group3))
                            || (p.Group4 != -1 && (userGroups.Group1 == p.Group4 || userGroups.Group2 == p.Group4 || userGroups.Group3 == p.Group4 || userGroups.Group4 == p.Group4 || userGroups.Group5 == p.Group4 || userGroups.Group6 == p.Group4 || userGroups.Group7 == p.Group4 || userGroups.Group8 == p.Group4 || userGroups.Group9 == p.Group4 || userGroups.Group10 == p.Group4))
                            || (p.Group5 != -1 && (userGroups.Group1 == p.Group5 || userGroups.Group2 == p.Group5 || userGroups.Group3 == p.Group5 || userGroups.Group4 == p.Group5 || userGroups.Group5 == p.Group5 || userGroups.Group6 == p.Group5 || userGroups.Group7 == p.Group5 || userGroups.Group8 == p.Group5 || userGroups.Group9 == p.Group5 || userGroups.Group10 == p.Group5))
                            || (p.Group6 != -1 && (userGroups.Group1 == p.Group6 || userGroups.Group2 == p.Group6 || userGroups.Group3 == p.Group6 || userGroups.Group4 == p.Group6 || userGroups.Group5 == p.Group6 || userGroups.Group6 == p.Group6 || userGroups.Group7 == p.Group6 || userGroups.Group8 == p.Group6 || userGroups.Group9 == p.Group6 || userGroups.Group10 == p.Group6))
                            || (p.Group7 != -1 && (userGroups.Group1 == p.Group7 || userGroups.Group2 == p.Group7 || userGroups.Group3 == p.Group7 || userGroups.Group4 == p.Group7 || userGroups.Group5 == p.Group7 || userGroups.Group6 == p.Group7 || userGroups.Group7 == p.Group7 || userGroups.Group8 == p.Group7 || userGroups.Group9 == p.Group7 || userGroups.Group10 == p.Group7))
                            || (p.Group8 != -1 && (userGroups.Group1 == p.Group8 || userGroups.Group2 == p.Group8 || userGroups.Group3 == p.Group8 || userGroups.Group4 == p.Group8 || userGroups.Group5 == p.Group8 || userGroups.Group6 == p.Group8 || userGroups.Group7 == p.Group8 || userGroups.Group8 == p.Group8 || userGroups.Group9 == p.Group8 || userGroups.Group10 == p.Group8))
                            || (p.Group9 != -1 && (userGroups.Group1 == p.Group9 || userGroups.Group2 == p.Group9 || userGroups.Group3 == p.Group9 || userGroups.Group4 == p.Group9 || userGroups.Group5 == p.Group9 || userGroups.Group6 == p.Group9 || userGroups.Group7 == p.Group9 || userGroups.Group8 == p.Group9 || userGroups.Group9 == p.Group9 || userGroups.Group10 == p.Group9))
                            || (p.Group10!= -1 && (userGroups.Group1 == p.Group10|| userGroups.Group2 == p.Group10|| userGroups.Group3 == p.Group10|| userGroups.Group4 == p.Group10|| userGroups.Group5 == p.Group10|| userGroups.Group6 == p.Group10|| userGroups.Group7 == p.Group10|| userGroups.Group8 == p.Group10|| userGroups.Group9 == p.Group10|| userGroups.Group10 == p.Group10))
                            ));
            }
            // Have role but no group...
            else if (userRole == ClaimValueRole.Administrator || userRole == ClaimValueRole.Publicator)
            {
                query = query?.Where(p => (state == null || (state != null && p.State == state.Value))
                    && p.Private == false);
            }
            else if (userRole == ClaimValueRole.Contributor)
            {
                if (state != null && state.Value != State.Valided)
                    return null;
                query = query?.Where(p => (p.CreatorId == appContext.User.Id || p.State == State.Valided) && p.Private == false);
            }
            else if (userRole == ClaimValueRole.Reader)
            {
                if (state != null && state.Value != State.Valided)
                    return null;
                query = query?.Where(p => p.State == State.Valided && p.Private == false);
            }
            // No role and no group...
            else
            {
                if (state != null && state.Value != State.Valided)
                    return null;
                query = query?.Where(p => p.State == State.Valided && p.Private == false);
            }

            return query;
        }
    }
}
