using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// Post region association.
    /// </summary>
    public class PostRegion
    {
        /// <summary>
        /// Association id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Post of the association.
        /// </summary>
        [Required]
        public /*virtual*/ Post Post { get; set; }

        /// <summary>
        /// Region id.
        /// 0 for all region.
        /// </summary>
        public int RegionId { get; set; }
    }
}