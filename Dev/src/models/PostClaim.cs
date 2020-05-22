using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// Post claim.
    /// </summary>
    public class PostClaim : Claim
    {
        /// <summary>
        /// Post.
        /// </summary>
        public int PostId { get; set; }
        /// <summary>
        /// Post.
        /// </summary>
        [Required]
        public Post Post { get; set; }

        /// <summary>
        /// Site.
        /// </summary>
        public int? SiteId { get; set; }
        /// <summary>
        /// Site.
        /// </summary>
        public Site Site { get; set; }
    }
}