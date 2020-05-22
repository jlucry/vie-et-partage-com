using Contracts;
using Microsoft.AspNetCore.Mvc;
using Services;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace www.vep
{
    /// <summary>
    /// Post controller.
    /// </summary>
    [Area("Vep")]
    public class PostController : Services.PostController
    {
        /// <summary>
        /// The post controller constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public PostController(WcmsAppContext appContext, IEmailSender emailSender)
            : base(appContext, emailSender)
        {
            int i = 0;
        }
    }
}
