using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// Page claim.
    /// </summary>
    public class PageClaim : Claim
    {
        /// <summary>
        /// Page.
        /// </summary>
        public int PageId { get; set; }
        /// <summary>
        /// Page.
        /// </summary>
        [Required]
        public Page Page { get; set; }

        /// <summary>
        /// Site id.
        /// </summary>
        public int? SiteId { get; set; }
        /// <summary>
        /// Site.
        /// </summary>
        //[Required]
        public Site Site { get; set; }
    }
}