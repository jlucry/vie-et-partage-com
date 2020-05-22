using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Performance middleware.
    /// </summary>
    public class PerformanceMiddleware
    {
        // Next middleware...
        private readonly RequestDelegate _next;

        /// <summary>
        /// Performance middleware constructor.
        /// </summary>
        /// <param name="next"></param>
        public PerformanceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Execute the performance middleware logic.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="appContext"></param>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context,
            Services.WcmsAppContext appContext,
            ILoggerFactory loggerFactory)
        {
            // Special case...
            if (context?.Request?.Path.Value == "/flush")
            {
                // Flush the performance file...
                appContext?.FlushPerfLog();
                await context.Response.WriteAsync("Ok\r\n");
                return;
            }

            // Start performance logging...
            appContext?.StartPerfLog();

            ILogger log = loggerFactory?.CreateLogger("Performance"/*typeof(PerformanceMiddleware).FullName*/);
            log?.LogInformation(">>> \"{0}\"...",
                context?.Request?.Path.Value?.ToString());
            {
                // Call next middleware...
                await _next(context);
            }
            // Trace process duration...
            log?.LogInformation("<<< \"{0}\" processed in {1} milliseconds.",
                context?.Request?.Path.Value?.ToString(),
                appContext?.ElapsedMilliseconds());

            // Write to the perfomance file...
            appContext?.WritePerfLog(context?.Request?.Path.Value);
        }
    }
}
