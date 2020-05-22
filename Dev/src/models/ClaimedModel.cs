using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Models
{
    /// <summary>
    /// Claim based Models.
    /// </summary>
    public abstract class ClaimedModel
    {
        /// <summary>
        /// Model id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Model title.
        /// </summary>
        [Required]
        //[Column(TypeName = "text")]
        //[MaxLength(512)] //Work arround to not have varchar(255) on mySql!
        public string Title { get; set; }

        /// <summary>
        /// Model state.
        /// </summary>
        public State State { get; set; }
    }
}