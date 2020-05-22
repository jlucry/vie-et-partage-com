using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// Post group association.
    /// </summary>
    public class PostGroup
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
        /// Groug id.
        /// </summary>
        public int GroupId { get; set; }
    }
}