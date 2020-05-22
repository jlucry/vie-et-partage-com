using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// Post text.
    /// </summary>
    public class PostText
    {
        /// <summary>
        /// Post text id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Post text type.
        /// </summary>
        [Required]
        public string Type { get; set; }

        /// <summary>
        /// Post text title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Post text number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Post text revision.
        /// </summary>
        public int Revision { get; set; }

        /// <summary>
        /// Post text value.
        /// </summary>
        [Required]
        public string Value { get; set; }

        /// <summary>
        /// Post text parent.
        /// </summary>
        public int PostId { get; set; }
        /// <summary>
        /// Post text parent.
        /// </summary>
        [Required]
        public Post Post { get; set; }

        /// <summary>
        /// Post text Site.
        /// </summary>
        public int? SiteId { get; set; }
        /// <summary>
        /// Post text Site.
        /// </summary>
        public Site Site { get; set; }
    }
}