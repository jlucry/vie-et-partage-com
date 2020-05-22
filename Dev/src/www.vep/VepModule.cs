using Contracts;
using Services;
using System.Collections.Generic;

namespace www.vep
{
    /// <summary>
    /// Vep module.
    /// </summary>
    public class VepModule : IModule
    {
        const string _VepDomain = "vieetpartage.com";
        static List<string> _DomainAlias = new List<string> { "www.vieetpartage.com", "www2.vieetpartage.com"
//#if DEBUG
            , "localhost:5963", "192.168.1.47:5963"
//#endif
        };
        const string _DefaultDescription = "Association catholique créée pour répondre à la soif d’une catéchèse vivante pour adultes, basée sur la lecture et la pratique de la Parole de Dieu, la conversion du coeur, la pratique du service dans l’église...";
        const string _DefaultPageKeywords = "vie et partage, vie, partage, conversion, retraite, pelerinage, priere, dieu, jesus, marie, saintesprit, saint, bible, catholique, eglise, chretien, delivrance, rosaire, chapelet, verite, juste, temoignage, enseignement";

        //const string _VepDomain = "localhost:5000";

        /// <summary>
        /// Register the model.
        /// </summary>
        public static void Register()
        {
            // Register the module...
            Factory.RegisterModule(_VepDomain,
                new VepModule
                {
                    Name = "Vep",
                    LongName = "Vie et Partage",
                    Domain = _VepDomain,
                    DomainAlias = _DomainAlias,
                    DefaultDescription = _DefaultDescription,
                    DefaultPageKeywords = _DefaultPageKeywords,
                    UseArea = true,
                    Authentications = new List<ModuleAuthentication>
                    {
                        new ModuleAuthentication
                        {                            
                            Type = AuthenticationType.ExtLive,
                            //AuthenticationType = "VepLive",
                            ClientId = "0000000048174AF6",
                            ClientSecret = "hoRqbtGu-Q58ntgaPn9t22WiLPWg2uKf",
                            //CallbackPath = "/vep-signin-live",
                            SignInAsAuthenticationType = ""
                        },
                        new ModuleAuthentication
                        {
                            Type = AuthenticationType.ExtGoogle,
                            ClientId = "860804842051-1tg1askm4b055hjv3sqjp8sl2bmlga92.apps.googleusercontent.com",
                            ClientSecret = "CJamm1OkArKnXLrFYIiuvd98",
                            SignInAsAuthenticationType = ""
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
