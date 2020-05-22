using Microsoft.AspNetCore.Mvc;
using Services;

namespace www.dfide
{
    /// <summary>
    /// Home controller.
    /// </summary>
    [ResponseCache(CacheProfileName = "Never")]
    public class DfideHomeController : BaseController
    {
        /// <summary>
        /// The Home controller constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public DfideHomeController(WcmsAppContext appContext)
            : base(appContext)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Dfide:Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Dfide:Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
