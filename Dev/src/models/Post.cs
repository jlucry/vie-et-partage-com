// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Models
{
    /// <summary>
    /// Represents a post.
    /// </summary>
    public class Post : ClaimedModel
    {
        /// <summary>
        /// Private access.
        /// When true, access is granted only to users member of the post group.
        /// </summary>
        public bool Private { get; set; }

        /// <summary>
        /// Post cover.
        /// </summary>
        public string Cover { get; set; }

        /// <summary>
        /// Post text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Page creator.
        /// </summary>
        public string CreatorId { get; set; }
        /// <summary>
        /// Post creator.
        /// </summary>
        [Required]
        public virtual ApplicationUser Creator { get; set; }
        /// <summary>
        /// Post creation date.
        /// </summary>
        [Required]
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// Post modified date.
        /// </summary>
        public DateTime? ModifiedDate { get; set; }
        /// <summary>
        /// Post validation date.
        /// </summary>
        public DateTime? ValidationDate { get; set; }

        // Fields on which we're making filtering...
        // {

        /// <summary>
        /// Post highlighted.
        /// </summary>
        public bool Highlight { get; set; }
        /// <summary>
        /// Post start date.
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Post end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        // }

        /// <summary>
        /// Post claims
        /// </summary>
        public ICollection<PostClaim> PostClaims { get; set; }
#       if !DENORMALIZE
        /// <summary>
        /// Post groups.
        /// </summary>
        public /*virtual*/ ICollection<PostGroup> PostGroups { get; set; }
        /// <summary>
        /// Post regions.
        /// </summary>
        public /*virtual*/ ICollection<PostRegion> PostRegions { get; set; }
        /// <summary>
        /// Post categories.
        /// </summary>
        public /*virtual*/ ICollection<PostCategory> PostCategorys { get; set; }
        /// <summary>
        /// Post tags.
        /// </summary>
        public /*virtual*/ ICollection<PostTag> PostTags { get; set; }
#       else
        /// <summary>
        /// Post groups.
        /// </summary>
        public int Group1 { get; set; }
        public int Group2 { get; set; }
        public int Group3 { get; set; }
        public int Group4 { get; set; }
        public int Group5 { get; set; }
        public int Group6 { get; set; }
        public int Group7 { get; set; }
        public int Group8 { get; set; }
        public int Group9 { get; set; }
        public int Group10 { get; set; }
        /// <summary>
        /// Post regions.
        /// </summary>
        public int Region1 { get; set; }
        public int Region2 { get; set; }
        public int Region3 { get; set; }
        public int Region4 { get; set; }
        public int Region5 { get; set; }
        public int Region6 { get; set; }
        public int Region7 { get; set; }
        public int Region8 { get; set; }
        public int Region9 { get; set; }
        public int Region10 { get; set; }
        /// <summary>
        /// Post categories.
        /// </summary>
        public int Category1 { get; set; }
        public int Category2 { get; set; }
        public int Category3 { get; set; }
        public int Category4 { get; set; }
        public int Category5 { get; set; }
        public int Category6 { get; set; }
        public int Category7 { get; set; }
        public int Category8 { get; set; }
        public int Category9 { get; set; }
        public int Category10 { get; set; }
        /// <summary>
        /// Post tags.
        /// </summary>
        public int Tag1 { get; set; }
        public int Tag2 { get; set; }
        public int Tag3 { get; set; }
        public int Tag4 { get; set; }
        public int Tag5 { get; set; }
        public int Tag6 { get; set; }
        public int Tag7 { get; set; }
        public int Tag8 { get; set; }
        public int Tag9 { get; set; }
        public int Tag10 { get; set; }
#       endif

        /// <summary>
        /// Post texts.
        /// </summary>
        public ICollection<PostText> PostTexts { get; set; }

        /// <summary>
        /// Post files.
        /// </summary>
        public ICollection<PostFile> PostFiles { get; set; }

        /// <summary>
        /// Post Site.
        /// </summary>
        public int SiteId { get; set; }
        /// <summary>
        /// Post Site.
        /// </summary>
        [Required]
        public Site Site { get; set; }

        /// <summary>
        /// Site on wich the post has been retrieved.
        /// </summary>
        [NotMapped]
        public Site RequestSite { get; set; }
    }
}