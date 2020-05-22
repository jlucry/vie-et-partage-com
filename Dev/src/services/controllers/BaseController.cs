using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;

namespace Services
{
    /// <summary>
    /// The base controller.
    /// This project can output the Class library as a NuGet Package.
    /// To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Logger.
        /// </summary>
        protected ILogger _Log { get; private set; }

        /// <summary>
        /// Base controller constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public BaseController(Services.WcmsAppContext appContext)
        {
            // Save the application context...
            AppContext = appContext;
            // Trace...
            _Log = AppContext?.LoggerFactory?.CreateLogger(this.GetType().Name);
            _Log?.LogDebug("BaseController: {0}", this.GetType().Name);
        }

        /// <summary>
        /// The applSetHourExpirationication context.
        /// </summary>
        public Services.WcmsAppContext AppContext { get; private set; }

        /// <summary>
        /// Set page expiration on hours.
        /// </summary>
        protected void UpdateExpirationToNextHour(int hours = 1)
        {
            HttpContext.UpdateExpirationToNextHour(hours);
        }

        /// <summary>
        /// Set page expiration on days.
        /// </summary>
        protected void UpdateExpirationToNextDay(int days = 1)
        {
            HttpContext.UpdateExpirationToNextDay(days);
        }
    }
}
