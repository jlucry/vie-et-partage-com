using Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Services
{
    /// <summary>
    /// Json site.
    /// </summary>
    public class JsonSite
    {
        /// <summary>
        /// Json site contructor.
        /// </summary>
        /// <param name="post"></param>
        public JsonSite(Site site)
        {
            // Set...
            if (site != null)
            {
                Id = site?.Id ?? 0;
                Title = site?.Title;
                State = site?.State ?? State.NotValided;
                Domain = site?.Domain;
                HasRegions = site?.HasRegions ?? false;
                Private = site?.Private ?? true;
                if (site.SiteClaims != null)
                {
                    SiteClaims = new List<JsonSiteClaim>();
                    foreach (SiteClaim claim in site.SiteClaims)
                    {
                        SiteClaims.Add(new JsonSiteClaim(claim));
                    }
                }
            }
        }

        /// <summary>
        /// Site id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Site title.
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Site state.
        /// </summary>
        public State State { get; set; }

        /// <summary>
        /// Site domain.
        /// </summary>
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
        /// Site claims:
        ///     * group
        ///     * region
        ///     * categorie
        ///     * tag
        /// </summary>
        public List<JsonSiteClaim> SiteClaims { get; set; }
    }
}