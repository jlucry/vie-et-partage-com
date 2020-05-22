using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// Page category association.
    /// </summary>
    public class PageCategory
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
        /// Category id.
        /// </summary>
        public int CategoryId { get; set; }
    }
}