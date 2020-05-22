using Models;
using System.Collections.Generic;

namespace UnitTests.Core
{
    public class SiteData
    {
        public Site site { get; set; }
        public List<SiteClaim> regions { get; set; }
        public List<SiteClaim> nullSiteCats { get; set; }
        public List<SiteClaim> nullSiteTags { get; set; }
        public List<ApplicationUser> users { get; set; }
    }
}
