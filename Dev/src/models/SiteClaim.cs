using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    /// <summary>
    /// Site claim.
    /// Claim type:
    ///     * region
    ///     * categorie
    ///     * tag
    ///     * group
    /// </summary>
    public class SiteClaim : Claim
    {
        /// <summary>
        /// Parent claim.
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        /// Parent claim.
        /// </summary>
        public SiteClaim Parent { get; set; }
        /// <summary>
        /// Childs claims.
        /// </summary>
        [NotMapped]
        public IEnumerable<SiteClaim> Childs { get; set; }

        /// <summary>
        /// Site.
        /// </summary>
        public int SiteId { get; set; }
        /// <summary>
        /// Site.
        /// </summary>
        [Required]
        public Site Site { get; set; }
    }
}