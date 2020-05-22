using Contracts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Route handler.
    /// </summary>
    public class SiteRoute : IRouter
    {
        // Logger...
        private readonly ILogger _log;
        // Next route handler...
        private readonly IRouter _next;

        /// <summary>
        /// Route handler constructor.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="loggerFactory"></param>
        public SiteRoute(IRouter next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _log = loggerFactory?.CreateLogger(typeof(SiteRoute).FullName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            // We just want to act as a pass-through for link generation
            return _next.GetVirtualPath(context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task RouteAsync(RouteContext context)
        {
            ModuleController modCtrl = null;
            WcmsAppContext appContext = null;
            var sw = new Stopwatch(); sw.Start();

            //_log?.LogDebug("===============================================================");

            // Checking....
            if (context == null)
            {
                throw new Exception("Route failed: Invalid context!!!");
            }
            
            try
            {
                // Manage case where a lib file not found match to a MVC route...
                if (context.HttpContext.Request.Path.Value.StartsWith(CRoute.RouteStaticFile_Lib) == true)
                {
                    // This lib file is missing, so return a 404 error status code...
                    context.HttpContext.Response.StatusCode = 404;
                    return;
                }

                // Get the application context...
                if ((appContext = _GetAppContext(context)) == null)
                {
                    throw new Exception("Route failed: Invalid app context!!!");
                }

                // For diagnostics and link-generation purposes, routing should include
                // a list of IRoute instances that lead to the ultimate destination.
                // It's the responsibility of each IRouter to add the 'next' before calling it.
                context.RouteData.Routers.Add(_next);

                // Initialize the application context...
                if (await appContext.InitRouteAsync(context) == false)
                {
                    throw new Exception($"Route failed: Route initilisation failed for {context.HttpContext.Request.Path.Value}!!!");
                }

                // Get controller and action...
                string controller = (string)context.RouteData.Values["controller"];
                if (controller != null && controller != "Error")
                {
                    // Mvc and not WebApi...
                    string action = (string)context.RouteData.Values["action"];
                    // Customize the route based on the page (must be done before 
                    // the customization based on the module...
                    if (controller == "Page" && appContext.Page != null)
                    {
                        _log?.LogDebug("Page {0}: controller={1}, action={2}.",
                            appContext.Page.Id,
                            appContext.Page.Controller, appContext.Page.Action);
                        // Overwrite controller page and action based on the retrieved page...
                        context.RouteData.Values.Remove("controller");
                        context.RouteData.Values.Add("controller", controller = appContext.Page.Controller);
                        context.RouteData.Values.Remove("action");
                        context.RouteData.Values.Add("action", action = appContext.Page.Action);
                        // Check if we have to load a post for the page...
                        IEnumerable<PageClaim> claims = null;
                        if ((claims = appContext.Page.GetClaims(PageClaimType.FilterPostId)) != null)
                        {
                            foreach(PageClaim claim in claims)
                            {
                                await appContext.LoadPost(claim?.Value?.ToString());
                                break;
                            }
                        }
                    }

                    // Get site module...
                    if (appContext.Module == null)
                    {
                        throw new Exception("Route failed: No module found for the site!!!");
                    }
                    // Save the module to route data...
                    context.RouteData.Values.Add("module", appContext.Module);

                    // Get module controller...
                    string lowerController = controller.ToLower();
                    if (controller != null
                        && appContext.Module.Controllers.ContainsKey(lowerController) == true
                        && (modCtrl = appContext.Module.Controllers[lowerController]) != null)
                    {
                        // Trace...
                        _log?.LogDebug("Module controller: {0}: ControllerAndView={1}, HaveLayout={2}.",
                            modCtrl?.Name,
                            modCtrl?.HaveControllerAndView ?? false,
                            modCtrl?.HaveLayout ?? false);
                        // Save the module controller to route data...
                        context.RouteData.Values.Add("moduleController", modCtrl);
                    }

                    // Update the context with the module and controller...
                    appContext.InitModule(context, modCtrl);

                    // Customize the route based on the module...
                    if (modCtrl != null
                        && modCtrl.HaveControllerAndView == true)
                    {
                        if (appContext.Module.UseArea == true)
                        {
                            // Area customization...
                            context.RouteData.Values.Remove("area");
                            context.RouteData.Values.Add("area", appContext.Module.Name/*.ToLower()*/);
                            _log?.LogDebug("Route overwrited: {0}\\{1}\\{2}", appContext.Module.Name, controller, action);
                        }
                        else
                        {
                            // Controller name customization...
                            context.RouteData.Values.Remove("controller");
                            context.RouteData.Values.Add("controller", appContext.Module.Name + controller);
                            _log?.LogDebug("Route overwrited: {0}{1}\\{2}", appContext.Module.Name, controller, action);
                        }
                    }

                    // Save the context to route data...
                    context.RouteData.Values.Add("appcontext", appContext);
                }

                // Process duration...
                long routeDuration = sw.ElapsedMilliseconds;
                _log?.LogInformation("Route handling for \"{0}\" processed in {1} milliseconds.",
                    context?.HttpContext?.Request?.Path.Value?.ToString(), routeDuration);
                
                // Execute the next route handler...
                await _next.RouteAsync(context);

                // Process %...
                long routeDuration2 = sw.ElapsedMilliseconds;
                _log?.LogInformation("Route handling for \"{0}\" took {1}% of the process ({2} milliseconds).",
                    context?.HttpContext?.Request?.Path.Value?.ToString(),
                    (routeDuration2 == 0) ? 0 : ((routeDuration * 100) / routeDuration2),
                    routeDuration2);
            }
            finally
            { }
            //catch(Exception e)
            //{
            //    if (appContext != null)
            //        appContext.Exception = e.Message;
            //    throw e;
            //}
        }

        /// <summary>
        /// Get the application context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static WcmsAppContext _GetAppContext(RouteContext context)
        {
            return ((Services.WcmsAppContext)context.HttpContext.RequestServices.GetService(Type.GetType("Services.WcmsAppContext")));
        }
    }
}
