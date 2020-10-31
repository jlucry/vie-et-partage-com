#define SERILOG_ON
#undef SERILOG_SED
#define SERILOG_FILE

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Models;
using Services;
using www.dfide;
using www.vep;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using Contracts;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Serialization;
//using MySQL.Data.EntityFrameworkCore.Extensions;
using contracts;
using Serilog;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using AuthorizationMiddleware = Microsoft.AspNetCore.Authorization.AuthorizationMiddleware;

namespace www
{
    //1. Mediatheque.
    //2. Annuaire comunautaire -> photos, num, email.
    //3. Skype.
    //4. Transmette les infos par email ou sms(pour communauté) - rapelle des dates.
    //5. mi nov 2016.
    //6. composition de la cellule.
    //
    // http://forums.mysql.com/list.php?38
    // En cours: http://forums.mysql.com/read.php?38,649736,649928#msg-649928
    // 
    // Migration:
    //  Add-Migration M1 -Context AppDbContext
    //
    // Mailing:
    // https://sendgrid.com/pricing/
    // https://fr.sendinblue.com/pricing/#tab-plan-emailplan
    // https://www.mailjet.com/pricing_v3
    // TODO: infinite loop on http://vieetpartage.com/admin/#/index

    /// <summary>
    /// http://tlevesque.developpez.com/tutoriels/csharp/les-nouveautes-de-csharp-6/
    /// 
    /// http://stackoverflow.com/questions/3540562/return-view-from-different-area
    /// return View("~/Views/YourArea/YourController/YourView.aspx");
    /// 
    /// TODO: Revoir la gestion des logs.
    /// 
    /// TODO: Revoir les meta de page et post.
    /// 
    /// TODO: Lier un utilisateur à son site d'enregistrement.
    ///         * Ajout d'un claim Site
    ///         * Utilisation d'un middleware pour vérifier le site (en derniere positon dans le pipe).
    ///         * OAuth:    https://www.simple-talk.com/dotnet/.net-framework/creating-custom-oauth-middleware-for-mvc-5/
    ///                     https://github.com/aspnet/Security/blob/1a59b385a012ac0873fb599fbe6c648b10088fd5/samples/SocialSample/Startup.cs
    ///                     [RFC] http://tools.ietf.org/html/rfc6749
    ///                     * http://www.asp.net/aspnet/overview/owin-and-katana/owin-oauth-20-authorization-server
    ///                     * https://www.simple-talk.com/dotnet/.net-framework/creating-custom-oauth-middleware-for-mvc-5/
    ///                     * http://shazwazza.com/post/configuring-aspnet-identity-oauth-login-providers-for-multi-tenancy/
    ///                     doc mvc6:   http://docs.asp.net/en/latest/security/sociallogins.html
    ///                                 (secret conf) https://github.com/aspnet/Home/wiki/DNX-Secret-Configuration
    /// 
    /// TODO: Gestion des autorizations.
    ///         * http://leastprivilege.com/2015/10/12/the-state-of-security-in-asp-net-5-and-mvc-6-authorization/
    ///         * http://odetocode.com/blogs/scott/archive/2015/10/06/authorization-policies-and-middleware-in-asp-net-5.aspx
    ///         * Authorization Policies and Middleware in ASP.NET 5: http://odetocode.com/blogs/scott/archive/2015/10/06/authorization-policies-and-middleware-in-asp-net-5.aspx
    /// 
    /// Architecture:
    ///     * Module:
    ///         * Avec area:
    ///         * Sans area: 
    ///             * Layout        : "_{module.name}Layout"
    ///             * Controller    : "{module.name}{controller name}"
    ///             * View          : "{module.name}{controller name}\{action name}"
    ///             * ViewComponent : Utiliser le  chemin réelle dans le module.
    ///     * Navigation:
    ///         * Le site contient des pages.
    ///         * Une page est definie par un controlleur, une action et des filtres qui 
    ///           permettent de retrouver un ou plusieurs post.    
    ///     * Route par defaut:
    ///         * Home:   Accueil du site.
    ///         * Région: Accueil d'une région du site.
    ///         * Page:   Une page du site.
    ///                   L'id dans la route permet re retrouver controlleur et action à utliser.
    ///         * Post:   Un post
    ///     * Controller et view fonction du site.
    /// 
    /// OWIN:
    ///     * Getting Your Client ID for Web Authentication: https://msdn.microsoft.com/en-us/library/bb676626.aspx
    ///     * http://benfoster.io/blog/oauth-providers (google, live, facebook, ect...).
    ///     Gestion des apps: 
    ///         Windows : https://account.live.com/developers/applications/index (david.fidelin@live.com)
    ///         Google  : https://code.google.com/apis/console (david.fidelin@dfide.net)
    ///                   https://developers.google.com/identity/protocols/OAuth2
    ///                   !!! Google+ Api need to be enabled
    ///                   Tuto: http://www.oauthforaspnet.com/providers/google/guides/aspnet-mvc5/
    /// 
    /// Port 80 used: 
    ///      ==> Microsoft HTTPAPI/2.0 use Port 80: http://www.ferolen.com/blog/microsoft-httpapi2-0-use-port-80-cannot-start-wamp-apache/
    ///         We must stop Web Deployment Agent Service (MsDepSvc) from 
    ///         Administrative Tools > Services area.
    ///         You can disable the service, to have port 80 ready when you boot your system. 
    ///      ==> Ou IIS
    ///  
    /// vNext CMS:
    /// https://github.com/OrchardCMS/Brochard
    /// https://github.com/joeaudette/cloudscribe
    /// 
    /// Mailing liste:
    ///     https://sendgrid.com/pricing
    /// 
    /// Link test:
    ///     http://vep.ddns.net/
    ///     http://dfide.ddns.net/
    /// 
    /// Migratation des données:
    ///     * Podcast: http://www.vieetpartage.com.preview08.oxito.com/podcast
    ///     * Lien vers des informations: Voir dfide.visualstudio.com\Wcms\business.tool\VepdbToMultiTenant.cs fonction __UpdateContainLinks
    ///     * podcast.xml
    ///     
    /// 
    /// 1. UnitTests (Edit): package.json
    /// 2. www (Edit): project.json, appsettings.json, Startup.cs, *.cshtml
    /// 3. www (Add) : hosting.json
    /// 4. www.dfide, www.vep (Edit): project.json, *.cshtml, areas\view and view moved to EmbeddedResources.
    /// 
    /// internationalization module for AngularJS:
    ///     https://github.com/angular/bower-angular-i18n
    /// </summary>
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            //TODO: Configuration: http://docs.asp.net/en/latest/fundamentals/configuration.html

