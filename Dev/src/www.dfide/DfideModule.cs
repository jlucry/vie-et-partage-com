using Contracts;
using Services;
using System.Collections.Generic;

namespace www.dfide
{
    /// <summary>
    /// Dfide module.
    /// </summary>
    public class DfideModule : IModule
    {
        const string _DfideDomain = "dfide.com";
        static List<string> _DomainAlias = new List<string> { "www.dfide.com"
#if DEBUG
            , "192.168.1.24:5963"
#endif
        };
        const string _DefaultDescription = "Dfide.com...";
        const string _DefaultPageKeywords = "dfide";

        /// <summary>
        /// Register the model.
        /// </summary>
        public static void Register()
        {
            Factory.RegisterModule(_DfideDomain,
                new DfideModule
                {
                    Name = "Dfide",
                    LongName = "Dfide",
                    Domain = _DfideDomain,
                    DomainAlias = _DomainAlias,
                    DefaultDescription = _DefaultDescription,
                    DefaultPageKeywords = _DefaultPageKeywords,
                    UseArea = false,
                    Authentications = new List<ModuleAuthentication>
                    {
                        new ModuleAuthentication
                        {
                            Type = AuthenticationType.ExtLive,
                            //AuthenticationType = "DfideLive",
                            ClientId = "000000004C16D198",
                            ClientSecret = "wAz6Fe53zdGCJW2c7PvOSMUEvSav4Iow",
                            //CallbackPath = "/dfide-signin-live",
                            //SignInAsAuthenticationType = ""
                        },
                        new ModuleAuthentication
                        {
                            Type = AuthenticationType.ExtGoogle,
                            ClientId = "860804842051-or85v6lse4se51lnaggo3t0ln7cpf2ts.apps.googleusercontent.com",
                            ClientSecret = "yudT1qa3apkV-NK9WDGM8XTb",
                            //SignInAsAuthenticationType = ""
                        }
                    },
                    Controllers = new Dictionary<string, ModuleController>
                    {
                        { "home", new ModuleController { Name = "Home", HaveControllerAndView = true, HaveLayout = true } },
                        { "post", new ModuleController { Name = "Post", HaveControllerAndView = true, HaveLayout = true } },
                        { "account", new ModuleController { Name = "Account", HaveControllerAndView = false, HaveLayout = true } },
                        { "manage", new ModuleController { Name = "Manage", HaveControllerAndView = false, HaveLayout = true } },
                    }
                });
        }

        /// <summary>
        /// Module name.
        /// </summary>
        public string Name { get; set; }

        public string LongName { get; set; }

        /// <summary>
        /// Module domain.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Module domain alias.
        /// </summary>
        public IList<string> DomainAlias { get; set; }

        public string DefaultDescription { get; set; }
        public string DefaultPageKeywords { get; set; }

        /// <summary>
        /// Is area based ?
        /// </summary>
        public bool UseArea { get; set; }

        /// <summary>
        /// Modules authentication.
        /// </summary>
        public List<ModuleAuthentication> Authentications { get; set; }

        /// <summary>
        /// Module controllers.
        /// </summary>
        public Dictionary<string, ModuleController> Controllers { get; set; }
    }
}
