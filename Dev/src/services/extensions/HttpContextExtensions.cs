// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    /// <summary>
    /// Page extensions.
    /// </summary>
    public static class contextExtensions
    {
        /// <summary>
        /// Set page expiration on next N hours.
        /// </summary>
        public static void UpdateExpirationToNextHour(this HttpContext context, int hours = 1)
        {
            int diff = ((60 - DateTime.UtcNow.Minute) + ((hours <= 1) ? 0 : (hours * 60))) * 60;
            DateTime exp = DateTime.UtcNow.AddSeconds(diff);
            DateTime nextHour = new DateTime(exp.Year, exp.Month, exp.Day, exp.Hour, exp.Minute, 0);

            context._UpdateExpirationToNextDate(diff, nextHour, $"{hours}hours");
        }

        /// <summary>
        /// Set page expiration on days.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="days"></param>
        public static void UpdateExpirationToNextDay(this HttpContext context, int days = 1)
        {
            DateTime now = DateTime.UtcNow;
            DateTime nowPlusOne = now.AddDays(days);
            DateTime nextMidNight = new DateTime(nowPlusOne.Year, nowPlusOne.Month, nowPlusOne.Day, 0, 0, 0);
            int diff = (int)nextMidNight.Subtract(now).TotalSeconds;

            context._UpdateExpirationToNextDate(diff, nextMidNight, $"{days}days");
        }

        /// <summary>
        /// Set page expiration on hours.
        /// </summary>
        private static void _UpdateExpirationToNextDate(this HttpContext context, int diff, DateTime nextDate, string unit)
        {
            // Clear all headers...
            context.Response.Headers.Remove("to-next-update");
            context.Response.Headers.Remove(HeaderNames.CacheControl);
            context.Response.Headers.Remove(HeaderNames.Expires);
            //context.Response.Headers.Remove(HeaderNames.Vary);

            if (context.User.Identity.IsAuthenticated == false)
            {
                // Set expiration headers...
                context.Response.Headers.Add("to-next-update", new[] { $"{unit}:{diff}scds" });
                context.Response.Headers.Add(HeaderNames.CacheControl, new[] { $"public,max-age={diff}" });
                context.Response.Headers.Add(HeaderNames.Expires, new[] { nextDate.ToString("R") });
            }
            else
            {
                // Set expiration headers...
                context.Response.Headers.Add(HeaderNames.CacheControl, new[] { $"no-store,no-cache" });
            }
            //context.Response.Headers.Add(HeaderNames.Vary, new string[] { "Host" });
        }
    }
}