            // Init performance log...
            //PerformanceProvider.Init(hostingEnvironment?.ContentRootPath);

            // Setup configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);

            if (env.IsDevelopment())
            {
                // This reads the configuration keys from the secret store.
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();

                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                //builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            //Assembly mscorlib = GetType().GetTypeInfo().Assembly;
            //foreach (Type type in mscorlib.GetTypes())
            //{
            //    Console.WriteLine(type.FullName);
            //}

            // Temp: Register module.
            // TODO: Load module assembly and discover modules...
            // TODO: var libraryManager = PlatformServices.Default.LibraryManager;
            // TODO: PlatformServices.Default.Application.ApplicationBasePath
            // TODO: string _testProjectPath = Path.GetDirectoryName(libraryManager.GetLibrary(projectName).Path);
            DfideModule.Register();
            VepModule.Register();

            // Init logs...
            if (Configuration["logMethod"] == "serilog")
            {
                // Official site: https://serilog.net/
                // Blog: https://nblumhardt.com/
                // Doc: https://github.com/serilog/serilog/wiki
                // https://github.com/serilog/serilog-settings-configuration/blob/dev/sample/Sample/appsettings.json
                // --
                // SeriLog doc specific to asp.net core:
                // https://github.com/serilog/serilog-extensions-logging-file
                // https://nblumhardt.com/2016/10/aspnet-core-file-logger/
                // --
                // project.json
                // "Serilog.Extensions.Logging.File": "1.0.0",
            }
            else if (Configuration["logMethod"] == "serilog_sed")
            {
                // Sed: log server: https://getseq.net/
                // Sed doc: http://docs.getseq.net/
                // Sed with seriLog: http://docs.getseq.net/docs/using-serilog
                // Local installation: http://localhost:5341/#/events
                // --
                // project.json
                // "Serilog.Extensions.Logging.File": "1.0.0",
                // "Serilog.Sinks.Seq": "3.0.1",
                Log.Logger = new LoggerConfiguration()
                    //.MinimumLevel.Verbose()
                    .MinimumLevel.Information()
                    .WriteTo.Seq("http://localhost:5341")
                    .CreateLogger();
            }
            else if (Configuration["logMethod"] == "serilog_console")
            {
                //    // project.json
                //    // "Serilog.Extensions.Logging": "1.2.0",
                //    // "Serilog.Sinks.Trace": "2.0.0",
                //    // "Serilog.Sinks.Literate": "2.0.0",
                //    // "Serilog.Sinks.RollingFile": "2.0.0",
                //    // "Serilog.Sinks.File": "3.0.0",
                //    Log.Logger = new LoggerConfiguration()
                //        .MinimumLevel.Verbose()
                //        .Enrich.FromLogContext()
                //        //.WriteTo.Trace()
                //        //.WriteTo.Async(w => w.RollingFile(Path.Combine(env.ContentRootPath, "logs/{Date}.txt")))
                //        .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "logs/{Date}.txt"))
                //        .WriteTo.LiterateConsole(restrictedToMinimumLevel: LogEventLevel.Information)
                //        .CreateLogger();
            }
            else if (Configuration["logMethod"] == "console")
            {
            }
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                // Init logging...
                if (Configuration["logMethod"] == "serilog")
                {
                    //loggerFactory.AddFile(Path.Combine(env.ContentRootPath, "logs/{Date}.txt"));
                    loggingBuilder.AddFile(Configuration.GetSection("Logging"));
                }
                else if (Configuration["logMethod"] == "serilog_sed")
                {
                    loggingBuilder.AddSerilog();
                }
                else if (Configuration["logMethod"] == "serilog_console")
                {
                    loggingBuilder.AddSerilog();
                    // Ensure any buffered events are sent at shutdown
                    //appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
                }
                else if (Configuration["logMethod"] == "console")
                {
                    // Init logging...
                    //loggerFactory.MinimumLevel = (hostingEnvironment.IsDevelopment())
                    //    ? LogLevel.Verbose
                    //    : LogLevel.Information;
                    //loggerFactory.AddConsole(loggerFactory.MinimumLevel);
                    //loggerFactory.AddDebug(loggerFactory.MinimumLevel);
                    loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
                    //"Microsoft.Extensions.Logging.TraceSource": "1.0.0",
                    //"Microsoft.Extensions.Logging.AzureAppServices": "1.0.0-preview1-final",
                    //var testSwitch = new SourceSwitch("sourceSwitch", "Logging Sample");
                    //testSwitch.Level = SourceLevels.Verbose;
                    //loggerFactory.AddTraceSource(testSwitch,
                    //    new TextWriterTraceListener(writer: Console.Out));
                    //loggerFactory.AddAzureWebAppDiagnostics();
                    //System.Diagnostics.Trace.TraceError("ERRRRRRRRRRRRR");
#if DEBUG
                    loggingBuilder.AddDebug();
#endif
                }
            });

            // Add framework services.
            //services.AddApplicationInsightsTelemetry(Configuration);

            if (Global.UseMySql == false)
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(
                        Configuration.GetConnectionString("defaultConnection"),
                        b => { b.MigrationsAssembly("www"); b.EnableRetryOnFailure(); }));
            }
            else
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseMySql(
                        Configuration.GetConnectionString("defaultMysqlConnection"),
                        b => b.MigrationsAssembly("www")));
            }

            // Add Identity services to the services container.
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    //options.User.AllowedUserNameCharacters = null;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            // Add cookie-based authentication to the request pipeline.
            services.ConfigureApplicationCookie(o =>
            {
                o.Cookie.HttpOnly = true;
                //o.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
#if !DEBUG
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
#else
                o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
#endif
                /*HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always,
                //OnAppendCookie = context =>
                //{
                //    context.CookieOptions.Expires = DateTimeOffset.Now.AddMinutes(10);
                //}*/
            });

            // Add response caching service...
            services.AddResponseCaching(options =>
            {
                options.UseCaseSensitivePaths = false;
                //options.MaximumBodySize = 64 * 1024 * 1024;
            });

            // Add MVC services to the services container.
            services
                .AddMvc(options =>
                {
                    options.EnableEndpointRouting = false;
                    options.CacheProfiles.Add("Default",
                        new CacheProfile()
                        {
                            VaryByHeader = "Host",
                            VaryByQueryKeys = new string[] { "skip" },
                            Location = ResponseCacheLocation.Any,
                            Duration = 24 * 60 * 60 // One day
                        });
                    options.CacheProfiles.Add("Never",
                        new CacheProfile()
                        {
                            VaryByHeader = "Host",
                            Location = ResponseCacheLocation.None,
                            NoStore = true
                        });
                })
                // De-Camelizing JSON in ASP.NET Core...
                .AddNewtonsoftJson(opt => {
                    var resolver = opt.SerializerSettings.ContractResolver;
                    if (resolver != null)
                    {
                        var res = resolver as DefaultContractResolver;
                        res.NamingStrategy = null;  // <<!-- this removes the camelcasing
                    }
                })
                // Is Modular Web Application in vNext Possible?
                // https://github.com/aspnet/Mvc/issues/4572
                // https://github.com/thiennn/trymodular/tree/master/Modular/src
                // TODO: Application Parts in ASP.NET Core: https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts
                .ConfigureApplicationPartManager(manager => manager.ApplicationParts.Clear())
                .AddApplicationPart(typeof(Startup).GetTypeInfo().Assembly)
                .AddApplicationPart(typeof(SiteApiController).GetTypeInfo().Assembly)
                .AddApplicationPart(typeof(DfideSiteSettings).GetTypeInfo().Assembly)
                .AddApplicationPart(typeof(VepSiteSettings).GetTypeInfo().Assembly);

            // Configure Razor view engine...
            services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
            {
                //options.FileProviders.Clear();
                options.FileProviders.Add(new CompositeFileProvider(
                    new EmbeddedFileProvider(
                        typeof(DfideSiteSettings).GetTypeInfo().Assembly,
                        baseNamespace: "www.dfide.EmbeddedResources"
                    )));
                options.FileProviders.Add(new EmbeddedFileProvider(
                        typeof(VepSiteSettings).GetTypeInfo().Assembly,
                        baseNamespace: "www.vep.EmbeddedResources"
                    ));
            });

            // Register application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            //services.AddTransient<ISiteSettings, SiteSettings>();

            // Inject context service.
            services.AddScoped<PerformanceProvider, PerformanceProvider>();
            services.AddScoped<WcmsAppContext, WcmsAppContext>();

            // Inject the view engine.
            //services.AddTransient/*AddScoped*//*AddSingleton*/<IRazorViewEngine, ViewEngine>();
            services.Configure<RazorViewEngineOptions>(options =>
            {
                var expander = new ViewLocationExpander(null);
                options.ViewLocationExpanders.Add(expander);
            });

            // Inject configuration service.
            services.AddSingleton(_ => Configuration);

            // only allow authenticated users
            /*var defaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            services.AddMvc(setup =>
            {
                setup.Filters.Add(new AuthorizeFilter(defaultPolicy));
            });*/

            // Register authorization policy...
            services.AddAuthorization(options =>
            {
                // inline policies
                options.AddPolicy(ClaimValueRole.Reader, policy =>
                {
                    policy.RequireClaim(UserClaimType.Role, ClaimValueRole.Reader);
                    //policy.RequireRole(Role.Reader);
                });
                options.AddPolicy(ClaimValueRole.Contributor, policy =>
                {
                    policy.RequireClaim(UserClaimType.Role, ClaimValueRole.Contributor);
                });
                options.AddPolicy(ClaimValueRole.Publicator, policy =>
                {
                    policy.RequireClaim(UserClaimType.Role, ClaimValueRole.Publicator);
                });
                options.AddPolicy(ClaimValueRole.Administrator, policy =>
                {
                    policy.RequireClaim(UserClaimType.Role, ClaimValueRole.Administrator);
                });
            });

            // Register resource authorization handlers...
            services.AddTransient<IAuthorizationHandler, SiteAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, PageAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, PostAuthorizationHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime/*, AppDbContext context*/)
        {
            // Enable the response cache middleware...
            app.UseResponseCaching();

            //http://tostring.it/2016/01/12/Using-Kestrel-with-ASPNET-5/
            //var certFile = hostingEnvironment.ContentRootPath + "\\democertificate.pfx";
            //var signingCertificate = new X509Certificate2(certFile, "democertificate.io");
            //app.UseKestrelHttps(signingCertificate);

            // Configure the HTTP request pipeline.
            //app.UseMiddleware<PerformanceMiddleware>();

            // Add the following to the request pipeline only in development environment.
#if DEBUG
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
#endif
            {
                // Add Error handling middleware which catches all application specific errors and
                // sends the request to the following path or controller action.
                app.UseExceptionHandler("/Error/Index");
                //app.UseStatusCodePages("text/plain", "Response, status code: {0}");

                // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859
                //try
                //{
                //    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                //        .CreateScope())
                //    {
                //        //serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                //        //     .Database.Migrate();
                //    }
                //}
                //catch { }
            }
            
            // Add the platform handler to the request pipeline.
            //app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            // Add static files to the request pipeline.
            //app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline.
            app.UseAuthentication();

#if DEPRECATED
            // Add and configure the options for authentication middleware to the request pipeline.
            // You can add options for middleware as shown below.
            // For more information see http://go.microsoft.com/fwlink/?LinkID=532715
            //app.UseFacebookAuthentication(options =>
            //{
            //    options.AppId = Configuration["Authentication:Facebook:AppId"];
            //    options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            //});
            //app.UseGoogleAuthentication(options =>
            //{
            //    options.ClientId = Configuration["Authentication:Google:ClientId"];
            //    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
            //});
            //app.UseMicrosoftAccountAuthentication(options =>
            //{
            //    options.ClientId = Configuration["Authentication:MicrosoftAccount:ClientId"];
            //    options.ClientSecret = Configuration["Authentication:MicrosoftAccount:ClientSecret"];
            //});
            //app.UseTwitterAuthentication(options =>
            //{
            //    options.ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"];
            //    options.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
            //});
            // Register site authentication middleware...
            foreach (KeyValuePair<string, IModule> mod in Factory.GetModules())
            {
                foreach (ModuleAuthentication modAuth in mod.Value.Authentications)
                {
                    if (modAuth.Type == AuthenticationType.ExtLive)
                    {
                        /* https://account.live.com/developers/applications
                        The MicrosoftAccount service has restrictions that prevent the use of http://localhost:54540/ for test applications.
                        As such, here is how to change this sample to uses http://mssecsample.localhost.this:54540/ instead.
                        Edit the Project.json file and replace http://localhost:54540/ with http://mssecsample.localhost.this:54540/.
                        From an admin command console first enter:
                         notepad C:\Windows\System32\drivers\etc\hosts
                        and add this to the file, save, and exit (and reboot?):
                         127.0.0.1 MsSecSample.localhost.this
                        Then you can choose to run the app as admin (see below) or add the following ACL as admin:
                         netsh http add urlacl url=http://mssecsample.localhost.this:54540/ user=[domain\user]
                        */
                        //"Microsoft.AspNet.Authentication.Facebook": "1.0.0-rc1-final",
                        //"Microsoft.AspNet.Authentication.Google": "1.0.0-rc1-final",
                        //"Microsoft.AspNet.Authentication.MicrosoftAccount": "1.0.0-rc1-final",
                        //"Microsoft.AspNet.Authentication.Twitter": "1.0.0-rc1-final",
                        /*
                        app.UseMicrosoftAccountAuthentication(options =>
                        {
                            options.AuthenticationScheme = $"{mod.Value.Name.ToLower()}Live";
                            options.ClientId = modAuth.ClientId;
                            options.ClientSecret = modAuth.ClientSecret;
                            options.CallbackPath = new PathString($"/{mod.Value.Name.ToLower()}-signin-live");
                            options.SignInScheme = modAuth.SignInAsAuthenticationType;
                        });*/
                    }
                    else if (modAuth.Type == AuthenticationType.ExtGoogle)
                    {/*
                        app.UseGoogleAuthentication(options =>
                        {
                            options.AuthenticationScheme = $"{mod.Value.Name.ToLower()}Google";
                            options.ClientId = modAuth.ClientId;
                            options.ClientSecret = modAuth.ClientSecret;
                            options.CallbackPath = new PathString($"/{mod.Value.Name.ToLower()}-signin-google");
                            options.SignInScheme = modAuth.SignInAsAuthenticationType;
                        });*/
                    }
                }
            }
#endif

            // Initializes the Authorization middleware...
            app.UseMiddleware<AuthorizationMiddleware>();
            // Work arround for error: 
            // invalidOperationException: 'VaryByQueryKeys' requires the response cache middleware.
            // when cache is disabled on client
            app.Use((context, next) =>
            {
                if (context.Features.Get<IResponseCachingFeature>() == null)
                {
                    context.Features.Set<IResponseCachingFeature>(new FakeResponseCachingFeature());
                }
                return next();
            });

            // TODO: Check perf degradation when the StaticFiles middleware is placed after all authentication middleware.
            // Add static files to the request pipeline.
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    context.Context.UpdateExpirationToNextDay(1);
                    //// Cache static file for 1 year
                    ////if (!string.IsNullOrEmpty(context.Context.Request.Query["v"]))
                    //{
                    //    context.Context.Response.Headers.Add(HeaderNames.CacheControl, new[] { "private,max-age=86400" }); //public,max-age=31536000
                    //    context.Context.Response.Headers.Add(HeaderNames.Expires, new[] { DateTime.UtcNow.AddDays(1).ToString("R") }); // DateTime.UtcNow.AddYears(1).ToString("R") - Format RFC1123
                    //}
                }
            });

            // Initializes the context with a middleware (not used - example purpose)...
            //app.UseMiddleware<ContextMiddleware>();

            // To enabled CORS:
