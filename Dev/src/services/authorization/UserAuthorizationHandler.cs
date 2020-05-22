// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE
// !!!To increase perf, do not retrieve others table when getting a list of user!!! ///
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
    /// Authorization handler for user data.
    /// </summary>
    public class UserAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, ApplicationUser>
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
        /// User authorization handler constructor.
        /// </summary>
        /// <param name="signInManager"></param>
        /// <param name="userManager"></param>
        /// <param name="perfProvider"></param>
        /// <param name="loggerFactory"></param>
        public UserAuthorizationHandler(WcmsAppContext appContext,
            PerformanceProvider perfProvider,
            ILoggerFactory loggerFactory)
        {
            _AppContext = appContext;
            _PerfProvider = perfProvider;
            // Trace...
            if (LogDisabled == false)
            {
                _Log = loggerFactory?.CreateLogger("Authorization"/*typeof(UserAuthorizationHandler).FullName*/);
            }
        }

        /// <summary>
        /// Handle the authorization policy for user.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <param name="data"></param>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            ApplicationUser data)
        {
            bool isSignedIn = (context?.User == null) ? false : (_AppContext?.SignInManager?.IsSignedIn(context.User) ?? false);
            string userName = (context?.User == null) ? null : _AppContext?.UserManager?.GetUserName(context.User);
            // Special case...
            if (requirement?.Name == null)
            {
                _Log?.LogCritical($"Access denied to user \"{data?.Email}\": Null requirement.");
                context.Fail();
            }
            else if (requirement.Name != AuthorizationRequirement.Read
                && requirement.Name != AuthorizationRequirement.Update)
            {
                _Log?.LogCritical($"Access denied to user \"{data?.Email}\": Invalid requirement.");
                context.Fail();
            }
            else if (data == null)
            {
                _Log?.LogInformation($"{requirement.Name} access granted to null user.");
                context.Succeed(requirement);
            }
            else if (_AppContext == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to user \"{data.Email}\": Invalid application context.");
                context.Fail();
            }
            else if (_AppContext.SignInManager == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to user \"{data.Email}\": Invalid SignIn manager.");
                context.Fail();
            }
            else if (_AppContext.UserManager == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to user \"{data.Email}\": Invalid User manager.");
                context.Fail();
            }
            else if (context.User == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to user \"{data.Email}\": Invalid User data.");
                context.Fail();
            }
            // Check inputs...
            else if (requirement == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to user \"{data.Email}\": Invalid requirement.");
                context.Fail();
            }
            else if (data.UserName ==null
                || data.RequestSite == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to user \"{data.Email}\": Invalid user.");
                context.Fail();
            }
            // Check inconsistency...
            else if (data.SiteId != data.RequestSite.Id)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to user \"{data.Email}\": Site inconsistency.");
                context.Fail();
            }
            // Check user inconsistency...
            else if (isSignedIn == true && _AppContext.User == null)
            {
                // Signed user in the http context, but no user in the application context.
                _Log?.LogCritical($"{requirement.Name} access denied to user \"{data.Email}\": User inconsistency (http but not in app).");
                context.Fail();
            }
            else if (isSignedIn == false && _AppContext.User != null)
            {
                // No Signed user in the http context, but user in the application context.
                _Log?.LogCritical($"{requirement.Name} access denied to user \"{data.Email}\": User inconsistency (not in http but in app).");
                context.Fail();
            }
            else if ((isSignedIn == true && _AppContext.User != null)
                && (userName == null || userName != _AppContext.User.UserName))
            {
                // Different user signed in http context and in application context.
                _Log?.LogCritical($"{requirement.Name} access denied to user \"{data.Email}\":  \"{userName}\" not matching to context user \"{_AppContext.User.UserName}\".");
                context.Fail();
            }
            else if (_AppContext.User != null && _AppContext.User.SiteId != data.SiteId)
            {
                // Invalid user site...
                _Log?.LogCritical($"{requirement.Name} access denied to user \"{data.Email}\": \"{context?.User?.Identity?.Name}\" not allowed on this site.");
                context.Fail();
            }
            // Check access right...
            else if (_AppContext.User == null)
            {
                // Anonymous is not granted...
                _Log?.LogWarning($"{requirement.Name} access denied to user \"{data.Email}\": Anonymous user.");
                context.Fail();
            }
            // Check user groups:
            //  For read: user need to have at least one group of the user data.
            //  For other requirement: user need to have all groups of the user data.
            else if (requirement.Name == AuthorizationRequirement.Read
                && _AppContext.User.MemberOf(data) == false)
            {
                _Log?.LogWarning($"{requirement.Name} access denied to user \"{data.Email}\": \"{context?.User?.Identity?.Name}\" not in allowed groups.");
                context.Fail();
            }
            else if (requirement.Name != AuthorizationRequirement.Read
                && _AppContext.User.MemberOfAll(data) == false)
            {
                _Log?.LogWarning($"{requirement.Name} access denied to user \"{data.Email}\": \"{context?.User?.Identity?.Name}\" not in all allowed groups.");
                context.Fail();
            }
            // Administrator have all rights...
            else if (_AppContext.User.HasRole(ClaimValueRole.Administrator) == true)
            {
                _Log?.LogInformation($"{requirement.Name} access granted to user \"{data.Email}\": \"{context?.User?.Identity?.Name}\" as {ClaimValueRole.Administrator}.");
                context.Succeed(requirement);
            }
            // User have access to their data...
            else if (_AppContext.User.Id == data.Id)
            {
                if (requirement.Name == AuthorizationRequirement.Read 
                    || requirement.Name == AuthorizationRequirement.Add
                    || requirement.Name == AuthorizationRequirement.Update)
                {
                    _Log?.LogInformation($"{requirement.Name} access granted to user \"{data.Email}\": \"{context?.User?.Identity?.Name}\" as owner.");
                    context.Succeed(requirement);
                }
                else
                {
                    _Log?.LogWarning($"{requirement.Name} access denied to user \"{data.Email}\": \"{context?.User?.Identity?.Name}\" not allowed.");
                    context.Fail();
                }
            }
            // Other cannot see user details...
            else
            {
                _Log?.LogWarning($"{requirement.Name} access denied to user \"{data.Email}\": \"{context?.User?.Identity?.Name}\" not allowed.");
                context.Fail();
            }
            // Trace performance...
            _PerfProvider?.Add("UserAuthorizationHandler::Handle");
            // Return...
            return Task.CompletedTask;
        }

        /// <summary>
        /// Count users.
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
                appContext?.AddPerfLog("UserAuthorizationHandler::Count::Query built");
                // Execute the query...
                int count = await query.CountAsync();
                // Trace performance...
                appContext?.AddPerfLog("UserAuthorizationHandler::Count::Query executed");
                // Exit...
                return count;
            }
        }

        /// <summary>
        /// Get users.
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="filters"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="allFields"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ApplicationUser>> Get(Services.WcmsAppContext appContext, Dictionary<string, object> filters, int skip, int take, bool allFields)
        {
            DataSorting orderBy = DataSorting.Email;
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
            if (orderBy == DataSorting.UserName)
            {
                query = query.OrderBy(p => p.UserName);
            }
            else if (orderBy == DataSorting.Email)
            {
                query = query.OrderBy(p => p.Email);
            }
            // Fields included...
            if (allFields == true)
            {
                query = query
                    .Include(p => p.Claims)
                    .Select(p => new ApplicationUser
                    {
                        AccessFailedCount = p.AccessFailedCount,
                        Email = p.Email,
                        EmailConfirmed = p.EmailConfirmed,
                        Id = p.Id,
                        //LockoutEnabled = p.LockoutEnabled,
                        LockoutEnd = p.LockoutEnd,
                        //PhoneNumber = p.PhoneNumber,
                        PhoneNumberConfirmed = p.PhoneNumberConfirmed,
                        UserName = p.UserName,
                    });
                    //.Include(p => p.Creator); //TODO: Only need for Unit test.
            }
            // Trace performance...
            appContext?.AddPerfLog("UserAuthorizationHandler::Get::Query built");
            // Execute the query...
            IEnumerable<ApplicationUser> items = await query
                    .Skip(skip * take).Take(take)
                    .ToListAsync();
            // Trace performance...
            appContext?.AddPerfLog("UserAuthorizationHandler::Get::Query executed");
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
                foreach (KeyValuePair<string, string> filter in filters)
                {
                    if (filter.Key != null && filter.Value != null)
                    {
                        if (filter.Key == QueryFilter.Title)
                        {
                            fltrs.Add(QueryFilter.Title, WebUtility.HtmlEncode(filter.Value));
                        }
                        else if (filter.Key.StartsWith(QueryFilter.Group) == true)
                        {
                            int groupId = 0;
                            if (int.TryParse(filter.Value, out groupId) == true)
                            {
                                fltrs.Add(QueryFilter.Group, groupId);
                            }
                        }
                    }
                }
            }
            return fltrs;
        }

        /// <summary>
        /// Get user query.
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="filters"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        private static IQueryable<ApplicationUser> _GetQuery(Services.WcmsAppContext appContext, Dictionary<string, object> filters, ref DataSorting orderBy)
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

            // Get filter to apply on the user table...
            string title = null;
            bool filterOngroup = false;
            if (filters != null)
            {
                foreach (KeyValuePair<string, object> filter in filters)
                {
                    if (filter.Key != null && filter.Value != null)
                    {
                        if (filter.Key == QueryFilter.Title
                            && string.IsNullOrWhiteSpace((string)filter.Value) == false)
                        {
                            title = (string)filter.Value;
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
                        /*else if (filter.Key == QueryFilter.UserEnabled)
                        {
                        }
                        else if (filter.Key == QueryFilter.UserLocked)
                        {
                        }
                        else if (filter.Key == QueryFilter.EmailConfirmed)
                        {
                        }*/
                    }
                }
            }

            // Base query...
            var query = appContext.AppDbContext.Users?.Where(u => u.SiteId == appContext.Site.Id);
            // Title filter...
            if (title != null)
            {
                query = query?.Where(u => u.UserName.Contains(title) == true
                    || u.Email.Contains(title) == true);
            }

            // Filtering based on the region...
            if (appContext.Site.HasRegions == true && (appContext.Region?.Id ?? -1) != -1)
            {
                query = query?
                    .Where(u => u.Region1 == 0 || u.Region1 == appContext.Region.Id
                        || u.Region2 == 0 || u.Region2 == appContext.Region.Id
                        || u.Region3 == 0 || u.Region3 == appContext.Region.Id
                        || u.Region4 == 0 || u.Region4 == appContext.Region.Id
                        || u.Region5 == 0 || u.Region5 == appContext.Region.Id
                        || u.Region6 == 0 || u.Region6 == appContext.Region.Id
                        || u.Region7 == 0 || u.Region7 == appContext.Region.Id
                        || u.Region8 == 0 || u.Region8 == appContext.Region.Id
                        || u.Region9 == 0 || u.Region9 == appContext.Region.Id
                        || u.Region10 == 0 || u.Region10 == appContext.Region.Id);
            }

            // Administrator have read rights on all users of the same groups...
            if ((userRole == ClaimValueRole.Administrator) && userGroups != null)
            {
                query = query?
                    .Where(p => (p.Group1 != -1 && (userGroups.Group1 == p.Group1 || userGroups.Group2 == p.Group1 || userGroups.Group3 == p.Group1 || userGroups.Group4 == p.Group1 || userGroups.Group5 == p.Group1 || userGroups.Group6 == p.Group1 || userGroups.Group7 == p.Group1 || userGroups.Group8 == p.Group1 || userGroups.Group9 == p.Group1 || userGroups.Group10 == p.Group1))
                            || (p.Group2 != -1 && (userGroups.Group1 == p.Group2 || userGroups.Group2 == p.Group2 || userGroups.Group3 == p.Group2 || userGroups.Group4 == p.Group2 || userGroups.Group5 == p.Group2 || userGroups.Group6 == p.Group2 || userGroups.Group7 == p.Group2 || userGroups.Group8 == p.Group2 || userGroups.Group9 == p.Group2 || userGroups.Group10 == p.Group2))
                            || (p.Group3 != -1 && (userGroups.Group1 == p.Group3 || userGroups.Group2 == p.Group3 || userGroups.Group3 == p.Group3 || userGroups.Group4 == p.Group3 || userGroups.Group5 == p.Group3 || userGroups.Group6 == p.Group3 || userGroups.Group7 == p.Group3 || userGroups.Group8 == p.Group3 || userGroups.Group9 == p.Group3 || userGroups.Group10 == p.Group3))
                            || (p.Group4 != -1 && (userGroups.Group1 == p.Group4 || userGroups.Group2 == p.Group4 || userGroups.Group3 == p.Group4 || userGroups.Group4 == p.Group4 || userGroups.Group5 == p.Group4 || userGroups.Group6 == p.Group4 || userGroups.Group7 == p.Group4 || userGroups.Group8 == p.Group4 || userGroups.Group9 == p.Group4 || userGroups.Group10 == p.Group4))
                            || (p.Group5 != -1 && (userGroups.Group1 == p.Group5 || userGroups.Group2 == p.Group5 || userGroups.Group3 == p.Group5 || userGroups.Group4 == p.Group5 || userGroups.Group5 == p.Group5 || userGroups.Group6 == p.Group5 || userGroups.Group7 == p.Group5 || userGroups.Group8 == p.Group5 || userGroups.Group9 == p.Group5 || userGroups.Group10 == p.Group5))
                            || (p.Group6 != -1 && (userGroups.Group1 == p.Group6 || userGroups.Group2 == p.Group6 || userGroups.Group3 == p.Group6 || userGroups.Group4 == p.Group6 || userGroups.Group5 == p.Group6 || userGroups.Group6 == p.Group6 || userGroups.Group7 == p.Group6 || userGroups.Group8 == p.Group6 || userGroups.Group9 == p.Group6 || userGroups.Group10 == p.Group6))
                            || (p.Group7 != -1 && (userGroups.Group1 == p.Group7 || userGroups.Group2 == p.Group7 || userGroups.Group3 == p.Group7 || userGroups.Group4 == p.Group7 || userGroups.Group5 == p.Group7 || userGroups.Group6 == p.Group7 || userGroups.Group7 == p.Group7 || userGroups.Group8 == p.Group7 || userGroups.Group9 == p.Group7 || userGroups.Group10 == p.Group7))
                            || (p.Group8 != -1 && (userGroups.Group1 == p.Group8 || userGroups.Group2 == p.Group8 || userGroups.Group3 == p.Group8 || userGroups.Group4 == p.Group8 || userGroups.Group5 == p.Group8 || userGroups.Group6 == p.Group8 || userGroups.Group7 == p.Group8 || userGroups.Group8 == p.Group8 || userGroups.Group9 == p.Group8 || userGroups.Group10 == p.Group8))
                            || (p.Group9 != -1 && (userGroups.Group1 == p.Group9 || userGroups.Group2 == p.Group9 || userGroups.Group3 == p.Group9 || userGroups.Group4 == p.Group9 || userGroups.Group5 == p.Group9 || userGroups.Group6 == p.Group9 || userGroups.Group7 == p.Group9 || userGroups.Group8 == p.Group9 || userGroups.Group9 == p.Group9 || userGroups.Group10 == p.Group9))
                            || (p.Group10!= -1 && (userGroups.Group1 == p.Group10|| userGroups.Group2 == p.Group10|| userGroups.Group3 == p.Group10|| userGroups.Group4 == p.Group10|| userGroups.Group5 == p.Group10|| userGroups.Group6 == p.Group10|| userGroups.Group7 == p.Group10|| userGroups.Group8 == p.Group10|| userGroups.Group9 == p.Group10|| userGroups.Group10 == p.Group10)));
            }
            // Have role but no group...
            else if (userRole == ClaimValueRole.Administrator)
            {
                // Can only see user without group...
                query = query?
                    .Where(p => p.Group1 == -1 
                        && p.Group2 == -1
                        && p.Group3 == -1
                        && p.Group4 == -1
                        && p.Group5 == -1
                        && p.Group6 == -1
                        && p.Group7 == -1
                        && p.Group8 == -1
                        && p.Group9 == -1
                        && p.Group10== -1);
            }
            // No role and no group...
            else
            {
                return null;
            }

            return query;
        }
    }
}
