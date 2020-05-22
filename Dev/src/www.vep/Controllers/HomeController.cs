using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace www.vep
{
    /// <summary>
    /// Home controller.
    /// </summary>
    [Area("Vep")]
    //[Route("[Area]/[Controller]/[Action]", Order = -2)]
    [ResponseCache(CacheProfileName = "Default")]
    public class HomeController : BaseController
    {
        /// <summary>
        /// The Home controller constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public HomeController(WcmsAppContext appContext)
            : base(appContext)
        {
            //_Log?.LogDebug("vep.HomeController: {0}", this.GetType().Name);
            //Console.WriteLine($"--- www.vep.HomeController...");
        }

        /// <summary>
        /// Home page.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            // Set page expiration...
            UpdateExpirationToNextHour();

            await _SetViewag();
            return View("Region");
        }

        /// <summary>
        /// Region page.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Region()
        {
            // Set page expiration...
            UpdateExpirationToNextHour();

            await _SetViewag();
            return View();
        }

        //public async Task<IActionResult> About()
        //{
        //    string debugString = string.Empty;
        //    SiteProvider provider = new SiteProvider(AppContext);
        //    // Get test...
        //    Site site = await provider?.Get(Request.Host.Value);
        //    debugString += $"Site {Request.Host.Value}: {site?.Id} ({site?.SiteClaims?.Count()} claims).\r\n";
        //    // Set claims test...
        //    await provider?.SetClaims(1,
        //        new List<SiteClaim>
        //        {
        //            new SiteClaim
        //            {
        //                Type = "type1",
        //                StringValue = "type1Value1"
        //            },
        //            new SiteClaim
        //            {
        //                Type = "type1",
        //                StringValue = "type1Value2"
        //            }
        //        });
        //    // Update claims...
        //    IEnumerable<SiteClaim> siteClaims = await provider.GetClaims(1, "type1");
        //    debugString += $"{siteClaims?.Count()} type1 claims.\r\n";
        //    if (siteClaims != null && siteClaims.Count() >= 2)
        //    {
        //        await provider?.SetClaims(1,
        //            new List<SiteClaim>
        //            {
        //                new SiteClaim
        //                {
        //                    Id = siteClaims.ElementAt(0).Id,
        //                    Type = "type1Modifier",
        //                    StringValue = $"type1Value1_{DateTime.Now.ToString()}"
        //                },
        //                new SiteClaim
        //                {
        //                    Id = siteClaims.ElementAt(1).Id,
        //                    Type = "type1",
        //                }
        //            });
        //        // Modify and delete site claims...
        //        siteClaims = await provider.GetClaims(1, "type1");
        //        debugString += $"{siteClaims?.Count()} type1 claims remains after delete one.\r\n";
        //    }

        //    ViewData["Message"] = 
        //        $"Vep:Your application description page.\r\n{debugString}";

        //    return View();
        //}

        //public IActionResult Contact()
        //{
        //    ViewData["Message"] = "Vep:Your contact page.";

        //    return View();
        //}

        [ResponseCache(CacheProfileName = "Never")]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }

        private async Task _SetViewag()
        {
            IEnumerable<Models.Post> latest = null;
            DateTime dtFilter = DateTime.Now;
//#if DEBUG
//            dtFilter = new DateTime(2016, 01, 01);
//#endif
            // Page metas...
            if (AppContext.Region == null)
            {
                ViewBag.MetaTitle = "VIE ET PARTAGE";
                ViewBag.MetaDescription = AppContext.Module.DefaultDescription;
                ViewBag.MetaKeywords = $"{AppContext.Module.DefaultPageKeywords}, {AppContext?.Site?.GetRegionsAsString().ToLower()}";
                ViewBag.MetaRobotsTerms = "index, follow";
                //ViewBag.HideMenu = true;
                ViewBag.HideNavPath = true;
                ViewBag.ForAllRegion = true;
            }
            else
            {
                ViewBag.MetaTitle = $"{AppContext?.Region?.StringValue} - VIE ET PARTAGE";
                ViewBag.MetaDescription = $"Vie et partage {AppContext?.Region?.StringValue} - {AppContext.Module.DefaultDescription}";
                ViewBag.MetaKeywords = $"{AppContext?.Region?.StringValue?.ToLower()}, {AppContext.Module.DefaultPageKeywords}";
                ViewBag.MetaRobotsTerms = "index, follow";
                ViewBag.HideNavPath = true;
                ViewBag.ForAllRegion = false;
            }
            // Post provider...
            PostProvider provider = new PostProvider(AppContext);
            // Post data...
            ViewBag.Retaites = await provider?.Get(new Dictionary<string, object>
            {
                { QueryFilter.Categorie, new List<int> { 14/*Retraite*/ } },
                { QueryFilter.EndDate, dtFilter }
            }, 0, 6);
            ViewBag.Events = await provider?.Get(new Dictionary<string, object>
            {
                { QueryFilter.Categorie, new List<int> { 13/*Calendrier*/ } },
                { QueryFilter.EndDate, dtFilter }
            }, 0, 6);
            ViewBag.Announcements = latest = await provider?.Get(new Dictionary<string, object>
            {
                { QueryFilter.Categorie, new List<int> { 15/*Annonces*/ } },
            }, 0, 20);
            //int nbLatest = ((latest?.Count() ?? 0) == 0) ? 11 : 6;
            ViewBag.Latests = await provider?.Get(new Dictionary<string, object>
            {
                { QueryFilter.ExcludePostsEvent, true },
                { $"{QueryFilter.Categorie}!", new List<int> { 13/*Retraite*/, 14/*Calendrier*/, 15/*Annonces*/ } },
            }, 0, 6/*nbLatest*/);
        }
    }
}
