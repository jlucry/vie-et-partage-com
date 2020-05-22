using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNetCore.Hosting;
using Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Services
{
    /// <summary>
    /// Authorization middleware.
    /// </summary>
    public class AuthorizationMiddleware
    {
        private char[] _sep = new char[] { '/', '\\' };

        // Next middleware...
        private readonly RequestDelegate _next;

        /// <summary>
        /// Authorization middleware constructor.
        /// </summary>
        /// <param name="next"></param>
        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Execute the authorization middleware logic.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="appContext"></param>
        /// <param name="hostEnvironment"></param>
        /// <param name="authorizationService"></param>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context,
            Services.WcmsAppContext appContext,
            IHostingEnvironment hostEnvironment,
            IAuthorizationService authorizationService,
            ILoggerFactory loggerFactory)
        {
            int init = 0;
            bool adminPages = false;
            bool needUser = false;
            string virtualPath = null;
            IFileInfo flInf = null;
            var sw = new Stopwatch(); sw.Start();

            // Checking....
            if (context == null)
            {
                throw new Exception("Authorization failed: Invalid context!!!");
            }
            else if (appContext == null)
            {
                throw new Exception("Authorization failed: Invalid app context!!!");
            }
            else if (hostEnvironment == null
                || hostEnvironment.ContentRootPath == null)
            {
                throw new Exception("Authorization failed: Invalid app env!!!");
            }
            else if (authorizationService == null)
            {
                throw new Exception("Authorization failed: Invalid authz!!!");
            }
            else if ((virtualPath = context?.Request?.Path.Value) == null)
            {
                throw new Exception("Authorization failed: Invalid path!!!");
            }
            // Init...
            ILogger log = loggerFactory?.CreateLogger(typeof(AuthorizationMiddleware).FullName);

            // Manage redirection from old vieetpartage site...
#if TRUE
            {
                string redirection = VepUrlRedirection.Migrate(
                    Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(context?.Request));
                if (redirection != null)
                {
                    context.Response.Redirect(redirection, false);
                    return;
                }
            }
#endif

            // Special cases where we don't need to initialize and check the access right...
            if (virtualPath == "/Plaintext")
            {
                // Performance test cases...
                await context.Response.WriteAsync("Ok\r\n");
                // Trace performance and exit...
                appContext?.AddPerfLog("AuthorizationMiddleware::Plaintext");
                return;
            }
#if DEBUG
            //else if (virtualPath.Contains("ng2") == true)
            //{
            //    // Allow common lib files...
            //    log?.LogInformation("Access granted to path \"{0}\": Lib files.", virtualPath);
            //    // Execute the next middleware...
            //    await _NextMiddleWare(appContext, "AuthorizationMiddleware::3.1", "AuthorizationMiddleware::3.1", context);
            //    return;
            //}
#endif
            else if (virtualPath.StartsWith(CRoute.RouteStaticFile_Lib) == true)
            {
                // Allow common lib files...
                log?.LogInformation("Access granted to path \"{0}\": Lib files.", virtualPath);
                // Execute the next middleware...
                await _NextMiddleWare(appContext, "AuthorizationMiddleware::3.1", "AuthorizationMiddleware::3.1", context);
                return;
            }
            else if (virtualPath.StartsWith($"{CRoute.RouteStaticFile_Admin}/assets") == true
                    || virtualPath.StartsWith($"{CRoute.RouteStaticFile_Admin}/global") == true)
            {
                // Allow admin lib files...
                log?.LogInformation("Access granted to path \"{0}\": Admin Lib files.", virtualPath);
                // Execute the next middleware...
                await _NextMiddleWare(appContext, "AuthorizationMiddleware::3.2", "AuthorizationMiddleware::3.2", context);
                return;
            }
            else if (virtualPath.StartsWith($"{CRoute.RouteStaticFile_Theme}") == true)
            {
                // Allow theme files...
                log?.LogInformation("Access granted to path \"{0}\": Theme files.", virtualPath);
                // Execute the next middleware...
                await _NextMiddleWare(appContext, "AuthorizationMiddleware::3.3", "AuthorizationMiddleware::3.3", context);
                return;
            }
            else if (virtualPath.StartsWith("/jollyany") == true)
            {
                // Jollyany theme...
                context.Request.Path = new PathString(virtualPath.Replace("/jollyany", $"{CRoute.RouteStaticFile_Theme}jollyany"));
                // Allow theme files...
                log?.LogInformation("Access granted to path \"{0}\": Theme files.", virtualPath);
                // Execute the next middleware...
                await _NextMiddleWare(appContext, "AuthorizationMiddleware::3.4", "AuthorizationMiddleware::3.4", context);
                return;
            }

            // Route overwritting cases...
            else if (virtualPath.StartsWith(CRoute.RouteStaticFile_Admin) == true)
            {
                // Admin area...
                if (virtualPath == CRoute.RouteStaticFile_Admin)
                {
                    // Redirect...
                    context.Response.Redirect($"{CRoute.RouteStaticFile_Admin}/");
                    appContext?.AddPerfLog("AuthorizationMiddleware::/admin");
                    return;
                }
                else if (virtualPath == $"{CRoute.RouteStaticFile_Admin}/")
                {
                    needUser = true;
                    adminPages = true;
                    virtualPath += "index.html";
                    context.Request.Path = new PathString(virtualPath);
                }
                else if (virtualPath == $"{CRoute.RouteStaticFile_Admin}/index"
                    || virtualPath == $"{CRoute.RouteStaticFile_Admin}/posts"
                    || virtualPath == $"{CRoute.RouteStaticFile_Admin}/pages"
                    || virtualPath == $"{CRoute.RouteStaticFile_Admin}/calendar"
                    || virtualPath == $"{CRoute.RouteStaticFile_Admin}/users")
                {
                    needUser = true;
                    adminPages = true;
                    virtualPath += ".html";
                    context.Request.Path = new PathString(virtualPath);
                }
                else
                {
                    // Admin page not allowsed, redirect to admin error page...
                    log?.LogInformation("\"{0}\" not allowed, redirect to admin error page.", virtualPath);
                    virtualPath = $"{CRoute.RouteStaticFile_Admin}/error-404.html";
                    context.Request.Path = new PathString(virtualPath);
                    // Execute the next middleware...
                    await _NextMiddleWare(appContext, "AuthorizationMiddleware::3.1", "AuthorizationMiddleware::3.3", context);
                    return;
                }
            }
            
            // Initialize the application the site context
            // and check if the site can be view by the current user...
            if ((init = await appContext.InitSiteAsync(context, authorizationService)) != 1)
            {
                // We failed to initialize the context...
                if (init == 0)
                {
                    // The site cannot be found.
                    log?.LogError("Site {0} cannot be found!!!", context.Request.Host);
                    _StopChain(appContext, "AuthorizationMiddleware::40", context);
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync($"Site {context.Request.Host} cannot be found!!!");
                }
                else if (init == 2)
                {
                    // The region cannot be found.
                    log?.LogInformation("Region {0} cannot be found!!!", appContext.RouteRegionName);
                    _StopChain(appContext, "AuthorizationMiddleware::41", context);
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync($"Region {appContext.RouteRegionName} cannot be found!!!");
                }
                else if (init == 3)
                {
                    // The site module cannot be found.
                    log?.LogInformation("{0} module cannot be found!!!", appContext.RouteRegionName);
                    _StopChain(appContext, "AuthorizationMiddleware::44", context);
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync($"{appContext.RouteRegionName} module cannot be found!!!");
                }
                else
                {
                    // Access is not granted, redirect to login page...
                    if (context.Request.Path.Value.ToLower() == CRoute.RouteAccountLogin.ToLower())
                    {
                        // Here something failed because we should not have a redirection on the login page.
                        log?.LogCritical("Internal ERROR: Init failed for {0}!!!", context.Request.Path);
                        _StopChain(appContext, "AuthorizationMiddleware::42", context);
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync($"Internal ERROR: Init failed for {context.Request.Path}!!!");
                    }
                    else
                    {
                        log?.LogInformation("Access denied to path \"{0}\": Redirect to {CRoute.RouteAccountLogin}.", virtualPath);
                        _StopChain(appContext, "AuthorizationMiddleware::43", context);
                        context.Response.StatusCode = 401;
                        //context.Response.Redirect(CRoute.RouteAccountLogin);
                    }
                }
                // Exit...
                return;
            }
            // Check if we need a signed user...
            else if (needUser == true 
                && appContext.SignInManager.IsSignedIn(context.User) == false)
            {
                // Here the page need a signed user, but no user is signed in,
                // so redirect to login page to authenticated the user...
                log?.LogInformation("Access denied to path \"{0}\": A signed user is needed.", virtualPath);
                if (adminPages == true)
                {
                    //virtualPath = virtualPath.Replace(".html", string.Empty);
                    //context.Request.Path = new PathString(virtualPath);
                    context.Response.Redirect(CRoute.RouteAccountLogin);
                }
                else
                {
                    context.Response.StatusCode = 401;
                }
                _StopChain(appContext, "AuthorizationMiddleware::6", context);
                return;
            }
            // Check access to the true file system...
            else if (((flInf = hostEnvironment?.WebRootFileProvider?.GetFileInfo(virtualPath))?.Exists ?? true) == true)
            {
                if (adminPages == true)
                {
                    // Allow admin pages...
                    log?.LogInformation("Access granted to path \"{0}\": Admin pages.", virtualPath);
                    await _NextMiddleWare(appContext, "AuthorizationMiddleware::7.1", "AuthorizationMiddleware::7.2", context);
                    return;
                }
                // Virtual path should not refer to the true file system...Denied the access...
                log?.LogInformation("Access denied to path \"{0}\": Access denied to the true FS.", virtualPath);
                context.Response.StatusCode = 403;
                _StopChain(appContext, "AuthorizationMiddleware::8", context);
                return;
            }
            else
            {
                log?.LogInformation("flInf.PhysicalPath=\"{0}\".", flInf?.PhysicalPath);

                // Create site path from the virtual path...
                string sitePath = $"/{appContext.Site.Id}{virtualPath}";
                // Is site path exist ?
                if (((flInf = hostEnvironment?.WebRootFileProvider?.GetFileInfo(sitePath))?.Exists ?? false) == true)
                {
                    // We have a site file, overwrite the path...
                    context.Request.Path = new PathString(sitePath);
                    // Check for root and libraries files (js libs, css, images and themes) of the site...
                    virtualPath = virtualPath.ToLower();
                    if (virtualPath == "/favicon.ico"
                        || virtualPath == "/robots.txt"
                        || virtualPath == "/podcast.xml"
                        || virtualPath == "/logo.png"
                        || virtualPath.StartsWith(CRoute.RouteStaticFile_Js) == true
                        || virtualPath.StartsWith(CRoute.RouteStaticFile_Css) == true
                        || virtualPath.StartsWith(CRoute.RouteStaticFile_Images) == true
                        || virtualPath.StartsWith(CRoute.RouteStaticFile_Lib) == true
                        || (virtualPath.StartsWith(CRoute.RouteStaticFile_Theme)) == true)
                    {
                        // Libraries are allowed...
                        log?.LogInformation("Access granted to path \"{0}\" (\"{1}\"): Site file public and common.", virtualPath, sitePath);
                        await _NextMiddleWare(appContext, "AuthorizationMiddleware::9.1", "AuthorizationMiddleware::9.2", context);
                        return;
                    }
                    // Check for post files...
                    else if (virtualPath.StartsWith(CRoute.RouteStaticFile_PostPub) == true)
                    {
                        // We have a post public file, allowed...
                        log?.LogInformation("Access granted to path \"{0}\": Access allowed to file from public post.", virtualPath);
                        await _NextMiddleWare(appContext, "AuthorizationMiddleware::10.1", "AuthorizationMiddleware::10.2", context);
                        return;
                    }
                    else if (virtualPath.StartsWith(CRoute.RouteStaticFile_Post) == true)
                    {
                        // We have a post private file, check access right...
                        int postId = 0;
                        Post post = null;
                        string[] folders = virtualPath.Split(_sep);
                        if (folders != null 
                            && folders.Length == 6
                            && int.TryParse(folders[4], out postId) == true
                            && (post = await (new PostProvider(appContext))?.Get(postId)) != null)
                        {
                            // Here, the current user have read access to the post...
                            log?.LogInformation("Access granted to path \"{0}\": Access allowed.", virtualPath);
                            await _NextMiddleWare(appContext, "AuthorizationMiddleware::11.1", "AuthorizationMiddleware::11.2", context);
                            return;
                        }
                    }
                    // Other files are not allowed...
                    log?.LogInformation("Access denied to path \"{0}\": Not allowed.", virtualPath);
                    context.Response.StatusCode = 403;
                    _StopChain(appContext, "AuthorizationMiddleware::12", context);
                    return;
                }
                else
                {
                    // This is not site file, let other middleware process the request...
                    log?.LogInformation("Let other middleware process \"{0} - {1}\".", virtualPath, flInf?.PhysicalPath);
                    await _NextMiddleWare(appContext, "AuthorizationMiddleware::13.1", "AuthorizationMiddleware::13.2", context);
                    return;
                }
            }
        }

        /// <summary>
        /// Call next middleware.
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="perfMsg1"></param>
        /// <param name="perfMsg2"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task _NextMiddleWare(WcmsAppContext appContext, string perfMsg1, string perfMsg2, 
            HttpContext context)
        {
            // Trace performance...
            appContext?.AddPerfLog(perfMsg1);
            {
                // Call next middleware...
                await _next(context);
            }
            // Trace performance...
            appContext?.AddPerfLog(perfMsg2);
        }

        /// <summary>
        /// Stop the middleware chain.
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="perfMsg1"></param>
        /// <param name="context"></param>
        private void _StopChain(WcmsAppContext appContext, string perfMsg1,
            HttpContext context)
        {
            // Trace performance...
            appContext?.AddPerfLog(perfMsg1);
        }
    }
}