#if DEBUG
            app.UseCors(builder =>
              builder.WithOrigins("http://locahost:4200")
              .AllowAnyHeader());
#endif

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.DefaultHandler = new SiteRoute(routes.DefaultHandler, loggerFactory);
                // Home route...
                routes.MapRoute(
                    name: "home",
                    template: "",
                    defaults: new { controller = "Home", action = "Index" });
                routes.MapRoute(
                    name: "region",
                    template: $"{{{CRoute.RegionTagName}}}",
                    defaults: new { controller = "Home", action = "Region" });
                // Pages route...
                routes.MapRoute(
                    name: "page",
                    template: $"{{{CRoute.PageTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}",
                    defaults: new { controller = "Page", action = "Index" });
                routes.MapRoute(
                    name: "pageRegion",
                    template: $"{{{CRoute.RegionTagName}}}/{{{CRoute.PageTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}",
                    defaults: new { controller = "Page", action = "Index" });
                // Post liste route...
                routes.MapRoute(
                    name: "postsByCat",
                    template: $"{{{CRoute.CatTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/ct{{{CRoute.CatIdTagName}}}",
                    defaults: new { controller = "Post", action = "Index" });
                routes.MapRoute(
                    name: "postsByCatRegion",
                    template: $"{{{CRoute.RegionTagName}}}/{{{CRoute.CatTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/ct{{{CRoute.CatIdTagName}}}",
                    defaults: new { controller = "Post", action = "Index" });
                routes.MapRoute(
                    name: "calendarByCat",
                    template: $"{{{CRoute.CatTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/ct{{{CRoute.CatIdTagName}}}/cldr",
                    defaults: new { controller = "Post", action = "Calendar" });
                routes.MapRoute(
                    name: "calendarByCatRegion",
                    template: $"{{{CRoute.RegionTagName}}}/{{{CRoute.CatTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/ct{{{CRoute.CatIdTagName}}}/cldr",
                    defaults: new { controller = "Post", action = "Calendar" });
                // Not needed, we'll use the page route which will provide the page id.
                // From this page id, we'll get controller and action.
                //routes.MapRoute(
                //    name: "posts",
                //    template: $"{{{CRoute.PageTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/pts{{{CRoute.PostIdTagName}}}",
                //    defaults: new { controller = "Post", action = "Index" });
                //routes.MapRoute(
                //    name: "postsRegion",
                //    template: $"{{{CRoute.RegionTagName}}}/{{{CRoute.PageTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/pts{{{CRoute.PostIdTagName}}}",
                //    defaults: new { controller = "Post", action = "Index" });
                // Post route...
                routes.MapRoute(
                    name: "post",
                    template: $"{{{CRoute.PostTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/pt{{{CRoute.PostIdTagName}}}",
                    defaults: new { controller = "Post", action = "Post" });
                routes.MapRoute(
                    name: "postRegion",
                    template: $"{{{CRoute.RegionTagName}}}/{{{CRoute.PostTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/pt{{{CRoute.PostIdTagName}}}",
                    defaults: new { controller = "Post", action = "Post" });
                routes.MapRoute(
                    name: "postFromCat",
                    template: $"{{{CRoute.PostTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/ct{{{CRoute.CatIdTagName}}}/pt{{{CRoute.PostIdTagName}}}",
                    defaults: new { controller = "Post", action = "Post" });
                routes.MapRoute(
                    name: "postFromCatRegion",
                    template: $"{{{CRoute.RegionTagName}}}/{{{CRoute.PostTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/ct{{{CRoute.CatIdTagName}}}/pt{{{CRoute.PostIdTagName}}}",
                    defaults: new { controller = "Post", action = "Post" });
                // Post registration route...
                routes.MapRoute(
                    name: "postReg",
                    template: $"{{{CRoute.PostTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/preg{{{CRoute.PostIdTagName}}}",
                    defaults: new { controller = "Post", action = "Register" });
                routes.MapRoute(
                    name: "postRegionReg",
                    template: $"{{{CRoute.RegionTagName}}}/{{{CRoute.PostTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/preg{{{CRoute.PostIdTagName}}}",
                    defaults: new { controller = "Post", action = "Register" });
                routes.MapRoute(
                    name: "postFromCatReg",
                    template: $"{{{CRoute.PostTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/ct{{{CRoute.CatIdTagName}}}/preg{{{CRoute.PostIdTagName}}}",
                    defaults: new { controller = "Post", action = "Register" });
                routes.MapRoute(
                    name: "postFromCatRegionReg",
                    template: $"{{{CRoute.RegionTagName}}}/{{{CRoute.PostTitleTagName}}}/pg{{{CRoute.PageIdTagName}}}/ct{{{CRoute.CatIdTagName}}}/preg{{{CRoute.PostIdTagName}}}",
                    defaults: new { controller = "Post", action = "Register" });
                // Default route...
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}"); //{area:exist}

                // Uncomment the following line to add a route for porting Web API 2 controllers.
                // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
            });

            // Ensure that the db is created.
            //context.Database.EnsureCreated();

            //app.Run(async context =>
            //{
            //    Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            //    await context.Response.WriteAsync("Hello, World, Again!");
            //});
        }
    }

    public class FakeResponseCachingFeature : IResponseCachingFeature
    {
        public string[] VaryByQueryKeys
        {
            get { return null; }
            set { }
        }
    }
}
