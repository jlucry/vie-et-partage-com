// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
#       if !DENORMALIZE
        NotImplemented;
#       else
        /// <summary>
        /// User groups.
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
        /// User regions.
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
#endif

        /// <summary>
        /// Enabled.
        /// </summary>
        public bool Enabled { get; set; }
        
		/// <summary>
        /// User picture.
        /// </summary>
        public string Cover { get; set; }
		
        /// <summary>
        /// Page Site.
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Site on wich the post has been retrieved.
        /// </summary>
        [NotMapped]
        public Site RequestSite { get; set; }

        /// <summary>
        /// Navigation property for the claims this user possesses.
        /// </summary>
        public virtual ICollection<IdentityUserClaim<string>> Claims { get; } = new List<IdentityUserClaim<string>>();
    }
}
