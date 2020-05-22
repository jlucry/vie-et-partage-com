using Contracts;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Services
{
    [ResponseCache(CacheProfileName = "Default")]
    public class PostController : BaseController
    {
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// The Base post controller constructor.
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="emailSender"></param>
        public PostController(Services.WcmsAppContext appContext, IEmailSender emailSender)
            : base(appContext)
        {
            _emailSender = emailSender;
        }

        /// <summary>
        /// List of post.
        /// </summary>
        /// <returns></returns>
        // GET: /<controller>/
        public async Task<IActionResult> Index(int skip, int take)
        {
            // Set page expiration...
            UpdateExpirationToNextDay();

            // Page metas...
            List<int> catList = _SetViewBagAndGetCats();
            bool excludeEvent = (AppContext?.Page?.GetPageFiltering() == PageFiltering.ExcludeEvent);
            // Post provider...
            PostProvider provider = new PostProvider(AppContext);
            ViewBag.pageMax = await provider?.Count(new Dictionary<string, object>
            {
                { QueryFilter.Categorie, catList },
                { QueryFilter.ExcludePostsEvent, excludeEvent },
            });
            ViewBag.Posts = await provider?.Get(new Dictionary<string, object>
            {
                { QueryFilter.Categorie, catList },
                { QueryFilter.ExcludePostsEvent, excludeEvent },
            }, skip, take);
            ViewBag.pageSkip = skip;
            ViewBag.pageTake = 20;

            //ViewData["Message"] = $"Posts: PageId={this.RouteData.Values[CRoute.PageIdTagName]}";
            return View();
        }

        /// <summary>
        /// Calendar.
        /// </summary>
        /// <returns></returns>
        // GET: /<controller>/
        public async Task<IActionResult> Calendar(int skip, int take)
        {
            // Set page expiration...
            UpdateExpirationToNextDay();

            DateTime dtFilter = DateTime.Now;
//#if DEBUG
//            dtFilter = new DateTime(2016, 01, 01);
//#endif
            // Post metas...
            List<int> catList = _SetViewBagAndGetCats();
            // Post provider...
            PostProvider provider = new PostProvider(AppContext);
            ViewBag.pageMax = await provider?.Count(new Dictionary<string, object>
            {
                { QueryFilter.Categorie, catList },
                { QueryFilter.EndDate, dtFilter }
            });
            ViewBag.Posts = await provider?.Get(new Dictionary<string, object>
            {
                { QueryFilter.Categorie, catList },
                { QueryFilter.EndDate, dtFilter }
            }, skip, take);
            ViewBag.pageSkip = skip;
            ViewBag.pageTake = 20;
            ViewBag.calendar = true;

            //ViewData["Message"] = $"Post: PageId={this.RouteData.Values[CRoute.PageIdTagName]}, PostId={this.RouteData.Values[CRoute.PostIdTagName]}";
            return View("Index");
        }

        /// <summary>
        /// A Post.
        /// </summary>
        /// <returns></returns>
        // GET: /<controller>/
        public IActionResult Post(bool registration)
        {
            // Set page expiration...
            UpdateExpirationToNextDay();
            // Set page metas...
            _SetPageMetas();
            //ViewData["Message"] = $"Post: PageId={this.RouteData.Values[CRoute.PageIdTagName]}, PostId={this.RouteData.Values[CRoute.PostIdTagName]}";
            return View();
        }

        /// <summary>
        /// A Post registration.
        /// </summary>
        /// <returns></returns>
        // GET: /<controller>/Register
        [ResponseCache(CacheProfileName = "Never")]
        public IActionResult Register()
        {
            // Set page metas...
            _SetPageMetas();
            ViewBag.MetaRobotsTerms = "noindex, nofollow";
            // Deserialize post registration data...
            List<JsonPostRegistrationField> jsonRegistrationFields = _GetRegistrationFiedl();
            return View(jsonRegistrationFields);
        }

        /// <summary>
        /// A Post registration.
        /// </summary>
        /// <param name="registrationFields"></param>
        /// <returns></returns>
        // POST: /<controller>/Register
        [HttpPost]
        [ResponseCache(CacheProfileName = "Never")]
        public async Task<IActionResult> Register(List<JsonPostRegistrationField> registrationFields)
        {
            bool isError = false;
            PostProvider provider = new PostProvider(AppContext, _emailSender);
            // Set page metas...
            _SetPageMetas();
            ViewBag.MetaRobotsTerms = "noindex, nofollow";
            // Checking for the model...
            if ((registrationFields?.Count ?? 0) == 0)
            {
                ViewBag.RegistrationMessage = "Une erreur s'est produite veuillez réessayer ultérieurement!";
                return Register();
            }
            //ModelState.IsValid
            // Checking model contains...
            foreach (JsonPostRegistrationField field in registrationFields)
            {
                // Init...
                field.IsError = false;
                field.IsError2 = false;
                // Checking...
                if (field.Type == 1) {
                    // Text area...
                    if (string.IsNullOrEmpty(field.Value) == true) {
                        isError = true;
                        field.IsError = true;
                    }
                }
                else if (field.Type == 2) {
                    // Oui\non...
                    if (string.IsNullOrEmpty(field.Value) == true) {
                        isError = true;
                        field.IsError = true;
                    }
                }
                else if (field.Type == 3) {
                    // Oui\non - Precisez...
                    if (string.IsNullOrEmpty(field.Value) == true) {
                        isError = true;
                        field.IsError = true;
                    }
                    else if (field.Value == "Oui"
                        && string.IsNullOrEmpty(field.Value2) == true) {
                        isError = true;
                        field.IsError2 = true;
                    }
                }
                if (field.Type == 4 || field.Type == 5) {
                    // Choix 1...
                    if (string.IsNullOrEmpty(field.Value) == true) {
                        isError = true;
                        field.IsError = true;
                    }
                }
                if (field.Type == 5) {
                    // Choix 2...
                    if (string.IsNullOrEmpty(field.Value2) == true) {
                        isError = true;
                        field.IsError2 = true;
                    }
                }
            }
            // Process the registration...
            if (isError == false
                && string.IsNullOrEmpty(ViewBag.RegistrationResult = await (provider?.Registration(AppContext?.Post, registrationFields) ?? null)) == false)
            {
                // Registration succedded...
                return View(null);
            }
            // Provide the error...
            if (isError == true)
            {
                ViewBag.RegistrationMessage = "Veuillez remplir tous les champs obligatoires!";
            }
            else if (string.IsNullOrEmpty(ViewBag.RegistrationResult) == true)
            {
                ViewBag.RegistrationMessage = "Une erreur s'est produite veuillez réessayer ultérieurement!";
            }
            // Update the registration data...
            List<JsonPostRegistrationField> dbRegistrationFields = _GetRegistrationFiedl();
            for (int i = 0; i < dbRegistrationFields?.Count; i++)
            {
               try
                {
                    dbRegistrationFields[i].Value = registrationFields[i].Value;
                    dbRegistrationFields[i].Value2 = registrationFields[i].Value2;
                    dbRegistrationFields[i].IsError = registrationFields[i].IsError;
                    dbRegistrationFields[i].IsError2 = registrationFields[i].IsError2;
                }
                catch { }
            }
            return View(dbRegistrationFields);
        }

        /// <summary>
        /// Set page metas.
        /// </summary>
        private void _SetPageMetas()
        {
            // Set page metas...
            ViewBag.Title = AppContext?.Post?.Title;
            ViewBag.MetaTitle = AppContext.Site.GetMetaTitle(AppContext, AppContext?.Post?.Title);
            ViewBag.MetaDescription = AppContext.Site.GetMetaDescription(AppContext, AppContext?.Post?.Title);
            ViewBag.MetaKeywords = AppContext.Site.GetMetaKeywords(AppContext, AppContext?.Post?.Title);
            ViewBag.MetaRobotsTerms = "index, follow";
            if (AppContext?.Post != null)
            {
                ViewBag.Title = AppContext?.Post?.Title;
            }
            ViewBag.HideRegionMenu = true;
        }

        /// <summary>
        /// Set View bag and get catergories.
        /// </summary>
        /// <returns></returns>
        private List<int> _SetViewBagAndGetCats()
        {
            List<int> catList;
            if (AppContext?.Cat != null)
            {
                ViewBag.Title = AppContext.Cat.StringValue;
                ViewBag.MetaTitle = AppContext.Site.GetMetaTitle(AppContext, AppContext.Cat.StringValue);
                ViewBag.MetaDescription = AppContext.Site.GetMetaDescription(AppContext, AppContext.Cat.StringValue);
                ViewBag.MetaKeywords = AppContext.Site.GetMetaKeywords(AppContext, AppContext.Cat.StringValue);
                if ((catList = AppContext?.Site?.GetCategoriesAsIdList(AppContext.RouteCatId, true)) == null)
                {
                    catList = new List<int>();
                }
                catList.Add(AppContext.RouteCatId);
            }
            else
            {
                ViewBag.Title = AppContext.Page.Title;
                ViewBag.MetaTitle = AppContext.Site.GetMetaTitle(AppContext, AppContext.Page.Title);
                ViewBag.MetaDescription = AppContext.Site.GetMetaDescription(AppContext, AppContext.Page.Title);
                ViewBag.MetaKeywords = AppContext.Site.GetMetaKeywords(AppContext, AppContext.Page.Title);
                catList = AppContext?.Page?.GetCategoriesAndChilsAsIdList(AppContext);
            }
            ViewBag.MetaRobotsTerms = "index, follow";
            ViewBag.ForAllRegion = (AppContext.Region == null) ? true : false;
            return catList;
        }

        /// <summary>
        /// Get registration fields.
        /// </summary>
        /// <returns></returns>
        private List<JsonPostRegistrationField> _GetRegistrationFiedl()
        {
            IEnumerable<PostRegistrationField> registrationFields = AppContext?.Post?.GetRegistrationField();
            List<JsonPostRegistrationField> jsonRegistrationFields = new List<JsonPostRegistrationField>();
            foreach (PostRegistrationField fld in registrationFields)
            {
                jsonRegistrationFields.Add(new JsonPostRegistrationField(fld));
            }
            return jsonRegistrationFields;
        }
    }
}
