using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// User group association.
    /// </summary>
    public class UserGroup
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
        /// Groug id.
        /// </summary>
        public int GroupId { get; set; }
    }
}