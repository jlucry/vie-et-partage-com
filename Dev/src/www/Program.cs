using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;

namespace www
{
    /// <summary>
    /// Entry point class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Since ASP.NET Core apps are just console apps,
        /// you must define an entry point for your application in Program.
        /// Main that sets up a web host, then tells it to start listening
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
            /*
            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                //.UseUrls("http://*:80/", "https://*:443/")
                .UseUrls("http://*:5963/")
                .UseIISIntegration()
                .UseKestrel()
                .Build();

            host.Run();
            */
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                //.UseUrls("http://*:80/", "https://*:443/")
                .UseUrls("http://*:5963/")
                .UseIISIntegration()
                .UseKestrel()
                .Build();
    }
}
