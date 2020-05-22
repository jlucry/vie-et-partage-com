using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    /// <summary>
    /// Claim.
    /// </summary>
    public class Claim
    {
        /// <summary>
        /// Claim id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Claim type:
        /// </summary>
        [Required]
        //[MaxLength(255)]
        public string Type { get; set; }

        /// <summary>
        /// Claim value.
        /// </summary>
        public int? Value { get; set; }

        /// <summary>
        /// Claim string value.
        /// </summary>
        //[Column(TypeName = "TEXT")]
        //[MaxLength(50000)] //Work arround to not have varchar(255) on mySql!
        public string StringValue { get; set; }

        /// <summary>
        /// Claim DateTime value.
        /// </summary>
        public DateTime? DateTimeValue { get; set; }
    }
}