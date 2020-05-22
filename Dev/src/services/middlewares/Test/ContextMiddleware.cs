using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// </summary>
    public class ContextMiddleware
    {
        private readonly RequestDelegate _next;

        public ContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, Services.WcmsAppContext appContext)
        {
            if (appContext == null)
            {
                throw new InvalidOperationException("Invalid context");
            }
            //appContext.Host = context.Request.Host;
            
            /*var appContext = context.RequestServices.GetService<WcmsAppContext>();
            if (appContext.RequestId != null)
            {
                throw new InvalidOperationException("RequestId should be null here");
            }
            var requestId = context.Request.Headers["RequestId"];
            appContext.RequestId = requestId;*/

            await _next(context);
        }
    }
}
