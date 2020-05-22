using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Admin site settings.
    /// </summary>
    public class JsonSiteSettings
    {
        public string Name { get; set; }
        public List<JsonAdminMenu> Menus { get; set; }
        public List<JsonSiteClaim> Regions { get; set; }
        public List<JsonSiteClaim> Categories { get; set; }
        public List<JsonSiteClaim> Tags { get; set; }
        public List<JsonSiteClaim> Groups { get; set; }
        public List<string> UserRoles { get; set; }
        public string UserName { get; set; }
        public string UserImg { get; set; }
    }
}
