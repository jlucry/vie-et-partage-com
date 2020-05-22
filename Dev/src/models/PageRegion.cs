using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// Page region association.
    /// </summary>
    public class PageRegion
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
        /// Region id.
        /// 0 for all region.
        /// </summary>
        public int RegionId { get; set; }
    }
}