using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Home controller.
    /// </summary>
    [Authorize]
    public class FileController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// The File controller constructor.
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        /// <param name="appContext"></param>
        public FileController(IHostingEnvironment hostingEnvironment, Services.WcmsAppContext appContext)
            : base(appContext)
        {
            //Console.WriteLine($"--- Services.FileController...");
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Upload files.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// https://github.com/aspnet/Announcements/issues/267
        [RequestSizeLimit(650_000_000)]
        public async Task<IActionResult> Upload(int id, string name, int type)
        {
            try
            {
                string url = null;
                PostProvider provider = new PostProvider(AppContext, null);
                if (provider == null)
                {
                    return Content("KO:Invalid post provider");
                }
                else if ((url = await provider.AddFile(id, name, type, Request.Body)) == null)
                {
                    return Content("KO:" + provider.LastError);
                }
                return Content(url);
            }
            catch(Exception e)
            {
                return Content("KO:" + e.Message);
            }
        }
    }
}
