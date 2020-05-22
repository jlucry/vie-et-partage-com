using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// Post tag association.
    /// </summary>
    public class PostTag
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
        /// Tag id.
        /// </summary>
        public int TagId { get; set; }
    }
}