using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// Page tag association.
    /// </summary>
    public class PageTag
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
        /// Tag id.
        /// </summary>
        public int TagId { get; set; }
    }
}