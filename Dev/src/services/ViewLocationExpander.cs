using Contracts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// View location expander.
    /// </summary>
    public class ViewLocationExpander : IViewLocationExpander
    {
        // Logger...
        private readonly ILogger _log;

        /// <summary>
        /// Location expander constructor.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public ViewLocationExpander(ILoggerFactory loggerFactory)
        {
            _log = loggerFactory?.CreateLogger(typeof(ViewLocationExpander).FullName);
        }

        /// <summary>
        /// Expand view layout.
        /// </summary>
        /// <param name="viewContext"></param>
        /// <returns></returns>
        public static string ExpandViewLayout(ViewContext viewContext)
        {
            string layout = "_Layout";
            try
            {
                // Get the module from the route...
                Contracts.IModule mod = (Contracts.IModule)viewContext.RouteData.Values["module"];
                // Case of non area module...
                if (mod != null && mod.UseArea == false)
                {
                    // We want to use the module layout.
                    Contracts.ModuleController modCtrl = (Contracts.ModuleController)viewContext.RouteData.Values["moduleController"];
                    if ((modCtrl?.HaveLayout ?? false) == true)
                    {
                        layout = $"_{mod.Name}Layout";
                    }
                }
            }
            catch(Exception e)
            {
                // Something failed, here...
                throw new Exception("Failed to set the view layout", e);
            }
            return layout;
        }

        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations)
        {
            try
            {
                // Get the module from the route...
                IModule mod = (IModule)context.ActionContext.RouteData.Values["module"];
                // Case of area module...
                if ((mod?.UseArea ?? false) == true)
                {
                    // We want to use the module layout, so add it in
                    // location prior to the other paths...
                    return ExpandViewLocationsCore(viewLocations,
                        $"/Areas/{mod.Name/*.ToLower()*/}/Views/Shared/{{0}}.cshtml");
                }
            }
            catch (Exception e)
            {
                // Something failed, here...
                throw new Exception("Failed to set the view locations", e);
            }
            // Non module case...
            return ExpandViewLocationsCore(viewLocations);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewLocations"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        private IEnumerable<string> ExpandViewLocationsCore(IEnumerable<string> viewLocations, string area = null)
        {
            if (area != null)
            {
                yield return area;
            }
            foreach (var location in viewLocations)
            {
                //yield return location.Replace("/Areas/", "/Modules/");
                yield return location;
            }
        }
    }
}