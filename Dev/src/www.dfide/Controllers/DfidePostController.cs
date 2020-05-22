using Contracts;
using Microsoft.AspNetCore.Mvc;
using Services;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Wcms.Controllers
{
    [ResponseCache(CacheProfileName = "Never")]
    public class DfidePostController : PostController
    {
        /// <summary>
        /// The post controller constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public DfidePostController(WcmsAppContext appContext, IEmailSender emailSender)
            : base(appContext, emailSender)
        {
        }
    }
}
