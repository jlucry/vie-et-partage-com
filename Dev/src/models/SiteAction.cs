using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// Site action.
    /// </summary>
    public class SiteAction
    {
        /// <summary>
        /// Action id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Action table.
        /// </summary>
        [Required]
        public string Table { get; set; }

        /// <summary>
        /// Action element Id.
        /// </summary>
        public string Element { get; set; }

        /// <summary>
        /// Action type:
        ///     Modification
        ///     Validation
        ///     Trashed
        /// </summary>
        [Required]
        public string Type { get; set; }

        /// <summary>
        /// Post file creator.
        /// </summary>
        public string ActorId { get; set; }
        /// <summary>
        /// Actor.
        /// </summary>
        [Required]
        public ApplicationUser Actor { get; set; }

        /// <summary>
        /// Action date.
        /// </summary>
        [Required]
        public DateTime ActionDate { get; set; }

        /// <summary>
        /// Action description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Post file Site.
        /// </summary>
        public int SiteId { get; set; }
        /// <summary>
        /// Action site.
        /// </summary>
        [Required]
        public Site Site { get; set; }
    }
}