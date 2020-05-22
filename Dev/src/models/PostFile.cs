using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    /// <summary>
    /// Post file.
    /// File can be added to post by any one that have contribution access.
    /// </summary>
    public class PostFile
    {
        /// <summary>
        /// Post file id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Post file type:
        ///     * cover
        ///     * url
        ///     * file
        /// </summary>
        [Required]
        public string Type { get; set; }

        /// <summary>
        /// Post file title.
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Post file Url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Post file creator.
        /// </summary>
        public string CreatorId { get; set; }
        /// <summary>
        /// Post file creator.
        /// </summary>
        [Required]
        public ApplicationUser Creator { get; set; }
        /// <summary>
        /// Post file creation date.
        /// </summary>
        [Required]
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// Post file modified date.
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Post file parent.
        /// </summary>
        public int? PostId { get; set; }
        /// <summary>
        /// Post file parent.
        /// </summary>
        public Post Post { get; set; }

        /// <summary>
        /// Post file Site.
        /// </summary>
        public int? SiteId { get; set; }
        /// <summary>
        /// Post file Site.
        /// </summary>
        public Site Site { get; set; }
    }
}