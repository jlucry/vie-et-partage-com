using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Admin app pages.
    /// </summary>
    public class JsonAdminMenu
    {
        public bool SitePage { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string IconMat { get; set; }
        public string Url { get; set; }
        public string UrlMat { get; set; }
        public string Active { get; set; }
        public List<JsonAdminMenu> Childs { get; set; }
        public Dictionary<string, string> DefaultFilters { get; set; }
    }
}
