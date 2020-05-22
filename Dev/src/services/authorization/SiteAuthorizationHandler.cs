using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Authorization handler for site data.
    /// </summary>
    public class SiteAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Site>
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
        /// Site authorization handler constructor.
        /// </summary>
        /// <param name="signInManager"></param>
        /// <param name="userManager"></param>
        /// <param name="perfProvider"></param>
        /// <param name="loggerFactory"></param>
        public SiteAuthorizationHandler(WcmsAppContext appContext,
            PerformanceProvider perfProvider,
            ILoggerFactory loggerFactory)
        {
            _AppContext = appContext;
            _PerfProvider = perfProvider;
            // Trace...
            if (LogDisabled == false)
            {
                _Log = loggerFactory?.CreateLogger("Authorization"/*typeof(SiteAuthorizationHandler).FullName*/);
            }
            //_Log?.LogInformation("SiteAuthorizationHandler allocated.");
        }

        /// <summary>
        /// Handle the authorization policy for site.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <param name="site"></param>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Site site)
        {
            bool isSignedIn =  (context?.User == null) ? false : (_AppContext?.SignInManager?.IsSignedIn(context.User) ?? false);
            string userName = (context?.User == null) ? null : _AppContext?.UserManager?.GetUserName(context.User);
            // Special case...
            if (requirement?.Name == null)
            {
                _Log?.LogCritical($"Access denied to site \"{site?.Title}\": Null requirement.");
                context.Fail();
            }
            else if (requirement.Name.StartsWith(AuthorizationRequirement.Read) == false)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to site \"{site?.Title}\": Invalid requirement.");
                context.Fail();
            }
            else if (site == null)
            {
                _Log?.LogInformation($"{requirement.Name} access granted to null site.");
                context.Succeed(requirement);
            }
            else if (_AppContext == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to site \"{site.Title}\": Invalid application context.");
                context.Fail();
            }
            else if (_AppContext.SignInManager == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to site \"{site.Title}\": Invalid SignIn manager.");
                context.Fail();
            }
            else if (_AppContext.UserManager == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to site \"{site.Title}\": Invalid User manager.");
                context.Fail();
            }
            else if (context.User == null)
            {
                _Log?.LogCritical($"{requirement.Name} access denied to site \"{site.Title}\": Invalid User data.");
                context.Fail();
            }
            // Check user inconsistency...
            else if (isSignedIn == true && _AppContext.User == null)
            {
                // Signed user in the http context, but no user in the application context.
                _Log?.LogCritical($"{requirement.Name} access denied to site \"{site.Title}\": User inconsistency (http but not in app).");
                context.Fail();
            }
            else if (isSignedIn == false && _AppContext.User != null)
            {
                // No Signed user in the http context, but user in the application context.
                _Log?.LogCritical($"{requirement.Name} access denied to site \"{site.Title}\": User inconsistency (not in http but in app).");
                context.Fail();
            }
            else if ((isSignedIn == true && _AppContext.User != null) 
                && (userName == null || userName != _AppContext.User.UserName))
            {
                // Different user signed in http context and in application context.
                _Log?.LogWarning($"{requirement.Name} access denied to site \"{site.Title}\": \"{userName}\" not matching to context user \"{_AppContext.User.UserName}\".");
                context.Fail();
            }
            else if (_AppContext.User != null && _AppContext.User.SiteId != site.Id)
            {
                // Invalid user site...
                _Log?.LogWarning($"{requirement.Name} access denied to site \"{site.Title}\": \"{context?.User?.Identity?.Name}\" not allowed on this site.");
                context.Fail();
            }
            // Check access right...
            else if (_AppContext.User == null)
            {
                // Read Public site is granted to anonymous...
                if (site.Private == false && requirement.Name == AuthorizationRequirement.Read)
                {
                    _Log?.LogInformation($"{requirement.Name} access granted to site \"{site.Title}\": Public site allowed to anonymous.");
                    context.Succeed(requirement);
                }
                //// Site theme is granted to anonymous user (private and public site)...
                //else if (requirement.Name.StartsWith($"{AuthorizationRequirement.Read}/css/") == true
                //    || requirement.Name.StartsWith($"{AuthorizationRequirement.Read}/images/") == true
                //    || requirement.Name.StartsWith($"{AuthorizationRequirement.Read}/jss/") == true)
                //{
                //    _Log?.LogInformation($"{requirement.Name} access granted to site \"{site.Title}\": Css, images and jss files allowed to anonymous.");
                //    context.Succeed(requirement);
                //}
                // Registration (public site) and login (private and public site) is granted to anonymous user...
                else if (requirement.Name.ToLower() == $"{AuthorizationRequirement.Read}{CRoute.RouteAccountLogin}".ToLower()
                    || (site.Private == false && requirement.Name.ToLower() == $"{AuthorizationRequirement.Read}{CRoute.RouteAccountRegister}".ToLower()))
                {
                    _Log?.LogInformation($"{requirement.Name} access granted to site \"{site.Title}\": Registration and login page allowed to anonymous.");
                    context.Succeed(requirement);
                }
                else
                {
                    _Log?.LogWarning($"{requirement.Name} access denied to site \"{site.Title}\": Anonymous user.");
                    context.Fail();
                }
            }
            // Read requirement is granted to all registered user...
            else if (requirement.Name == AuthorizationRequirement.Read
                || requirement.Name == $"{AuthorizationRequirement.Read}{CRoute.RouteAccountLogin}"
                || requirement.Name == $"{AuthorizationRequirement.Read}{CRoute.RouteAccountRegister}")
            {
                _Log?.LogInformation($"{requirement.Name} access granted to site \"{site.Title}\": Read granted to \"{context?.User?.Identity?.Name}\".");
                context.Succeed(requirement);
            }
            // Update requirement is granted to admin member of all groups...
            else if (requirement.Name == AuthorizationRequirement.Update
                && _AppContext.User.HasRole(ClaimValueRole.Administrator) == true
                && _AppContext.User.MemberOfAllGroup() == true)
            {
                _Log?.LogInformation($"{requirement.Name} access granted to site \"{site.Title}\": \"{context?.User?.Identity?.Name}\" as site {ClaimValueRole.Administrator} of all groups.");
                context.Succeed(requirement);
            }
            // Access denied...
            else
            {
                _Log?.LogWarning($"{requirement.Name} access denied to site \"{site.Title}\": \"{context?.User?.Identity?.Name}\" not allowed.");
                context.Fail();
            }
            // Trace performance...
            _PerfProvider?.Add("SiteAuthorizationHandler::Handle");
            // Return...
            return Task.CompletedTask;
        }
    }
}
