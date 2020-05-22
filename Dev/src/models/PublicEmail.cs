using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class PublicEmail
    {
        /// <summary>
        /// Model id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// User email.
        /// </summary>
        [Required]
        public string Email { get; set; }

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

        /// <summary>
        /// Enabled.
        /// </summary>
        public bool Enabled { get; set; }
		
        /// <summary>
        /// Page Site.
        /// </summary>
        public int SiteId { get; set; }
    }
}
