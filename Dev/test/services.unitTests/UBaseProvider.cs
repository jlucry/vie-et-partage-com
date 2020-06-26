using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Threading.Tasks;
using UnitTests.Core;
using Xunit;
using Xunit.Abstractions;

namespace Services.UnitTests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class UBaseProvider
    {
        public static int LogMaxLevel = 1;
        public static bool onlyUtestLog = true;
        private static ILoggerFactory _LogFactory = null;

        private readonly ServiceCollection _services = null;
        private readonly IServiceProvider _serviceProvider = null;

        /// <summary>
        /// Sample: https://github.com/aspnet/MusicStore/blob/dev/test/MusicStore.Test/HomeControllerTest.cs
        /// </summary>
        public UBaseProvider(ITestOutputHelper output)
        {
            Output = output;

            // Create and configure the service collection...
            if ((_services = new ServiceCollection()) != null)
            {
                // Add options...
                _services.AddOptions();

                // Register hosting env...
                _services.AddScoped<IHostingEnvironment, UtHostingEnvironment>();

                // Add Entity Framework services to the services container.
                _services.AddDbContext<Models.AppDbContext>(options => options.UseSqlServer(DbUtil.ConnectionString));

                // Add Identity services to the services container.
                _services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<Models.AppDbContext>()
                    .AddDefaultTokenProviders();
                // To create User claims...
                _services.AddScoped<UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>, UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>>();

                // Inject context service.
                _services.AddScoped<PerformanceProvider, PerformanceProvider>();
                _services.AddScoped<Services.WcmsAppContext, Services.WcmsAppContext>();

                // Register authorization policy...
                _services.AddAuthorization(options =>
                {
                    // inline policies
                    options.AddPolicy(ClaimValueRole.Reader, policy =>
                    {
                        policy.RequireClaim(UserClaimType.Role, ClaimValueRole.Reader);
                        //policy.RequireRole(Role.Reader);
                    });
                    options.AddPolicy(ClaimValueRole.Contributor, policy =>
                    {
                        policy.RequireClaim(UserClaimType.Role, ClaimValueRole.Contributor);
                    });
                    options.AddPolicy(ClaimValueRole.Publicator, policy =>
                    {
                        policy.RequireClaim(UserClaimType.Role, ClaimValueRole.Publicator);
                    });
                    options.AddPolicy(ClaimValueRole.Administrator, policy =>
                    {
                        policy.RequireClaim(UserClaimType.Role, ClaimValueRole.Administrator);
                    });
                });
                // Register resource authorization handlers...
                _services.AddTransient<IAuthorizationHandler, SiteAuthorizationHandler>();
                _services.AddTransient<IAuthorizationHandler, PageAuthorizationHandler>();
                _services.AddTransient<IAuthorizationHandler, PostAuthorizationHandler>();

                // Add logging services...
                _services.AddLogging();
            }

            // Build the service provider...
            _serviceProvider = _services?.BuildServiceProvider();
        }

        protected ITestOutputHelper Output { get; set; }
        
        /// <summary>
        /// Create and check the Db context.
        /// </summary>
        /// <returns></returns>
        protected AppDbContext CreateAndCheckDbContext(bool prod = false)
        {
            AppDbContext dbctx = DbUtil.CreateDbContext(prod);
            Assert.NotEqual(null, dbctx);
            return dbctx;
        }
        
        /// <summary>
        /// Create and init the context.
        /// </summary>
        /// <param name="dbctx"></param>
        /// <param name="host"></param>
        /// <param name="path"></param>
        /// <param name="regionName"></param>
        /// <param name="user"></param>
        /// <param name="checkInitResults"></param>
        /// <returns></returns>
        protected async Task<WcmsAppContext> CreateAndInitAppContext(AppDbContext dbctx, string host, string path, string regionName, ApplicationUser user = null, bool checkInitResults = true)
        {
            // Check the service provider...
            Assert.NotEqual(null, _serviceProvider);

            // Create and init the http context...
            var httpContext = new DefaultHttpContext();
            Assert.NotEqual(null, httpContext);
            // Configure the http context...
            httpContext.RequestServices = _services.BuildServiceProvider();
            Assert.NotEqual(null, httpContext.RequestServices);
            httpContext.Request.Host = new HostString(host);
            httpContext.Request.Path = new PathString(path);
            // Add user to the http context...
            if (user != null)
            {
                UserClaimsPrincipalFactory<ApplicationUser, IdentityRole> clmFact 
                    = _GetRequiredServicee<UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>>(httpContext.RequestServices);
                Assert.NotEqual(null, clmFact);
                httpContext.User = /*ClaimsPrincipal upp =*/ await clmFact.CreateAsync(user);
                Assert.NotEqual(null, httpContext.User/*upp*/);
                //httpContext.User = new ClaimsPrincipal(upp);
            }

            // Create and init the route context...
            var routeContext = new RouteContext(httpContext);
            Assert.NotEqual(null, routeContext);
            // Configure the route context...
            routeContext.RouteData = new RouteData();
            Assert.NotEqual(null, routeContext.RouteData);
            routeContext.RouteData.Values.Add(CRoute.RegionTagName, regionName);

            // Build loger factory...
            var logFactory = 
                _GetRequiredServicee<ILoggerFactory>(httpContext.RequestServices);
            Assert.NotEqual(null, logFactory);
#if DEBUG
            //logFactory.AddConsole(_LogFilter);
            //logFactory.AddDebug(_LogFilter);
#endif

            // Create and init the context...
            WcmsAppContext ctx =
                _GetRequiredServicee<WcmsAppContext>(httpContext.RequestServices);
            Assert.NotEqual(null, ctx);
            Assert.Equal(null, ctx.User);
            Assert.Equal(null, ctx.Site);
            //ctx.UnitTestInit(dbctx);
            int initSiteAsyncRes = await ctx.InitSiteAsync(httpContext,
                _GetRequiredServicee<IAuthorizationService>(httpContext.RequestServices));
            bool initRouteAsyncRes = await ctx.InitRouteAsync(routeContext);
            if (checkInitResults == true)
            {
                Assert.Equal(3, initSiteAsyncRes); // No module registered.
                Assert.Equal(true, initRouteAsyncRes);
            }
            return ctx;
        }

        /// <summary>
        /// Log message.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="ctx"></param>
        /// <param name="message"></param>
        protected void _Log(int level, WcmsAppContext ctx, string message)
        {
            if (level > LogMaxLevel)
                return;
            ctx?.Log?.LogInformation(message);
            Output?.WriteLine(message);
        }

        /// <summary>
        /// Get required service.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private T _GetRequiredServicee<T>(IServiceProvider serviceProvider = null)
        {
            if (serviceProvider != null)
            {
                return serviceProvider.GetRequiredService<T>();
            }
            return _serviceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Log filter.
        /// </summary>
        private Func<string, LogLevel, bool> _LogFilter = delegate (string s, LogLevel l)
        {
            if (s.Contains("DefaultAuthorizationService") == true)
                return false;
            else if (s.Contains("Services.WcmsAppContext") == true)
                return true;
            else if (s == "Authorization" && (SiteAuthorizationHandler.LogDisabled == false
                || PageAuthorizationHandler.LogDisabled == false
                || PostAuthorizationHandler.LogDisabled == false
                || UserAuthorizationHandler.LogDisabled == false))
                return true;
            else if (onlyUtestLog == true)
                return false;
#if DEBUG
            return true;
#else
            return false;
#endif
        };

        protected void Assert_Equal<T>(WcmsAppContext ctx, T expected, T actual, string error)
        {
            try
            {
                //if (expected?.ToString() != actual?.ToString())
                //{
                //    int brk = 0;
                //}
                Assert.Equal(expected, actual);
            }
            catch(Exception e)
            {
                _Log(1, ctx, $"{expected} not equal to {actual}: " + error);
                throw e;
            }
        }

        protected void Assert_NotEqual<T>(WcmsAppContext ctx, T notExpected, T actual, string error)
        {
            try
            {
                //if (notExpected?.ToString() == actual?.ToString())
                //{
                //    int brk = 0;
                //}
                Assert.NotEqual(notExpected, actual);
            }
            catch (Exception e)
            {
                _Log(1, ctx, $"{notExpected} equal to {actual}: " + error);
                throw e;
            }
        }
    }
}
