using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// Page group association.
    /// </summary>
    public class PageGroup
    {
        /// <summary>
        /// Association id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Page of the association.
        /// </summary>
        [Required]
        public virtual Page Page { get; set; }

        /// <summary>
        /// Groug id.
        /// </summary>
        public int GroupId { get; set; }

        ///// <summary>
        ///// Site claim of the association.
        ///// </summary>
        //public virtual SiteClaim SiteClaim { get; set; }
    }
}