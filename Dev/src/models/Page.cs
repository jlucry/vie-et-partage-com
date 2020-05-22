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
    /// Represents a page.
    /// </summary>
    public class Page : ClaimedModel
    {
        /// <summary>
        /// Private access.
        /// When true, access is granted only to users member of the page group.
        /// </summary>
        public bool Private { get; set; }

        /// <summary>
        /// Position in the navigation tree.
        /// 0 if the page should not be inserted in the navigation tree.
        /// </summary>
        public int PositionInNavigation { get; set; }

        /// <summary>
        /// Page controller.
        /// </summary>
        [Required]
        public string Controller { get; set; }

        /// <summary>
        /// Page action.
        /// </summary>
        [Required]
        public string Action { get; set; }

        /// <summary>
        /// Page claims.
        /// </summary>
        public ICollection<PageClaim> PageClaims { get; set; }
#       if !DENORMALIZE
        /// <summary>
        /// Page groups.
        /// </summary>
        public ICollection<PageGroup> PageGroups { get; set; }
        /// <summary>
        /// Page regions.
        /// </summary>
        public ICollection<PageRegion> PageRegions { get; set; }
        /// <summary>
        /// Page categories.
        /// </summary>
        public ICollection<PageCategory> PageCategorys { get; set; }
        /// <summary>
        /// Page tags.
        /// </summary>
        public ICollection<PageTag> PageTags { get; set; }
#       else
        /// <summary>
        /// Page groups.
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
        /// Page regions.
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
        /// Page categories.
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
        /// Page tags.
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
        /// Page creator.
        /// </summary>
        public string CreatorId { get; set; }
        /// <summary>
        /// Page creator.
        /// </summary>
        [Required]
        public ApplicationUser Creator { get; set; }
        /// <summary>
        /// Page creation date.
        /// </summary>
        [Required]
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// Page modified date.
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Parent page.
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        /// Parent page.
        /// </summary>
        public Page Parent { get; set; }
        /// <summary>
        /// Childs pages.
        /// </summary>
        [NotMapped]
        public IEnumerable<Page> Childs { get; set; }

        /// <summary>
        /// Page Site.
        /// </summary>
        public int SiteId { get; set; }
        /// <summary>
        /// Page Site.
        /// </summary>
        [Required]
        public Site Site { get; set; }

        /// <summary>
        /// Site on wich the page has been retrieved.
        /// </summary>
        [NotMapped]
        public Site RequestSite { get; set; }
    }
}