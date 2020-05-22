using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Models
{
    /// <summary>
    /// Represents a site.
    /// 
    /// Vu des données:
    /// ---------------
    /// [X] Le Site contient des Catégories:
    /// [X]     Une Catégorie peut être enfant d'une autre Catégorie.
    /// [X] Le Site contient des Mot-clefs:
    /// [X]     Un seul niveau.
    /// [ ] Le Site contient des Posts:
    /// [X]     Un Post est caractérisé par:
    /// [X]         un id, un titre, un texte, 
    /// [X]         un niveau de confidentialité,
    /// [X]         un etat (non validé, validé, en poubelle),
    /// [X]         une date de derniere modification (validation compris), 
    /// [X]         id du createur
    /// [ ]     Un Post est associé à zero\une\des Catégories (voir Metas).
    /// [ ]     Un Post est associé à zero\un\des Mot-clefs (voir Metas).
    /// [ ]     Un Post est associé à zero\un\des Metas.
    /// [X]         Un Méta permet l'extention d'un Post et apporte des information complémentaires sur:
    /// [X]             * Les catégories
    /// [X]             * Les mot-clefs.
    /// [X]             * Les régions.
    /// [X]             * Une oeuvre.
    /// [X]             * Un événement.
    /// [X]             * Des fichiers.
    /// [X]             * Historique de modification et de validation (Quoi, quand et qui).
    /// 
    /// Vue du site:
    /// ------------
    /// Questions:
    ///     * UTILISATION DE SOUS DOMAINE POUR LES REGIONS ?
    ///        => NON: Utilisation de route avec régions (et niveau).
    /// Caracteristique du Wcms:
    ///     * Site publique: MVC pour le referencement.
    ///     * Site privé: Angular + web api.
    ///     * Chaque module definie ces routes, ces controlleurs et ces vues.
    ///     * Pour les vues, le theme\css est définie par le site.
    ///     * Par page web, un controleur et des vues (ou des Views Components). 
    ///     * Tous les controleurs derive d'un seul et unique controlleur qui à la charge de 
    ///       charger le model (Post et ces Metas).
    ///       Les controlleurs dérivées peuvent définire un filtre sur les métas à chargé. 
    /// Le Site contient des Pages:
    ///     Une Page peut être enfant d'une autre Page.
    ///     Une Page est caractérisée par:
    ///         un id, un titre, 
    ///         zéro\une\des région(s)
    ///             une url definie automatique: /{titre} s'il n'y a pas de région, /{region}/{titre} s'il y en a,
    ///         sa présence dans la navigation du site,
    ///         une route qui definie la vue et le controleur à utiliser (page asp.net MVC ou page html avec javascript),
    ///         un filtre qui definie le Models. Le model est fournie par le backend (web api, ou object pour du asp.net mvc),
    ///         ?? un type (page d'affichage d'un post, autre) ??
    ///     Une route peut être dynamique:
    ///         Le titre et l'identifiant du model est spécifié avec l'url de la page.
    ///         Example:
    ///             {url de page}\{le titre du post encodé pour les url}\12: La page affichera le post d'id 12.
    ///         ou {url de page}\
    /// Vue de la database:
    /// -------------------
    ///     Metas:
    ///         un id, un id\type d'application, une valeur numerique, une date, une chaine de caracteres.
    /// 
    /// A voir\utiliser\connaitre (Attention, lien pour la 6.0.0-beta7):
    ///     * Localization: https://github.com/aspnet/Mvc/tree/6.0.0-beta7/test/WebSites/LocalizationWebSite
    ///     * Custom ViewEngines: https://github.com/aspnet/Mvc/tree/6.0.0-beta7/test/WebSites/CompositeViewEngineWebSite
    ///     * Controller discovery conventions: https://github.com/aspnet/Mvc/tree/6.0.0-beta7/test/WebSites/ControllerDiscoveryConventionsWebSite 
    ///     * Controllers from services class library: 
    ///         Classes: https://github.com/aspnet/Mvc/tree/6.0.0-beta7/test/WebSites/ControllersFromServicesClassLibrary
    ///         Web site: https://github.com/aspnet/Mvc/blob/6.0.0-beta7/test/WebSites/ControllersFromServicesWebSite/Startup.cs
    ///     * Content function: https://msdn.microsoft.com/en-us/library/system.web.mvc.controller.content(v=vs.118).aspx#M:System.Web.Mvc.Controller.Content%28System.String%29
    ///     * Cors policy:
    ///         With middleware: https://github.com/aspnet/Mvc/tree/6.0.0-beta7/test/WebSites/CorsWebSite
    ///         With configuration: https://github.com/aspnet/Mvc/tree/6.0.0-beta7/test/WebSites/CorsMiddlewareWebSite
    ///     * Routes:
    ///         Injects route data: https://github.com/aspnet/Mvc/tree/6.0.0-beta7/test/WebSites/CustomRouteWebSite
    ///         Use conventional and attribute routes: https://github.com/aspnet/Mvc/blob/6.0.0-beta7/test/WebSites/RoutingWebSite/Startup.cs
    ///     * Precompilation of razor pages:
    ///         https://github.com/aspnet/Mvc/tree/6.0.0-beta7/test/WebSites/PrecompilationWebSite
    /// </summary>
    public class Site : ClaimedModel
    {
        /// <summary>
        /// Site domain.
        /// </summary>
        [MaxLength(512)] //Work arround to not have varchar(255) on mySql!
        public string Domain { get; set; }

        /// <summary>
        /// Has regions.
        /// </summary>
        public bool HasRegions { get; set; }

        /// <summary>
        /// Private site:
        /// When true,
        ///     * Access granted only to authenticated user.
        ///     * Registration is disabled (new users can only be invited by an administrator).
        /// </summary>
        public bool Private { get; set; }

        /// <summary>
        /// Did public registration is enabled.
        /// </summary>
        public bool IsPublicRegistration { get; set; }
		
		/// <summary>
        /// Did we lockout user on authentication failure.
        /// </summary>
		public bool LockoutUserOnFailure { get; set; }

        /// <summary>
        /// Site claims:
        ///     * group
        ///     * region
        ///     * categorie
        ///     * tag
        /// </summary>
        public virtual ICollection<SiteClaim> SiteClaims { get; set; }

        /// <summary>
        /// Site actions.
        /// </summary>
        public virtual ICollection<SiteAction> SiteActions { get; set; }

        /// <summary>
        /// Site pages.
        /// </summary>
        public virtual ICollection<Page> Pages { get; set; }

        /// <summary>
        /// Site posts.
        /// </summary>
        public virtual ICollection<Post> Posts { get; set; }
    }
}