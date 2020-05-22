using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// Post category association.
    /// </summary>
    public class PostCategory
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
        /// Category id.
        /// </summary>
        public int CategoryId { get; set; }
    }
}