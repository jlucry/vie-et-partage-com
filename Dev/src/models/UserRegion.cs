using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// User region association.
    /// </summary>
    public class UserRegion
    {
        /// <summary>
        /// Association id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// User of the association.
        /// </summary>
        [Required]
        public /*virtual*/ ApplicationUser User { get; set; }

        /// <summary>
        /// Region id.
        /// 0 for all region.
        /// </summary>
        public int RegionId { get; set; }
    }
}