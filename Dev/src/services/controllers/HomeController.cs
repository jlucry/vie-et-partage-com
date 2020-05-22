using Microsoft.AspNetCore.Mvc;
using System;

namespace Services
{
    /// <summary>
    /// Home controller.
    /// </summary>
    [ResponseCache(CacheProfileName = "Default")]
    public class HomeController : BaseController
    {
        /// <summary>
        /// The Home controller constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public HomeController(Services.WcmsAppContext appContext)
            : base(appContext)
        {
            //Console.WriteLine($"--- Services.HomeController...");
        }

        public IActionResult Index()
        {
            return View();
        }

        //public IActionResult Region()
        //{
        //    ViewData["Message"] = $"Region={this.RouteData.Values[CRoute.RegionTagName]}";

        //    return View();
        //}

        //public IActionResult About()
        //{
        //    ViewData["Message"] = "Your application description page.";

        //    return View();
        //}

        //public IActionResult Contact()
        //{
        //    ViewData["Message"] = "Your contact page.";

        //    return View();
        //}

        [ResponseCache(CacheProfileName = "Never")]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
