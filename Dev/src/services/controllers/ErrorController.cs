using Microsoft.AspNetCore.Mvc;
using System;

namespace Services
{
    /// <summary>
    /// Home controller.
    /// </summary>
    [ResponseCache(CacheProfileName = "Never")]
    public class ErrorController : BaseController
    {
        /// <summary>
        /// The Error controller constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public ErrorController(Services.WcmsAppContext appContext)
            : base(appContext)
        {
            //Console.WriteLine($"--- Services.ErrorController...");
        }

        public IActionResult Index()
        {
            ViewData["Message"] = (AppContext?.Exception != null)
                ? AppContext?.Exception
                : "An error occurred while processing your request!";
            return View();
        }
    }
}
